using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using BeachRehberi.API.Middlewares;
using BeachRehberi.API.Validators;
using BeachRehberi.API.Mappings;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using BCrypt.Net;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────
// 1. JWT SECRET (Must be provided in production)
// ─────────────────────────────────────────
var jwtSecret = builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrEmpty(jwtSecret) || (builder.Environment.IsProduction() && jwtSecret.Contains("Testing_Secret_Key")))
{
    throw new InvalidOperationException("Production level JWT Secret Key is MISSING or INSECURE. Please set Jwt:SecretKey in environment variables or appsettings.json.");
}

if (jwtSecret.Length < 32)
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");

// ... (Database section) ...
var dbConn = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? "Data Source=beachrehberi.db";

builder.Services.AddDbContext<BeachDbContext>(options => {
    options.UseSqlite(dbConn);
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

// ... (other infrastructure) ...
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ... (DI registrations) ...
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddSingleton<IGeoCalculator, GeoCalculator>();
builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

// ─────────────────────────────────────────
// 5. MAPSTER – IMapper DI
// ─────────────────────────────────────────
MapsterConfig.Register();
var mapsterConfig = TypeAdapterConfig.GlobalSettings;
mapsterConfig.Scan(typeof(Program).Assembly);
builder.Services.AddSingleton(mapsterConfig);
builder.Services.AddScoped<IMapper, ServiceMapper>();

// ─────────────────────────────────────────
// 6. MEDIATR
// ─────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// ─────────────────────────────────────────
// 7. FLUENT VALIDATION
// ─────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// ─────────────────────────────────────────
// 8. RATE LIMITING (Production Grade)
// ─────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Global Sliding Window - preventing burst and sustained abuse
    options.AddSlidingWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 4;
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
    });

    // Auth endpoints - stricter
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });
});

// ─────────────────────────────────────────
// 9. JWT AUTHENTICATION
// ─────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "BeachRehberi.API",
        ValidAudience = "BeachRehberi.App",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// ─────────────────────────────────────────
// 10. SWAGGER + JWT AUTHORIZE BUTONU
// ─────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BeachRehberi API",
        Version = "v1",
        Description = "BeachGo – Plaj Rehberi REST API"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token değerini girin (Bearer prefix otomatik eklenir).\nÖrnek: eyJhbGciOiJIUzI1NiIs..."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ─────────────────────────────────────────
// 11. CORS
// ─────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        // Production domains - uncomment for production
        // policy.WithOrigins("https://beachgo.com", "https://www.beachgo.com")

        // Development domains (HTTP + HTTPS)
        policy.WithOrigins(
                "http://localhost:3000",
                "https://localhost:3000",
                "http://localhost:5173",
                "https://localhost:5173",
                "http://192.168.1.6:3000",
                "https://192.168.1.6:3000",
                "http://192.168.1.6:5173",
                "https://192.168.1.6:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// ─────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────
// MIDDLEWARE PIPELINE (sıralama kritik!)
// ─────────────────────────────────────────
// HTTPS redirect: production ortamında enable et
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeachRehberi API v1");
    c.RoutePrefix = "swagger";
    c.DisplayRequestDuration();
});

// CORS: Authentication'dan ÖNCE olmalı
app.UseCors("AllowFrontend");
app.UseRateLimiter();

// Exception handling: en erken
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<JwtBlacklistMiddleware>();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ─────────────────────────────────────────
// AUTO MIGRATE (Development Only)
// ─────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();
    try 
    {
        await db.Database.MigrateAsync();

        // Seed data
        if (!await db.Beaches.AnyAsync())
        {
            var beach = new BeachRehberi.API.Models.Beach("Test Plajı", "Test açıklaması", "Test Adresi", 36.8785, 30.6657, 0);
            db.Beaches.Add(beach);
            await db.SaveChangesAsync();
            Console.WriteLine("Seed data: Test beach created.");
        }

        // Admin user seed
        if (!await db.BusinessUsers.AnyAsync(u => u.Email == "admin@beachgo.com"))
        {
            var adminUser = new BeachRehberi.API.Models.BusinessUser(
                "admin@beachgo.com",
                BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                BeachRehberi.API.Models.UserRoles.Admin
            );
            adminUser.UpdateProfile("Admin User", "BeachGo Admin");
            db.BusinessUsers.Add(adminUser);
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during migration: {ex.Message}");
    }
}
else 
{
    // Production validation: Check if DB is reachable but don't migrate
    Console.WriteLine("Production mode: Skipping auto-migrations. Ensure migrations are applied via CI/CD.");
}

app.Run();
