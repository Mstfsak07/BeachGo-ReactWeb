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
    public required string Email { get; set; }
}

public class ResetPasswordRequest
{
    public required string Token { get; set; }
    public required string Email { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}

public class VerifyEmailRequest
{
    public required string Token { get; set; }
    public required string Email { get; set; }
}

public class ResendVerificationRequest
{
    public required string Email { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public List<string>? Errors { get; set; }
    
    // Added to prevent breaking Login/Register mapping
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

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token zorunludur.");
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni þifre zorunludur.")
            .MinimumLength(8).WithMessage("Þifre en az 8 karakter olmalýdýr.")
            .Matches("[A-Z]").WithMessage("Þifre en az bir büyük harf içermelidir.")
            .Matches("[0-9]").WithMessage("Þifre en az bir rakam içermelidir.");
    }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email zorunludur.");
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token zorunludur.");
    }
}

public class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir email adresi giriniz.");
    }
}

