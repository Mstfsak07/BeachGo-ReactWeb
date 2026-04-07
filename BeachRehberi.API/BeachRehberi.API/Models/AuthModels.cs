using System;

namespace BeachRehberi.API.Models;

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string Business = "Business";
    public const string User = "User";
}

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PhoneNumber { get; set; }
}

public class LoginRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class LoginResponse
{
    public required string Token { get; set; }
    public required string RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public required UserDto User { get; set; }
}

public class ForgotPasswordRequest
{
    public required string Email { get; set; }
}

public class ResetPasswordRequest
{
    public required string Email { get; set; }
    public required string OtpCode { get; set; }
    public required string NewPassword { get; set; }
}

public class VerifyEmailRequest
{
    public required string Email { get; set; }
    public required string OtpCode { get; set; }
}

public class AuthResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public string? Token { get; set; }
    public string? RefreshToken { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public required string Email { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string PhoneNumber { get; set; }
    public bool IsEmailVerified { get; set; }
}

// Keep existing ones to prevent build errors elsewhere if used
public class RevokeRequest
{
    public required string RefreshToken { get; set; }
}
public class RefreshRequest
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

// UserRegisterRequest for backward compatibility if needed in UI
public class UserRegisterRequest 
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}