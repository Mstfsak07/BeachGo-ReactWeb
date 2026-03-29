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

public class UserRegisterRequest  // ← YENİ: User Registration
{
    public required string Username { get; set; }
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
    public string Role { get; set; } = UserRoles.User;
}

// ← DEĞİŞTİ: artık body tabanlı, cookie yok
public class RefreshRequest
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

public class RevokeRequest  // ← YENİ
{
    public required string RefreshToken { get; set; }
}

public class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime AccessTokenExpiry { get; set; } // ← YENİ: proaktif refresh için
}