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
// 1. JWT SECRET (env var > appsettings fallback)
// ─────────────────────────────────────────
var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET")
                ?? builder.Configuration["Jwt:SecretKey"]
                ?? "Testing_Secret_Key_For_BeachGo_2026_MinLength32Chars!";

if (jwtSecret.Length < 32)
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");

// ─────────────────────────────────────────
// 2. DATABASE
// ─────────────────────────────────────────
var dbConn = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? "Data Source=beachrehberi.db";

builder.Services.AddDbContext<BeachDbContext>(options =>
    options.UseSqlite(dbConn));

// ─────────────────────────────────────────
// 3. CORE INFRASTRUCTURE
// ─────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// ─────────────────────────────────────────
// 4. APPLICATION SERVICES (tüm DI kayıtları)
// ─────────────────────────────────────────
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// IGeoCalculator – BeachService constructor dependency (düzeltildi)
builder.Services.AddSingleton<IGeoCalculator, GeoCalculator>();

builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// WeatherService – typed HttpClient
builder.Services.AddHttpClient<IWeatherService, WeatherService>();

// ─────────────────────────────────────────
// 5. MAPSTER – IMapper DI (ReservationService dependency)
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
// 8. RATE LIMITING (controllers [EnableRateLimiting] bekliyor)
// ─────────────────────────────────────────
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 60;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
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
// AUTO MIGRATE
// ─────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();
    await db.Database.MigrateAsync();

    // Seed data
    if (!await db.Beaches.AnyAsync())
    {
        var beach = new BeachRehberi.API.Models.Beach("Test Plajı", "Test açıklaması", "Test Adresi", 36.8785, 30.6657);
        db.Beaches.Add(beach);
        await db.SaveChangesAsync();
        Console.WriteLine("Seed data: Test beach created with ID: " + beach.Id);
    }

    // Seed admin user
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
        Console.WriteLine("Seed data: Admin user created");
    }

    // Seed normal user
    if (!await db.BusinessUsers.AnyAsync(u => u.Email == "user@beachgo.com"))
    {
        var normalUser = new BeachRehberi.API.Models.BusinessUser(
            "user@beachgo.com",
            BCrypt.Net.BCrypt.HashPassword("User123!"),
            BeachRehberi.API.Models.UserRoles.User
        );
        normalUser.UpdateProfile("Normal User", "BeachGo User");
        db.BusinessUsers.Add(normalUser);
        await db.SaveChangesAsync();
        Console.WriteLine("Seed data: Normal user created");
    }
}

app.Run();
