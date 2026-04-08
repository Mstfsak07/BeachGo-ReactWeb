using FluentValidation;
using System;
using System.Collections.Generic;

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
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Token { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.Compare("NewPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class VerifyEmailRequest
{
    [System.ComponentModel.DataAnnotations.Required]
    [System.ComponentModel.DataAnnotations.EmailAddress]
    public string Email { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.Required]
    public string Token { get; set; } = string.Empty;
}

public class ResendVerificationRequest
{
    public string Email { get; set; } = string.Empty;
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public List<string>? Errors { get; set; }
    public string? RefreshToken { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public UserDto? User { get; set; }
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

public class RevokeRequest
{
    public required string RefreshToken { get; set; }
    public string? Reason { get; set; }
}

public class RefreshRequest
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
}

public class UserRegisterRequest
{
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}

public class RefreshTokenRequest {
    [System.ComponentModel.DataAnnotations.Required] public string AccessToken { get; set; } = string.Empty;
    [System.ComponentModel.DataAnnotations.Required] public string RefreshToken { get; set; } = string.Empty;
}

public class TokenResponse {
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}