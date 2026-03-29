using Microsoft.AspNetCore.Http;

namespace BeachRehberi.Infrastructure.Services;

/// <summary>
/// Tenant service implementation
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Dictionary<Guid, string> _tenants;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));

        // In a real application, this would come from configuration or database
        _tenants = new Dictionary<Guid, string>
        {
            { Guid.Parse("11111111-1111-1111-1111-111111111111"), "DefaultTenant" },
            { Guid.Parse("22222222-2222-2222-2222-222222222222"), "BeachGoTenant" }
        };
    }

    public Guid GetCurrentTenantId()
    {
        // Try to get tenant from HTTP context (header, claim, etc.)
        var context = _httpContextAccessor.HttpContext;

        if (context?.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdHeader) == true)
        {
            if (Guid.TryParse(tenantIdHeader.ToString(), out var tenantId) && IsValidTenant(tenantId))
            {
                return tenantId;
            }
        }

        // Try to get from user claims
        var tenantIdClaim = context?.User.FindFirst("tenant_id")?.Value;
        if (!string.IsNullOrEmpty(tenantIdClaim) &&
            Guid.TryParse(tenantIdClaim, out var claimTenantId) &&
            IsValidTenant(claimTenantId))
        {
            return claimTenantId;
        }

        // Default tenant
        return Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    public string GetCurrentTenantName()
    {
        var tenantId = GetCurrentTenantId();
        return _tenants.TryGetValue(tenantId, out var name) ? name : "Unknown";
    }

    public bool IsValidTenant(Guid tenantId)
    {
        return _tenants.ContainsKey(tenantId);
    }
}