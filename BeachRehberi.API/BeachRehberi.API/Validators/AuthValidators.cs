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
            .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.FirstName));

        RuleFor(x => x.LastName)
            .MaximumLength(50).WithMessage("Soyad en fazla 50 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.LastName));

        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Ad soyad en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.ContactName)
            .MaximumLength(100).WithMessage("İletişim kişisi en fazla 100 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.ContactName));

        RuleFor(x => x.BusinessName)
            .MaximumLength(150).WithMessage("İşletme adı en fazla 150 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.BusinessName));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.")
            .When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber));

        RuleFor(x => x)
            .Must(HasSupportedNamePayload)
            .WithMessage("Ad soyad veya işletme bilgileri zorunludur.");
    }

    private static bool HasSupportedNamePayload(RegisterRequest request)
    {
        var hasFullName = !string.IsNullOrWhiteSpace(request.Name);
        var hasSeparatedName = !string.IsNullOrWhiteSpace(request.FirstName) && !string.IsNullOrWhiteSpace(request.LastName);
        var hasBusinessProfile = !string.IsNullOrWhiteSpace(request.BusinessName) && !string.IsNullOrWhiteSpace(request.ContactName);
        return hasFullName || hasSeparatedName || hasBusinessProfile;
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
