using System.Security.Claims;
using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace BeachRehberi.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? User?.FindFirstValue("sub");
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public int? TenantId
    {
        get
        {
            var value = User?.FindFirstValue("tenantId");
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Email => User?.FindFirstValue(ClaimTypes.Email)
                           ?? User?.FindFirstValue("email");

    public UserRole? Role
    {
        get
        {
            var value = User?.FindFirstValue(ClaimTypes.Role);
            return Enum.TryParse<UserRole>(value, out var role) ? role : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;
    public bool IsAdmin => Role == UserRole.Admin;
    public bool IsBusinessOwner => Role == UserRole.BusinessOwner || IsAdmin;
}
