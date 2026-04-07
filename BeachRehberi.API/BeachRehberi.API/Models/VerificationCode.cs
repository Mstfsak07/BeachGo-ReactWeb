using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public enum OtpPurpose
{
    EmailVerification,
    PasswordReset
}

public class VerificationCode
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public OtpPurpose Purpose { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime CreatedAt { get; set; }
}