using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using BeachRehberi.API.Middlewares;
using BeachRehberi.API.Validators;
using BeachRehberi.API.Models;
using FluentValidation;
using FluentValidation.AspNetCore;

var instanceMutexName = "BeachRehberi.API.Singleton";
Mutex? instanceMutex = null;

try
{
    instanceMutex = new Mutex(true, instanceMutexName, out bool createdNew);
    if (!createdNew)
    {
        Console.WriteLine("Another BeachRehberi.API instance is already running. Exiting duplicate instance.");
        return;
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unable to acquire instance lock: {ex.Message}");
}

var builder = WebApplication.CreateBuilder(args);

// --- Environment Variables ---
var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    if (builder.Environment.IsProduction()) throw new InvalidOperationException("CRITICAL: BEACHGO_JWT_SECRET environment variable is missing!"); else jwtSecret = "Testing_Secret_Key_For_BeachGo_2026!";
}

var dbConn = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=beachrehberi.db";

// --- Database ---
builder.Services.AddDbContext<BeachDbContext>(options => options.UseSqlite(dbConn));

// --- Memory Cache ---
builder.Services.AddMemoryCache(); // Required for TokenService optimization

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("fixed", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
    options.AddFixedWindowLimiter("auth", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
    });
});

// --- Services & Controllers ---
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            var response = new ErrorResponse
            {
                Success = false,
                Message = "Doğrulama hatası oluştu.",
                Errors = errors
            };

            return new BadRequestObjectResult(response);
        };
    });

// --- FluentValidation Configuration ---
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// Business Services Registration
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

// CORS
builder.Services.AddCors(options => {
    options.AddPolicy("BeachGoPolicy", policy => {
        policy.WithOrigins("https://beachgo.app", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// Authentication
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
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

builder.Services.AddAuthorization(options => {
    options.AddPolicy("BusinessOnly", policy => policy.RequireRole("BusinessOwner", "Admin"));
});

var preferredHttpPort = 5143;
var actualHttpPort = FindAvailablePort(preferredHttpPort);
builder.WebHost.UseUrls($"http://127.0.0.1:{actualHttpPort}");
Console.WriteLine($"BeachRehberi.API will listen on http://127.0.0.1:{actualHttpPort}");

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// --- Production Pipeline ---
app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseRateLimiter();
app.UseCors("BeachGoPolicy");

app.UseAuthentication();
app.UseMiddleware<JwtBlacklistMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

static int FindAvailablePort(int preferredPort)
{
    for (var port = preferredPort; port < preferredPort + 1000; port++)
    {
        if (!IsTcpPortInUse(port))
        {
            return port;
        }
    }

    throw new InvalidOperationException("Unable to find an available HTTP port for BeachRehberi.API.");
}

static bool IsTcpPortInUse(int port)
{
    try
    {
        using var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        listener.Stop();
        return false;
    }
    catch (SocketException)
    {
        return true;
    }
}
