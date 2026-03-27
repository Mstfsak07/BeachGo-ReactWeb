using System;
using System.Text.Json.Serialization;

namespace BeachRehberi.API.Models;   

public class BusinessUser
{
    public int Id { get; set; }      
    public int? BeachId { get; set; }
    [JsonIgnore]
    public Beach? Beach { get; set; }

    public required string Email { get; set; }
    [JsonIgnore]
    public required string PasswordHash { get; set; }

    public string? ContactName { get; set; }
    public string? BusinessName { get; set; }

    public string Role { get; set; } = UserRoles.User; // Updated default role
    public DateTime? LastLoginAt { get; set; }

    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
