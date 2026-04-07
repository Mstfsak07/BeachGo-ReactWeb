using FluentValidation;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>      
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz.")  
            .MaximumLength(100).WithMessage("E-posta adresi en fazla 100 karakter olabilir.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.")
            .MinimumLength(6).WithMessage("Şifre en az 6 karakter olmalıdır.")  
            .MaximumLength(50).WithMessage("Şifre en fazla 50 karakter olabilir.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Ad zorunludur.")
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Soyad zorunludur.")
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.");
            
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage("Telefon numarası zorunludur.")
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.");
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-posta adresi zorunludur.")
            .EmailAddress().WithMessage("Geçerli bir e-posta adresi giriniz."); 

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Şifre zorunludur.");
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olmamalı")
            .EmailAddress().WithMessage("Geçerli email formatı olmalı");
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token boş olmamalı");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olmamalı")
            .EmailAddress().WithMessage("Geçerli email formatı olmalı");
            
        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Yeni şifre boş olmamalı")
            .MinimumLength(8).WithMessage("Yeni şifre min 8 karakter olmalı")
            .Matches("[A-Z]").WithMessage("Yeni şifre en az 1 büyük harf içermeli")
            .Matches("[0-9]").WithMessage("Yeni şifre en az 1 rakam içermeli");
            
        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.NewPassword).WithMessage("Şifreler eşleşmiyor");
    }
}

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token boş olmamalı");
            
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email boş olmamalı")
            .EmailAddress().WithMessage("Geçerli email formatı olmalı");
    }
}