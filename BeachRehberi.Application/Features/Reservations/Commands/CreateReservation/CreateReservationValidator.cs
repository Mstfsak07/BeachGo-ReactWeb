using FluentValidation;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.BeachId)
            .GreaterThan(0).WithMessage("Geçerli bir beach seçiniz.");

        RuleFor(x => x.ReservationDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Rezervasyon tarihi bugün veya sonrası olmalıdır.")
            .LessThanOrEqualTo(DateTime.Today.AddYears(1))
            .WithMessage("Rezervasyon 1 yıldan fazla ileri bir tarih için yapılamaz.");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("Misafir sayısı en az 1 olmalıdır.")
            .LessThanOrEqualTo(100).WithMessage("Misafir sayısı en fazla 100 olabilir.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notlar 500 karakterden uzun olamaz.")
            .When(x => x.Notes is not null);
    }
}
