using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Interfaces;

namespace BeachRehberi.Infrastructure.Services;

public interface ITenantService
{
    Task<int?> GetTenantIdBySlugAsync(string slug, CancellationToken cancellationToken = default);
    Task<bool> IsActiveAsync(int tenantId, CancellationToken cancellationToken = default);
}

public class TenantService : ITenantService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;

    public TenantService(IUnitOfWork unitOfWork, ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
    }

    public async Task<int?> GetTenantIdBySlugAsync(
        string slug, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"tenant:slug:{slug}";

        return await _cacheService.GetOrSetAsync(
            cacheKey,
            async () =>
            {
                var tenant = await _unitOfWork.Tenants.FirstOrDefaultAsync(
                    t => t.Slug == slug && !t.IsDeleted, cancellationToken);
                return tenant?.Id;
            },
            TimeSpan.FromMinutes(15),
            cancellationToken);
    }

    public async Task<bool> IsActiveAsync(int tenantId, CancellationToken cancellationToken = default)
    {
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        return tenant is { IsActive: true };
    }
}
