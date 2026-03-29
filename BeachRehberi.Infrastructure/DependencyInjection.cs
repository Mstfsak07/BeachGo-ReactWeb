using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Interfaces;
using BeachRehberi.Infrastructure.Persistence;
using BeachRehberi.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BeachRehberi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Veritabanı
        var connectionString = Environment.GetEnvironmentVariable("BEACHGO_DB_CONN")
                               ?? configuration.GetConnectionString("DefaultConnection")
                               ?? "Data Source=beachrehberi.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        // Repository & UnitOfWork
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // Cache (önce Memory, Redis gelince değiştirilir)
        services.AddDistributedMemoryCache();
        services.AddScoped<ICacheService, CacheService>();

        // Services
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
