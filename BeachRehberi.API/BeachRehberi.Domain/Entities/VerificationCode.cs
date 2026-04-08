using System;

namespace BeachRehberi.Domain.Entities;

public enum OtpPurpose
{
    EmailVerification,
    PasswordReset,
    TwoFactorAuth
}

public class VerificationCode
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string CodeHash { get; set; }
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
