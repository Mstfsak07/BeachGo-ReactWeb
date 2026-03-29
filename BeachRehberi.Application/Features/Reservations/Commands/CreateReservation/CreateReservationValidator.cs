using FluentValidation;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationValidator : AbstractValidator<CreateReservationCommand>
{
    public CreateReservationValidator()
    {
        RuleFor(x => x.BeachId)
            .GreaterThan(0).WithMessage("Geçerli bir plaj seçiniz.");

        RuleFor(x => x.ReservationDate)
            .NotEmpty().WithMessage("Rezervasyon tarihi boş olamaz.")
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("Geçmiş tarihli rezervasyon yapılamaz.");

        RuleFor(x => x.GuestCount)
            .GreaterThan(0).WithMessage("Misafir sayısı en az 1 olmalıdır.")
            .LessThanOrEqualTo(50).WithMessage("Tek rezervasyonda en fazla 50 misafir olabilir.");

        RuleFor(x => x.Notes)
            .MaximumLength(500).WithMessage("Notlar en fazla 500 karakter olabilir.")
            .When(x => x.Notes != null);
    }
}
