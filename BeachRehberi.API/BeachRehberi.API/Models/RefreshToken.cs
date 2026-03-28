using System;
using System.Security.Cryptography;
using System.Text;

namespace BeachRehberi.API.Models;

public class RefreshToken
{
    public int Id { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? CreatedByUserAgent { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    // EF Core constructor
    private RefreshToken() { }

    public RefreshToken(int userId, string token, DateTime expiresAt, string? ipAddress, string? userAgent)
    {
        UserId = userId;
        TokenHash = HashToken(token) ?? throw new ArgumentNullException(nameof(token));
        ExpiresAt = expiresAt;
        CreatedByIp = ipAddress;
        CreatedByUserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }

    public static string HashToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return string.Empty;
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(token);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }

    public bool VerifyToken(string token)
    {
        return TokenHash == HashToken(token);
    }

    public void Revoke()
    {
        RevokedAt = DateTime.UtcNow;
    }
}
