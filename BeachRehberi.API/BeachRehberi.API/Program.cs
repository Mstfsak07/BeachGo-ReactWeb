using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Builder;
using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using BeachRehberi.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// --- Environment Variables ---
var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    throw new InvalidOperationException("CRITICAL: BEACHGO_JWT_SECRET environment variable is missing!");
}

var dbConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=beachrehberi.db";

// --- Database ---
builder.Services.AddDbContext<BeachDbContext>(options => options.UseSqlite(dbConn));

// --- Rate Limiting (Korumalı) ---
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
    options.AddFixedWindowLimiter("auth", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5; // Production seviyesi kısıtlama
    });
});

// --- Services ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Business Services Registration
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("BeachGoPolicy", policy => {
        policy.WithOrigins("https://beachgo.app", "http://localhost:3000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
    });
});

// Authentication
var jwtKey = Encoding.UTF8.GetBytes(jwtSecret);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        RequireExpirationTime = true,
        ClockSkew = TimeSpan.FromMinutes(2),
        ValidateIssuerSigningKey = true,
        ValidIssuer = "BeachRehberi.API",
        ValidAudience = "BeachRehberi.App",
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("BusinessOnly", policy => policy.RequireRole("BusinessOwner", "Admin"));
});

var app = builder.Build();

// --- Production Pipeline ---
app.UseMiddleware<GlobalExceptionMiddleware>(); // Global Exception Handling (En Başta)

app.UseRateLimiter();
app.UseCors("BeachGoPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

