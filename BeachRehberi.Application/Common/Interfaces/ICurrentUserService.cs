using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    int? TenantId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsBusinessOwner { get; }
}
