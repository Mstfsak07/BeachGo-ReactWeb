using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Enums;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using BeachRehberi.Domain.Common;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

// ── Command ───────────────────────────────────────────────────────────────────
public record CreateReservationCommand(
    int BeachId,
    DateTime ReservationDate,
    int GuestCount,
    string? Notes = null
) : IRequest<Result<CreateReservationResponse>>;

public record CreateReservationResponse(
    int Id,
    string BeachName,
    DateTime ReservationDate,
    int GuestCount,
    decimal TotalPrice,
    string Status
);
