using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Application.Features.Reservations.Queries.GetMyReservations;

// ── Query ─────────────────────────────────────────────────────────────────────
public record GetMyReservationsQuery(
    int PageNumber = 1,
    int PageSize = 10
) : IRequest<Result<PagedResult<ReservationListItemDto>>>;

// ── DTO ───────────────────────────────────────────────────────────────────────
public record ReservationListItemDto(
    int Id,
    string BeachName,
    string BeachCity,
    string? BeachCoverImage,
    DateTime ReservationDate,
    int GuestCount,
    decimal TotalPrice,
    string Status,
    string? Notes,
    DateTime CreatedAt
);