using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Builder;
using BeachRehberi.API.Data;
using BeachRehberi.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BeachDbContext>(options => 
    options.UseSqlite(builder.Configuration.GetConnectionString(\"DefaultConnection\") ?? \"Data Source=beachrehberi.db\"));

builder.Services.AddRateLimiter(options => {
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter(\"fixed\", opt => {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBeachService, BeachService>();
builder.Services.AddScoped<IBusinessService, BusinessService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<INotificationService, NotificationService>();

builder.Services.AddCors(options => {
    options.AddPolicy(\"BeachGoPolicy\", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseRateLimiter();
app.UseCors(\"BeachGoPolicy\");
app.UseAuthentication();
app.UseAuthorization();

// Explicit cast to IEndpointConventionBuilder to avoid CS1061 in some SDKs
((IEndpointConventionBuilder)app.MapControllers()).RequireRateLimiter(\"fixed\");

app.Run();
