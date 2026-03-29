using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string? Phone { get; private set; }
    public string? ProfileImageUrl { get; private set; }

    public UserRole Role { get; private set; } = UserRole.User;
    public bool IsActive { get; private set; } = true;
    public bool EmailVerified { get; private set; } = false;

    public string? RefreshToken { get; private set; }
    public DateTime? RefreshTokenExpiry { get; private set; }

    public int? TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();

    // EF Core constructor
    private User() { }

    public User(string email, string passwordHash, string firstName, string lastName,
                UserRole role = UserRole.User, string? phone = null)
    {
        Email = email?.ToLowerInvariant() ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Role = role;
        Phone = phone;
        IsActive = true;
    }

    public string FullName => $"{FirstName} {LastName}".Trim();

    public void SetRefreshToken(string token, DateTime expiry)
    {
        RefreshToken = token;
        RefreshTokenExpiry = expiry;
        SetUpdated();
    }

    public void RevokeRefreshToken()
    {
        RefreshToken = null;
        RefreshTokenExpiry = null;
        SetUpdated();
    }

    public bool IsRefreshTokenValid(string token)
        => RefreshToken == token
           && RefreshTokenExpiry.HasValue
           && RefreshTokenExpiry.Value > DateTime.UtcNow;

    public void UpdateProfile(string firstName, string lastName, string? phone, string? profileImageUrl = null)
    {
        FirstName = firstName ?? FirstName;
        LastName = lastName ?? LastName;
        Phone = phone;
        ProfileImageUrl = profileImageUrl;
        SetUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        RevokeRefreshToken();
        SetUpdated();
    }

    public void VerifyEmail()
    {
        EmailVerified = true;
        SetUpdated();
    }

    public void Activate() { IsActive = true; SetUpdated(); }
    public void Deactivate() { IsActive = false; SetUpdated(); }

    public void AssignToTenant(int tenantId)
    {
        TenantId = tenantId;
        SetUpdated();
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        SetUpdated();
    }
}
