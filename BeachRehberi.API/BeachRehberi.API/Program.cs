using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using BeachRehberi.API.Data;

using BeachRehberi.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── Environment Variables ───────────────────────────
var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET") 
                ?? builder.Configuration["Jwt:SecretKey"] 
                ?? "Development_Secret_Key_Do_Not_Use_In_Production_2026!";

var dbConn = Environment.GetEnvironmentVariable("BEACHGO_DB_CONN") 
             ?? builder.Configuration.GetConnectionString("DefaultConnection") 
             ?? "Data Source=beachrehberi.db";

// ─── Veritabanı ───────────────────────────────────────────
builder.Services.AddDbContext<BeachDbContext>(options => options.UseSqlite(dbConn));

// ─── Rate Limiting ───────────────────────────────────────
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

// ─── Services ─────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// ─── CORS ─────────────────────────────────────────────────
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

// ─── Authentication ─────────────────────────────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BeachRehberi.API",
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BeachRehberi.App",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("BusinessOnly", policy => policy.RequireRole("BusinessOwner", "Admin"));
});

var app = builder.Build();

// ─── Pipeline ─────────────────────────────────────────────

app.UseRateLimiter();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("BeachGoPolicy");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers().RequireRateLimiter("fixed");
app.Run();
