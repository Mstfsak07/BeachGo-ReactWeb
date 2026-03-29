using FluentValidation;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.BeachId)
            .GreaterThan(0).WithMessage("Geçerli bir plaj seçiniz.");

        RuleFor(x => x.ReservationDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("Rezervasyon tarihi bugünden sonra olmalıdır.");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("Misafir sayısı en az 1 olmalıdır.")
            .LessThanOrEqualTo(50).WithMessage("Tek rezervasyonda en fazla 50 misafir olabilir.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notlar 500 karakterden uzun olamaz.")
            .When(x => !string.IsNullOrEmpty(x.Notes));
    }
}
