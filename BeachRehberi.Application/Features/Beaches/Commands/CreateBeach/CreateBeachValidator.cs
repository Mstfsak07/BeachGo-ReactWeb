using FluentValidation;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

public class CreateBeachValidator : AbstractValidator<CreateBeachCommand>
{
    public CreateBeachValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plaj adı zorunludur.")
            .MaximumLength(200).WithMessage("Plaj adı 200 karakterden uzun olamaz.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Açıklama 2000 karakterden uzun olamaz.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Adres zorunludur.")
            .MaximumLength(500).WithMessage("Adres 500 karakterden uzun olamaz.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Geçerli bir enlem değeri giriniz.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Geçerli bir boylam değeri giriniz.");

        RuleFor(x => x.EntryFee)
            .GreaterThanOrEqualTo(0).WithMessage("Giriş ücreti negatif olamaz.")
            .When(x => x.HasEntryFee);

        RuleFor(x => x.SunbedPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Şezlong fiyatı negatif olamaz.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Kapasite 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(100000).WithMessage("Kapasite gerçekçi bir değer olmalıdır.");
    }
}
