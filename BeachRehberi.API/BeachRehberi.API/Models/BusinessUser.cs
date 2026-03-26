namespace BeachRehberi.API.Models;

public class BusinessUser
{
    public int Id { get; set; }
    public int BeachId { get; set; }
    public Beach Beach { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string ContactName { get; set; } = string.Empty;
    public string Role { get; set; } = "BusinessOwner"; // Default rol
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }
}