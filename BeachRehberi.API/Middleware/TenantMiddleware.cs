using BeachRehberi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Middleware;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TenantMiddleware> _logger;

    public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IUnitOfWork unitOfWork)
    {
        // Tenant bilgisini header'dan veya subdomain'den al
        var tenantSlug = context.Request.Headers["X-Tenant-Slug"].FirstOrDefault()
                         ?? GetSubdomain(context.Request.Host.Value);

        if (!string.IsNullOrEmpty(tenantSlug))
        {
            var tenant = await unitOfWork.Tenants.FirstOrDefaultAsync(
                t => t.Slug == tenantSlug && !t.IsDeleted);

            if (tenant != null)
            {
                context.Items["TenantId"] = tenant.Id;
                context.Items["TenantSlug"] = tenant.Slug;

                if (!tenant.IsActive)
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Bu tenant devre dışı bırakılmıştır.");
                    return;
                }
            }
        }

        await _next(context);
    }

    private static string? GetSubdomain(string host)
    {
        if (string.IsNullOrEmpty(host)) return null;
        var parts = host.Split('.');
        return parts.Length > 2 ? parts[0] : null;
    }
}
