using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
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
using System.Globalization;
using BCrypt.Net;
using System.Security.Claims;
using BeachRehberi.API.Models;
using Resend;
using Npgsql;
using BeachRehberi.Domain.Interfaces;

var builder = WebApplication.CreateBuilder(args);
var runningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);
var forwardedHeadersEnabled = builder.Configuration.GetValue<bool>("ASPNETCORE_FORWARDEDHEADERS_ENABLED");


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
// DATABASE CONFIGURATION (PostgreSQL)
// ─────────────────────────────────────────
var dbConn = ResolveDatabaseConnectionString(builder.Configuration);

if (string.IsNullOrWhiteSpace(dbConn))
{
    throw new InvalidOperationException("Database connection string is missing. Set BEACHGO_DB_CONN or provide Cloud SQL compatible DB env vars.");
}

builder.Services.AddDbContext<BeachDbContext>(options =>
{
    options.UseNpgsql(dbConn, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null);
    });

    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.EnableDetailedErrors();
    }
});

static string? ResolveDatabaseConnectionString(ConfigurationManager configuration)
{
    var directConnectionString = Environment.GetEnvironmentVariable("BEACHGO_DB_CONN");
    if (!string.IsNullOrWhiteSpace(directConnectionString))
    {
        return directConnectionString;
    }

    var configuredConnectionString = configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrWhiteSpace(configuredConnectionString))
    {
        return configuredConnectionString;
    }

    var database = Environment.GetEnvironmentVariable("BEACHGO_DB_NAME");
    var username = Environment.GetEnvironmentVariable("BEACHGO_DB_USER");
    var password = Environment.GetEnvironmentVariable("BEACHGO_DB_PASSWORD");

    if (string.IsNullOrWhiteSpace(database) ||
        string.IsNullOrWhiteSpace(username) ||
        string.IsNullOrWhiteSpace(password))
    {
        return null;
    }

    var builder = new NpgsqlConnectionStringBuilder
    {
        Database = database,
        Username = username,
        Password = password,
        Pooling = true,
        Timeout = 15,
        CommandTimeout = 30
    };

    var cloudSqlConnectionName = Environment.GetEnvironmentVariable("CLOUD_SQL_CONNECTION_NAME");
    if (!string.IsNullOrWhiteSpace(cloudSqlConnectionName))
    {
        // Cloud Run + Cloud SQL connector mounts PostgreSQL sockets under /cloudsql/<INSTANCE_CONNECTION_NAME>.
        builder.Host = $"/cloudsql/{cloudSqlConnectionName}";
        return builder.ConnectionString;
    }

    builder.Host = Environment.GetEnvironmentVariable("BEACHGO_DB_HOST") ?? "127.0.0.1";

    if (int.TryParse(Environment.GetEnvironmentVariable("BEACHGO_DB_PORT"), out var port) && port > 0)
    {
        builder.Port = port;
    }

    return builder.ConnectionString;
}

// ... (other infrastructure) ...
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddScoped(typeof(IRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// ... (DI registrations) ...
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddSingleton<IGeoCalculator, GeoCalculator>();
builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IStoryService, StoryService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddHttpClient<IWeatherService, WeatherService>();
builder.Services.AddScoped<IRevokedTokenCleanupJob, RevokedTokenCleanupJob>();
builder.Services.AddSingleton<CloudSchedulerRequestValidator>();
builder.Services.AddSingleton<IDatabaseInitializationState, DatabaseInitializationState>();
builder.Services.AddSingleton<DbInitializer>();
builder.Services.AddSingleton<IGoogleCloudStorageService, GoogleCloudStorageService>();

// Auth support services (OTP + Email)
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<BeachRehberi.Application.Common.Interfaces.IOtpService, OtpService>();
// Email service: use Resend whenever a key is configured, regardless of environment.
var resendApiKey = Environment.GetEnvironmentVariable("RESEND_API_KEY")
                   ?? builder.Configuration["Resend:ApiKey"];
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
builder.Services.AddScoped<IPaymentService, StripePaymentService>();

// Provider Configurations
// ...
// (rest of DI)

// ─────────────────────────────────────────
// 6. MEDIATR
// ─────────────────────────────────────────
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly);
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

    options.OnRejected = async (context, cancellationToken) =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting.GuestReservation");
        var ip = context.HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        logger.LogWarning(
            "Rate limit aşıldı. {Method} {Path}, IP={IP}",
            context.HttpContext.Request.Method,
            context.HttpContext.Request.Path.Value,
            ip);

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
        {
            context.HttpContext.Response.Headers.RetryAfter =
                ((int)retryAfter.TotalSeconds).ToString(NumberFormatInfo.InvariantInfo);
        }

        await context.HttpContext.Response.WriteAsync(
            "Too many requests. Please try again later.",
            cancellationToken);
    };

    // Global Sliding Window - preventing burst and sustained abuse
    options.AddSlidingWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.SegmentsPerWindow = 4;
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
    });

    // Auth endpoints - stricter and partitioned per client IP
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global-auth",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                Window = TimeSpan.FromMinutes(1),
                PermitLimit = 20,
                QueueLimit = 0
            }));

    // Misafir uçları — lookup: 10 istek / 1 dk / IP (sliding, 4 segment); cancel+pay: 5 istek / 1 dk / IP (fixed).
    // Reddedilen istekler RateLimiting.GuestReservation log kategorisiyle uyarı olarak yazılır.
    options.AddPolicy("guest-reservation-lookup", httpContext =>
        RateLimitPartition.GetSlidingWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global-guest-lookup",
            factory: _ => new SlidingWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 10,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1),
                SegmentsPerWindow = 4
            }));

    // İptal ve ödeme uçları: daha sıkı (aynı IP ile toplu deneme)
    options.AddPolicy("guest-reservation-mutation", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "global-guest-mutation",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
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

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// ─────────────────────────────────────────
// BUILD
// ─────────────────────────────────────────
var app = builder.Build();

// ─────────────────────────────────────────
// MIDDLEWARE PIPELINE (sıralama kritik!)
// ─────────────────────────────────────────

if (forwardedHeadersEnabled || runningInContainer)
{
    app.UseForwardedHeaders();
}

var enableHttpsRedirection = !runningInContainer && !forwardedHeadersEnabled;

app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers.Append("X-Content-Type-Options", "nosniff");
    headers.Append("X-Frame-Options", "DENY");
    headers.Append("X-XSS-Protection", "1; mode=block");
    await next();
});

if (enableHttpsRedirection)
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
app.UseRouting();
app.UseRateLimiter();

// Authentication & Authorization
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.Services.GetRequiredService<DbInitializer>()
    .InitializeAsync(app.Lifetime.ApplicationStopping);

app.Run();

