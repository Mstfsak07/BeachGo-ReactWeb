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

        RuleFor(x => x.BusinessName)
            .NotEmpty().WithMessage("İşletme adı zorunludur.")
            .MaximumLength(100).WithMessage("İşletme adı en fazla 100 karakter olabilir.");

        RuleFor(x => x.ContactName)
            .MaximumLength(50).WithMessage("Yetkili ismi en fazla 50 karakter olabilir.");
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
