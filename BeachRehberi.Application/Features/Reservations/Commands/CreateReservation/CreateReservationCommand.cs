using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

public record CreateReservationCommand(
    int BeachId,
    DateTime ReservationDate,
    int GuestCount,
    string? Notes
) : IRequest<Result<CreateReservationResponse>>;

public record CreateReservationResponse(
    int Id,
    string BeachName,
    DateTime ReservationDate,
    int GuestCount,
    decimal TotalPrice,
    string Status
);
