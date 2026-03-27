using System;

namespace BeachRehberi.API.Models;

public class RefreshToken
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiryDate { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Rotation & Security Tracking
    public string? ReplacedByToken { get; set; }
    public string? ReasonRevoked { get; set; }
    
    // Requirement 1: New Tracking Fields
    public string? CreatedByIp { get; set; }
    public string? CreatedByUserAgent { get; set; }
    
    public bool IsExpired => DateTime.UtcNow >= ExpiryDate;
    public bool IsActive => !IsRevoked && !IsExpired;
}
