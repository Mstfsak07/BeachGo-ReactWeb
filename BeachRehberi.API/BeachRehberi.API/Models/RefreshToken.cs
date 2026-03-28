using System;

namespace BeachRehberi.API.Models;

public class RefreshToken
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Token { get; private set; } = string.Empty;
    public DateTime ExpiryDate { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Rotation & Security Tracking
    public string? ReplacedByToken { get; private set; }
    public string? ReasonRevoked { get; private set; }

    public string? CreatedByIp { get; private set; }
    public string? CreatedByUserAgent { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;

    // EF Core constructor
    private RefreshToken() { }

    public RefreshToken(int userId, string token, DateTime expiryDate, string? ipAddress, string? userAgent)
    {
        UserId = userId;
        Token = token ?? throw new ArgumentNullException(nameof(token));
        ExpiryDate = expiryDate;
        CreatedByIp = ipAddress;
        CreatedByUserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }

    public void Revoke(string? reason = null, string? replacedByToken = null)
    {
        IsRevoked = true;
        ReasonRevoked = reason;
        ReplacedByToken = replacedByToken;
    }
}

