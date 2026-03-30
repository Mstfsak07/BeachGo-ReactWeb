using System.Text;
using System.Threading.RateLimiting;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using BeachRehberi.API.Middleware;
using BeachRehberi.Application;
using BeachRehberi.Infrastructure;
using BeachRehberi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Events;

// ─── Serilog ─────────────────────────────────────────────
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/beachrehberi-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("🏖️ BeachRehberi API başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    // ─── Dış Proxy/Load Balancer Desteği ─────────────────
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
    {
        // Reverse proxy (Nginx, AWS vb.) arkasındaysak gerçek IP'yi ve Port/Şema'yı almayı sağlar.
        options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        options.KnownNetworks.Clear();
        options.KnownProxies.Clear();
    });

    // ─── Global Cookie Security Policy (Üretim Ortamı) ───
    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        // CORS AllowCredentials uyumluluğu için SameSite None (Cross-domain çağrılar için gerekli)
        options.MinimumSameSitePolicy = SameSiteMode.None;
        // Çerezlere JavaScript üzerinden asla ulaşılamasın (XSS koruması)
        options.HttpOnly = Microsoft.AspNetCore.CookiePolicy.HttpOnlyPolicy.Always;
        // Çerezler sadece ve daima HTTPS üzerinden taşınsın
        options.Secure = CookieSecurePolicy.Always;
    });

    // ─── JWT Config & Claims Mapping ─────────────────────
    // Rol (Role) Eşleşme Problemini Giderir: Modern Token 'role' array'inin .NET Role Claim olarak işlenmesini zorunlu kılar.
    JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

    var jwtSecret = Environment.GetEnvironmentVariable("BEACHGO_JWT_SECRET")
                    ?? builder.Configuration["Jwt:SecretKey"]
                    ?? throw new InvalidOperationException("JWT Secret Key tanımlı değil!");

    // ─── Katman DI ───────────────────────────────────────
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // ─── Controllers ─────────────────────────────────────
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "BeachRehberi API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header. Örn: 'Bearer {token}'",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });
        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // ─── Rate Limiting ───────────────────────────────────
    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        options.AddFixedWindowLimiter("fixed", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 100;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 5;
        });
        options.AddFixedWindowLimiter("auth", opt =>
        {
            opt.Window = TimeSpan.FromMinutes(1);
            opt.PermitLimit = 10;
            opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            opt.QueueLimit = 2;
        });
    });

    // ─── CORS ────────────────────────────────────────────
    var allowedOrigins = Environment.GetEnvironmentVariable("BEACHGO_ALLOWED_ORIGINS")?.Split(',')
                         ?? new[] { "http://localhost:3000", "http://localhost:3001" };

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("BeachGoPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Cookie policy için gerekli
        });
    });

    // ─── Authentication ──────────────────────────────────
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            // Production'da HTTPS zorunluluğunu artırmak
            options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
            options.SaveToken = true;
            
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "BeachRehberi.API",
                ValidAudience = builder.Configuration["Jwt:Audience"] ?? "BeachRehberi.App",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero,
                // Yetki/Owner kontrollerinin hatasız çalışması için standartlaştırılmış map işlemi:
                RoleClaimType = "role",
                NameClaimType = "name"
            };
        });

    builder.Services.AddAuthorization(options =>
    {
        // ─── GÜVENLİK (Varsayılan Olarak Koru) ───────────────
        // Eğer bir uç noktanın açıkça [AllowAnonymous] özniteliği yoksa,
        // sistem varsayılan olarak yetkilendirilmiş (giriş yapmış) bir kullanıcı isteyecektir.
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
        options.AddPolicy("BusinessOnly", policy => policy.RequireRole("BusinessOwner", "Admin"));
        options.AddPolicy("AuthenticatedUser", policy => policy.RequireAuthenticatedUser());
    });

    // ─── Build ───────────────────────────────────────────
    var app = builder.Build();

    // ─── Auto Migration ──────────────────────────────────
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        Log.Information("✓ Veritabanı migration tamamlandı.");
    }

    // ─── Pipeline (Middleware Sırası Önemlidir) ──────────

    // 1. IP Yönlendirme (En başta olmalı)
    app.UseForwardedHeaders();

    // 2. Exception Handling ve HSTS (Canlı Ortam)
    app.UseMiddleware<ExceptionMiddleware>();
    if (!app.Environment.IsDevelopment())
    {
        // Geliştirme dışı ortamlarda tarayıcıyı daima HTTPS kullanmaya zorlar.
        app.UseHsts();
    }

    // 3. Özel Güvenlik Başlıkları (Security Headers Middleware)
    app.Use(async (context, next) =>
    {
        var headers = context.Response.Headers;
        headers.Append("X-Content-Type-Options", "nosniff");
        headers.Append("X-Frame-Options", "DENY");
        headers.Append("X-XSS-Protection", "1; mode=block");
        headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        headers.Append("Content-Security-Policy", "default-src 'self';");
        
        await next();
    });

    app.UseMiddleware<TenantMiddleware>();
    app.UseSerilogRequestLogging();
    app.UseRateLimiter();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "BeachRehberi API v1"));
    }

    app.UseHttpsRedirection();
    
    // Güvenlik: Cookie Policy aktif edildi
    app.UseCookiePolicy();

    // CORS, UseRouting sonrasında fakat Auth mekanizmalarından önce gelmelidir 
    // (MapControllers varsayılan olarak Routing araya ekler)
    app.UseCors("BeachGoPolicy");
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.MapControllers().RequireRateLimiter("fixed");

    Log.Information("✓ BeachRehberi API hazır.");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ API başlatılamadı.");
}
finally
{
    Log.CloseAndFlush();
}
