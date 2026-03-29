using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.User;
    public bool IsEmailVerified { get; private set; } = false;
    public bool IsActive { get; private set; } = true;

    public int? TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiresAt { get; private set; }

    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();

    // EF Core constructor
    private User() { }

    public User(string email, string passwordHash, string firstName, string lastName, UserRole role = UserRole.User)
    {
        Email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        FirstName = firstName ?? string.Empty;
        LastName = lastName ?? string.Empty;
        Role = role;
    }

    public void SetRefreshToken(string token, DateTime expiresAt)
    {
        RefreshToken = token;
        RefreshTokenExpiresAt = expiresAt;
        SetUpdated();
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiresAt = null;
        SetUpdated();
    }

    public bool IsRefreshTokenValid(string token)
    {
        return RefreshToken == token
            && RefreshTokenExpiresAt.HasValue
            && RefreshTokenExpiresAt.Value > DateTime.UtcNow;
    }

    public void SetPasswordResetToken(string token, DateTime expiresAt)
    {
        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = expiresAt;
        SetUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
        SetUpdated();
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        SetUpdated();
    }

    public void AssignToTenant(int tenantId)
    {
        TenantId = tenantId;
        SetUpdated();
    }

    public void UpdateProfile(string firstName, string lastName, string phone)
    {
        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        Phone = phone ?? Phone;
        SetUpdated();
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
