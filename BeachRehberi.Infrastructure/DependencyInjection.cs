using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Interfaces;
using BeachRehberi.Infrastructure.Persistence;
using BeachRehberi.Infrastructure.Persistence.Repositories;
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
        // ─── Veritabanı (PostgreSQL) ───────────────────────────────
        var connectionString =
            Environment.GetEnvironmentVariable("BEACHGO_DB_CONN")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Veritabanı bağlantı dizisi tanımlı değil! " +
                "BEACHGO_DB_CONN env variable veya ConnectionStrings:DefaultConnection ayarlanmalı.");

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly("BeachRehberi.Infrastructure");
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(5),
                    errorCodesToAdd: null);
            });

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        // ─── Redis / In-Memory Cache ───────────────────────────────
        var redisConn =
            Environment.GetEnvironmentVariable("BEACHGO_REDIS_CONN")
            ?? configuration["Redis:ConnectionString"];

        if (!string.IsNullOrWhiteSpace(redisConn))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConn;
                options.InstanceName = "BeachRehberi:";
            });
        }
        else
        {
            // Redis yoksa memory cache ile devam et
            services.AddDistributedMemoryCache();
        }

        // ─── Repository + UnitOfWork ──────────────────────────────
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        // ─── Application Servisleri ───────────────────────────────
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICacheService, CacheService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITenantService, TenantService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
