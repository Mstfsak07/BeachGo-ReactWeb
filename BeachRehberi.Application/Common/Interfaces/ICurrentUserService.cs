using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Application.Common.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    UserRole? Role { get; }
    int? TenantId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsBusinessOwner { get; }
}
