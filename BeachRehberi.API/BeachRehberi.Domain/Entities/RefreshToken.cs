using System;
using System.Security.Cryptography;

namespace BeachRehberi.Domain.Entities;

public class RefreshToken
{
    public int Id { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public int UserId { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? RevokedAt { get; private set; }
    public string? ReplacedByToken { get; private set; }
    public string? RevokedReason { get; private set; }
    public string? CreatedByIp { get; private set; }
    public string? CreatedByUserAgent { get; private set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    private RefreshToken() { }

    public RefreshToken(int userId, string plainToken, DateTime expiresAt,
        string? ipAddress, string? userAgent)
    {
        UserId = userId;
        TokenHash = HashToken(plainToken);
        ExpiresAt = expiresAt;
        CreatedByIp = ipAddress;
        CreatedByUserAgent = userAgent;
        CreatedAt = DateTime.UtcNow;
    }

    public void RevokeAndReplace(string newPlainToken, string reason = "rotation")
    {
        RevokedAt = DateTime.UtcNow;
        ReplacedByToken = HashToken(newPlainToken);
        RevokedReason = reason;
    }

    public void Revoke(string reason = "logout")
    {
        RevokedAt = DateTime.UtcNow;
        RevokedReason = reason;
    }

    public static string HashToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return string.Empty;
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(token)));
    }
}
