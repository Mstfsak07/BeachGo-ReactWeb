namespace BeachRehberi.API.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Business = "Business";
    public const string User = "User";
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string BusinessName { get; set; }
    public string? ContactName { get; set; }
    public int? BeachId { get; set; }
    public string Role { get; set; } = UserRoles.User; // New field for role selection if needed
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; } // Nullable for cookie implementation
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class RefreshRequest
{
    public required string RefreshToken { get; set; }
}
