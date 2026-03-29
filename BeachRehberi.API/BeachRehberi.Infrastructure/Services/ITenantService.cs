namespace BeachRehberi.Infrastructure.Services;

/// <summary>
/// Tenant service interface for multi-tenant support
/// </summary>
public interface ITenantService
{
    Guid GetCurrentTenantId();
    string GetCurrentTenantName();
    bool IsValidTenant(Guid tenantId);
}