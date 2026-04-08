using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BeachRehberi.Domain.Entities;

public class BusinessUser : BaseEntity
{
    public int? BeachId { get; private set; }
    [JsonIgnore]
    public Beach? Beach { get; private set; }

    public string Email { get; private set; } = string.Empty;
    [JsonIgnore]
    public string PasswordHash { get; private set; } = string.Empty;

    public string? ContactName { get; private set; }
    public string? BusinessName { get; private set; }

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public bool IsEmailVerified { get; private set; }

    public string Role { get; private set; } = string.Empty;
    public DateTime? LastLoginAt { get; private set; }

    public bool IsActive { get; private set; }
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    public string? EmailVerificationToken { get; set; }
    public bool EmailVerified { get; set; } = false;

    // EF Core constructor
    private BusinessUser() : base() { }

    public BusinessUser(string email, string passwordHash, string role = "User") : base()
    {
        SetEmail(email);
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Role = role;
        IsActive = true;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new Exception("Email adresi boş olamaz.");
        // Basic regex for domain validation if needed
        Email = email;
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
        MarkAsUpdated();
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
        MarkAsUpdated();
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        MarkAsUpdated();
    }

    public void AssignToBeach(int? beachId)
    {
        BeachId = beachId;
        MarkAsUpdated();
    }

    public void UpdateProfile(string? contactName, string? businessName)
    {
        ContactName = contactName;
        BusinessName = businessName;
        MarkAsUpdated();
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    public void Deactivate() { IsActive = false; MarkAsUpdated(); }
    public void Activate() { IsActive = true; MarkAsUpdated(); }
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Business = "Business";
    public const string User = "User";
}
