using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BeachRehberi.API.Data;
using BeachRehberi.API.Middlewares;
using BeachRehberi.API.Mappings;
using BeachRehberi.API.Services;

var builder = WebApplication.CreateBuilder(args);

// ─── VeritabanÄą KonfigĂźrasyonu ─────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                    ?? "Data Source=beachrehberi.db";

builder.Services.AddDbContext<BeachDbContext>(options =>
    options.UseSqlite(connectionString)); // Docker environment ile connection string gelirse o kullanÄąlÄąr

// ─── Standart Servisler ──────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

// ─── Custom Servisler ────────────────────────────
builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();

// ─── AutoMapper ──────────────────────────────────
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

// ─── CORS PolitikasÄą ──────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("BeachGoPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost") // Prod ve Dev iĂ§in
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// ─── JWT Authentication ──────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
var secretKey = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"] ?? "BeachRehberi_SuperSecret_Key_2026!");

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
        ValidIssuer = jwtSettings["Issuer"] ?? "BeachRehberi.API",
        ValidAudience = jwtSettings["Audience"] ?? "BeachRehberi.App",
        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
    };
});

var app = builder.Build();

// ─── Pipeline (Middleware) ──────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Global Exception Middleware
app.UseMiddleware<ExceptionMiddleware>();

app.UseHttpsRedirection();
app.UseCors("BeachGoPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
