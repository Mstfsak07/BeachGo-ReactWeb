using FluentValidation;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

public class CreateBeachValidator : AbstractValidator<CreateBeachCommand>
{
    public CreateBeachValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Plaj adı boş olamaz.")
            .MaximumLength(200).WithMessage("Plaj adı en fazla 200 karakter olabilir.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Açıklama boş olamaz.")
            .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir.");

        RuleFor(x => x.Location)
            .NotEmpty().WithMessage("Konum boş olamaz.")
            .MaximumLength(500).WithMessage("Konum en fazla 500 karakter olabilir.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("Şehir boş olamaz.")
            .MaximumLength(100).WithMessage("Şehir en fazla 100 karakter olabilir.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90).WithMessage("Geçerli bir enlem değeri giriniz (-90 ile 90 arası).");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180).WithMessage("Geçerli bir boylam değeri giriniz (-180 ile 180 arası).");

        RuleFor(x => x.PricePerPerson)
            .GreaterThanOrEqualTo(0).WithMessage("Fiyat negatif olamaz.")
            .LessThanOrEqualTo(100_000).WithMessage("Fiyat 100.000 TL'den fazla olamaz.");

        RuleFor(x => x.Capacity)
            .GreaterThan(0).WithMessage("Kapasite 0'dan büyük olmalıdır.")
            .LessThanOrEqualTo(50_000).WithMessage("Kapasite 50.000'den fazla olamaz.");

        RuleFor(x => x.Phone)
            .MaximumLength(20).WithMessage("Telefon en fazla 20 karakter olabilir.")
            .When(x => x.Phone != null);

        RuleFor(x => x.Website)
            .MaximumLength(200).WithMessage("Web sitesi en fazla 200 karakter olabilir.")
            .When(x => x.Website != null);
    }
}
