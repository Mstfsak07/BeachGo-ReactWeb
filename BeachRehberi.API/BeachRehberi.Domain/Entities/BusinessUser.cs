using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.ValueObjects;

namespace BeachRehberi.Domain.Entities;

/// <summary>
/// Business User entity - işletme kullanıcılarını temsil eder
/// </summary>
public class BusinessUser : BaseEntity
{
    public int? BeachId { get; private set; }
    public Beach? Beach { get; private set; }

    public Email Email { get; private set; }
    public string PasswordHash { get; private set; }

    public string? ContactName { get; private set; }
    public string? BusinessName { get; private set; }

    public string Role { get; private set; }
    public DateTime? LastLoginAt { get; private set; }

    // EF Core constructor
    private BusinessUser() : base()
    {
        Email = null!;
        PasswordHash = string.Empty;
        Role = string.Empty;
    }

    public BusinessUser(Guid tenantId, Email email, string passwordHash, string role = "User")
        : base(tenantId)
    {
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Role = role ?? throw new ArgumentNullException(nameof(role));
    }

    public void UpdateProfile(string? contactName, string? businessName)
    {
        ContactName = contactName;
        BusinessName = businessName;
        MarkAsUpdated();
    }

    public void AssignToBeach(int? beachId)
    {
        BeachId = beachId;
        MarkAsUpdated();
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        MarkAsUpdated();
    }

    public void ChangeRole(string newRole)
    {
        Role = newRole ?? throw new ArgumentNullException(nameof(newRole));
        MarkAsUpdated();
    }
}