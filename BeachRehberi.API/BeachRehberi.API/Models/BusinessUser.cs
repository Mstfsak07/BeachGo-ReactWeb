using System;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace BeachRehberi.API.Models;   

public class BusinessUser
{
    public int Id { get; private set; }      
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
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // EF Core constructor
    private BusinessUser() { }

    public BusinessUser(string email, string passwordHash, string role = UserRoles.User)
    {
        SetEmail(email);
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Role = role;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) throw new DomainException("Email adresi boş olamaz.");
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) throw new DomainException("Geçersiz email formatı.");
        Email = email;
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string phoneNumber)
    {
        FirstName = firstName;
        LastName = lastName;
        PhoneNumber = phoneNumber;
    }

    public void VerifyEmail()
    {
        IsEmailVerified = true;
    }
    
    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
    }

    public void AssignToBeach(int? beachId)
    {
        BeachId = beachId;
    }

    public void UpdateProfile(string? contactName, string? businessName)
    {
        ContactName = contactName;
        BusinessName = businessName;
    }

    public void RecordLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void Deactivate() => IsActive = false;
    public void Activate() => IsActive = true;
    public void SoftDelete() => IsDeleted = true;
}