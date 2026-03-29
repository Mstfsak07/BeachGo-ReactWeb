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
) : IRequest<PagedResult<MyReservationDto>>;

// ── DTO ───────────────────────────────────────────────────────────────────────
public record MyReservationDto(
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

// ── Handler ───────────────────────────────────────────────────────────────────
public class GetMyReservationsQueryHandler
    : IRequestHandler<GetMyReservationsQuery, PagedResult<MyReservationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public GetMyReservationsQueryHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<PagedResult<MyReservationDto>> Handle(
        GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException("Giriş yapmanız gerekmektedir.");

        var query = _unitOfWork.Reservations.Query()
            .Include(r => r.Beach)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(r => new MyReservationDto(
                r.Id,
                r.Beach!.Name,
                r.Beach.City,
                r.Beach.CoverImageUrl,
                r.ReservationDate,
                r.NumberOfPeople,
                r.TotalPrice,
                r.Status.ToString(),
                r.Notes,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return PagedResult<MyReservationDto>.Create(
            items, totalCount, request.PageNumber, request.PageSize);
    }
}
