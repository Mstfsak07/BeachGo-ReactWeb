using FluentValidation;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

public class CreateBeachValidator : AbstractValidator<CreateBeachCommand>
{
    public CreateBeachValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Beach adı zorunludur.")
            .MaximumLength(200).WithMessage("Beach adı 200 karakterden uzun olamaz.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama zorunludur.")
            .MaximumLength(2000).WithMessage("Açıklama 2000 karakterden uzun olamaz.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Konum bilgisi zorunludur.")
            .MaximumLength(500).WithMessage("Konum 500 karakterden uzun olamaz.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir zorunludur.")
            .MaximumLength(100).WithMessage("Şehir adı 100 karakterden uzun olamaz.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Enlem -90 ile 90 arasında olmalıdır.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Boylam -180 ile 180 arasında olmalıdır.");

        RuleFor(x => x.PricePerPerson)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.")
            .LessThanOrEqualTo(100_000).WithMessage("Fiyat 100.000 TL'den fazla olamaz.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Kapasite 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(10_000).WithMessage("Kapasite 10.000'den fazla olamaz.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon 20 karakterden uzun olamaz.")
            .When(x => x.Phone != null);

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Website adresi 200 karakterden uzun olamaz.")
            .When(x => x.Website != null);
    }
}
