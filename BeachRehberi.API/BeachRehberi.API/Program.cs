using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using BeachRehberi.API.Middlewares;
using BeachRehberi.API.Validators;
using BeachRehberi.API.Mappings;
using BeachRehberi.API.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using MapsterMapper;
using MediatR;
using System.Threading.RateLimiting;
using BCrypt.Net;
using System.Security.Claims;
using BeachRehberi.API.Models;
using Resend;

using BeachRehberi.Domain.Interfaces;
using BeachRehberi.Application;

var builder = WebApplication.CreateBuilder(args);


// ─────────────────────────────────────────
// 1. JWT SECRET (Must be provided in production)
// ─────────────────────────────────────────
var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET")
                ?? builder.Configuration["Jwt:SecretKey"];

if (string.IsNullOrEmpty(jwtSecret) || (builder.Environment.IsProduction() && jwtSecret.Contains("Testing_Secret_Key")))
{
    throw new InvalidOperationException("Production level JWT Secret Key is MISSING or INSECURE. Please set Jwt:SecretKey in environment variables or appsettings.json.");
}

if (jwtSecret.Length < 32)
    throw new InvalidOperationException("JWT Secret must be at least 32 characters long.");

// ─────────────────────────────────────────
// DATABASE CONFIGURATION (SQLite)
// ─────────────────────────────────────────
var dbConn = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? "Data Source=beachgo.db";
builder.Services.AddDbContext<BeachDbContext>(options =>
{
    // PostgreSQL entegrasyonu
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

builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

// Auth support services (OTP + Email)
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<BeachRehberi.Application.Common.Interfaces.IOtpService, OtpService>();
// Email service: use NoOp in dev (no Resend API key), ResendEmailService in production
var resendApiKey = builder.Configuration["Resend:ApiKey"];
if (!string.IsNullOrWhiteSpace(resendApiKey))
{
    builder.Services.Configure<ResendClientOptions>(o => o.ApiToken = resendApiKey);
    builder.Services.AddHttpClient<IResend, ResendClient>();
    builder.Services.AddScoped<IEmailService, ResendEmailService>();
    builder.Services.AddScoped<BeachRehberi.Application.Common.Interfaces.IEmailService, ResendEmailService>();
}
else
{
    builder.Services.AddScoped<IEmailService, NoOpEmailService>();
    builder.Services.AddScoped<BeachRehberi.Application.Common.Interfaces.IEmailService, NoOpEmailService>();
}
builder.Services.AddScoped<IGuestReservationService, GuestReservationService>();
builder.Services.AddScoped<IPaymentService, MockPaymentService>();

// Provider Configurations
// ...
// (rest of DI)

// ─────────────────────────────────────────
// 6. MEDIATR
// ─────────────────────────────────────────
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
    cfg.RegisterServicesFromAssembly(typeof(BeachRehberi.Application.DependencyInjection).Assembly);
});

// ─────────────────────────────────────────
// 7. FLUENT VALIDATION
// ─────────────────────────────────────────
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
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
        opt.PermitLimit = 20;
        opt.QueueLimit = 0;
    });
});

// ─────────────────────────────────────────
// 9. JWT AUTHENTICATION & AUTHORIZATION POLICIES
// ─────────────────────────────────────────
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "BeachRehberi.API",
        ValidAudience = "BeachRehberi.App",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role
    };
});

builder.Services.AddAuthorization(options =>
{
    // Global fallback for robust safety
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();

    // Custom Policies definition that were missing
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(UserRoles.Admin));
    options.AddPolicy("BusinessOnly", policy => policy.RequireRole(UserRoles.Business, UserRoles.Admin));
});

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
        if (builder.Environment.IsProduction())
        {
            policy.WithOrigins(
                    "https://beachgo.com",
                    "https://www.beachgo.com")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
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
        }
    });
});

// ─────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────
// MIDDLEWARE PIPELINE (sıralama kritik!)
// ─────────────────────────────────────────

app.UseForwardedHeaders(); // Proxy/Nginx reverse yönlendirmeleri için gerekli

// Security, HTTPS redirect & HSTS: Production korumaları
if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.Append("X-Content-Type-Options", "nosniff");
    headers.Append("X-Frame-Options", "DENY");
    headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeachRehberi API v1");
        c.RoutePrefix = "swagger";
        c.DisplayRequestDuration();
    });
}

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
    try
    {
        await db.Database.MigrateAsync();

        // Seed data
        if (!await db.Beaches.AnyAsync())
        {
            var beaches = new List<BeachRehberi.API.Models.Beach>
            {
                new("Konyaalti Plaji", "Antalya'nin en gozde cakil plaji. Berrak mavi sulari ve uzun sahil seridiyle mukemmel bir tatil deneyimi sunar.", "Konyaalti, Antalya", 36.8784, 30.6657, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "08:00", CloseTime = "20:00",
                    HasSunbeds = true, HasShower = true, HasParking = true, HasWifi = true, HasBar = true, IsChildFriendly = true,
                    OccupancyPercent = 65, Capacity = 2000, IsOpen = true, SunbedPrice = 150
                },
                new("Lara Plaji", "Antalya'nin incisi Lara Plaji, ince kum ve turkuaz sulari ile ziyaretcilerini buyuluyor.", "Lara, Antalya", 36.8469, 30.7843, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "07:30", CloseTime = "21:00",
                    HasSunbeds = true, HasRestaurant = true, HasParking = true, HasWaterSports = true, IsChildFriendly = true,
                    OccupancyPercent = 80, Capacity = 3000, IsOpen = true, SunbedPrice = 200, HasEntryFee = true, EntryFee = 50
                },
                new("Mermerli Plaji", "Antalya Kaleici'nde tarihi dokularla cevrili, mermer kayaliklariyla benzersiz bir koy.", "Kaleici, Antalya", 36.8825, 30.7056, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1476673160081-cf065607f449?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "09:00", CloseTime = "19:00",
                    HasSunbeds = true, HasBar = true, HasShower = true,
                    OccupancyPercent = 45, Capacity = 500, IsOpen = true, SunbedPrice = 250, HasEntryFee = true, EntryFee = 100
                },
                new("Adrasan Plaji", "Kumluca'ya bagli sakin ve dogal guzelligini koruyan essiz bir koy.", "Adrasan, Antalya", 36.3451, 30.4712, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "08:00", CloseTime = "19:30",
                    HasShower = true, IsChildFriendly = true,
                    OccupancyPercent = 30, Capacity = 800, IsOpen = true
                },
                new("Phaselis Plaji", "Antik liman kalintilari arasinda tarihe dokunan, uc koylu muhtesem plaj.", "Kemer, Antalya", 36.5204, 30.5549, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1510414842594-a61c69b5ae57?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "08:30", CloseTime = "18:30",
                    HasParking = true, IsChildFriendly = true, HasShower = true,
                    OccupancyPercent = 55, Capacity = 1200, IsOpen = true, HasEntryFee = true, EntryFee = 120
                },
                new("Oludeniz Lagunu", "Turkiye'nin en fotograflanan noktasi, masmavi lagunu ve milli park statusuyle essiz guzellik.", "Oludeniz, Fethiye", 36.5500, 29.1167, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1520454974749-611b7248ffdb?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "07:00", CloseTime = "21:00",
                    HasSunbeds = true, HasBar = true, HasWaterSports = true, HasParking = true, IsChildFriendly = true, HasRestaurant = true, HasWifi = true,
                    OccupancyPercent = 90, Capacity = 5000, IsOpen = true, SunbedPrice = 300, HasEntryFee = true, EntryFee = 75
                },
            };

            // Seed rating'ler (UpdateRating private set oldugundan reflection ile)
            var ratingsAndCounts = new[] { (4.6, 287), (4.8, 512), (4.3, 94), (4.9, 156), (4.5, 203), (4.7, 1024) };
            for (int i = 0; i < beaches.Count; i++)
                beaches[i].UpdateRating(ratingsAndCounts[i].Item1, ratingsAndCounts[i].Item2);

            db.Beaches.AddRange(beaches);
            await db.SaveChangesAsync();
            Console.WriteLine("Seed data: 6 beaches created.");
        }

        // Admin user seed
        if (!await db.BusinessUsers.AnyAsync(u => u.Email == "admin@beachgo.com"))
        {
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(adminPassword))
                throw new InvalidOperationException("ADMIN_PASSWORD environment variable is not set. Cannot seed admin user.");

            var adminUser = new BeachRehberi.API.Models.BusinessUser(
                "admin@beachgo.com",
                BCrypt.Net.BCrypt.HashPassword(adminPassword),
                BeachRehberi.API.Models.UserRoles.Admin
            );
            adminUser.UpdateProfile("Admin User", "BeachGo Admin");
            db.BusinessUsers.Add(adminUser);
            await db.SaveChangesAsync();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred during PostgreSQL migration: {ex.Message}");
    }
}

app.Run();
