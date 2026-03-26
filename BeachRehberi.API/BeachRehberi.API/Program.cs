using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // ─── Veritabanı ───────────────────────────────────────────
        builder.Services.AddDbContext<BeachDbContext>(options =>
            options.UseSqlite("Data Source=beachrehberi.db"));  // ✅ )) ile kapandı

        // ─── Servisler ────────────────────────────────────────────
        builder.Services.AddHttpClient();
        builder.Services.AddScoped<IBeachService, BeachService>();
        builder.Services.AddScoped<IWeatherService, WeatherService>();
        builder.Services.AddScoped<IReservationService, ReservationService>();
        builder.Services.AddScoped<INotificationService, NotificationService>();
        builder.Services.AddScoped<IBusinessService, BusinessService>();
        builder.Services.AddScoped<IAuthService, AuthService>();

        // ─── JWT Auth ─────────────────────────────────────────────
        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = builder.Configuration["Jwt:Issuer"],
                    ValidAudience = builder.Configuration["Jwt:Audience"],
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                        System.Text.Encoding.UTF8.GetBytes(
                            builder.Configuration["Jwt:SecretKey"]!))
                };
            });

        builder.Services.AddAuthorization();

        // ─── CORS ─────────────────────────────────────────────────
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new() { Title = "Beach Rehberi API", Version = "v1" });
        });

        var app = builder.Build();

        // ─── Migration otomatik uygula ────────────────────────────
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();
            db.Database.Migrate();
        }

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        //app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.Run("http://0.0.0.0:5143");
    }
}