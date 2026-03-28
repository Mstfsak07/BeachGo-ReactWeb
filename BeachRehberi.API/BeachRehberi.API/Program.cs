using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using BeachRehberi.API.Middlewares;
using BeachRehberi.API.Validators;
using BeachRehberi.API.Models;
using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;

var builder = WebApplication.CreateBuilder(args);

// --- JWT SECRET ---
var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    jwtSecret = "Testing_Secret_Key_For_BeachGo_2026!";
}

// --- DATABASE ---
var dbConn = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? "Data Source=beachrehberi.db";

builder.Services.AddDbContext<BeachDbContext>(options =>
    options.UseSqlite(dbConn));

// --- SERVICES ---
builder.Services.AddMemoryCache();
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBeachService, BeachService>();

// --- VALIDATION ---
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();

// --- AUTH ---
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };
});

builder.Services.AddAuthorization();

// --- SWAGGER + JWT ---
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BeachRehberi.API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
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
            new string[] {}
        }
    });
});

// --- BUILD ---
var app = builder.Build();

// --- MIDDLEWARE ---
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();