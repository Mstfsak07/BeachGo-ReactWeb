using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using BeachRehberi.API.Data;
using BeachRehberi.API.Middlewares;
using BeachRehberi.API.Mappings;
using BeachRehberi.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── VeritabanÄą ───────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? "Data Source=beachrehberi.db";
builder.Services.AddDbContext<BeachDbContext>(options => options.UseSqlite(connectionString));

// ─── Rate Limiting (GĂźvenlik) ───────────────────────────
builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    
    // Genel kÄąsÄątlama: IP baĹąÄąna 1 dakikada 100 istek
    options.AddFixedWindowLimiter("fixed", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueLimit = 0;
    });

    // Auth kÄąsÄątlama: GiriĹą denemeleri iĂ§in 1 dakikada 5 istek (Brute-force korumasÄą)
    options.AddFixedWindowLimiter("auth", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });
});

// ─── Servisler ───────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ─── CORS (GĂźvenli YapÄąlandÄąrma) ────────────────────────
var allowedOrigins = Environment.GetEnvironmentVariable("BEACHGO_ALLOWED_ORIGINS")?.Split(',') 
                    ?? new[] { "http://localhost:3000" };

builder.Services.AddCors(options => {
    options.AddPolicy("BeachGoPolicy", policy => {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ─── JWT Authentication ───────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKeyString = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET") 
                    ?? jwtSettings["SecretKey"] 
                    ?? "BeachRehberi_Development_Only_Super_Secret_Key_2026!";
var secretKey = Encoding.ASCII.GetBytes(secretKeyString);

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
        ValidIssuer = jwtSettings["Issuer"] ?? "BeachRehberi.API",
        ValidAudience = jwtSettings["Audience"] ?? "BeachRehberi.App",
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("BusinessOnly", policy => policy.RequireRole("BusinessOwner", "Admin"));
});

var app = builder.Build();

// ─── Pipeline (Middleware) ─────────────────────────────
app.UseMiddleware<ExceptionMiddleware>();
app.UseRateLimiter(); // Ănemli: CORS'tan Ăśnce gelmeli mi? HayÄąr, Routing'den sonra.

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BeachGoPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiter("fixed"); // VarsayÄąlan olarak tĂźmĂźne uygula
app.Run();
