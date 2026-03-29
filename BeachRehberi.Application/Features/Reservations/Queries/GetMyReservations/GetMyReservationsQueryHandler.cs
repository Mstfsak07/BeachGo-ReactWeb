using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Application.Features.Reservations.Queries.GetMyReservations;

public class GetMyReservationsQueryHandler
    : IRequestHandler<GetMyReservationsQuery, Result<PagedResult<ReservationListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public GetMyReservationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<PagedResult<ReservationListItemDto>>> Handle(
        GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("Bu işlem için giriş yapmanız gerekiyor.");

        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var pageNumber = Math.Max(request.PageNumber, 1);

        var query = _unitOfWork.Reservations.Query()
            .Include(r => r.Beach)
                .ThenInclude(b => b!.Photos)
            .Where(r => r.UserId == userId && !r.IsDeleted);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        query = query.OrderByDescending(r => r.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new ReservationListItemDto(
                r.Id,
                r.Beach != null ? r.Beach.Name : "Bilinmiyor",
                r.Beach != null ? r.Beach.CoverImageUrl : null,
                r.Beach != null ? r.Beach.City : string.Empty,
                r.ReservationDate,
                r.GuestCount,
                r.TotalPrice,
                r.Status.ToString(),
                r.IsPaid,
                r.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<PagedResult<ReservationListItemDto>>.Success(
            PagedResult<ReservationListItemDto>.Create(items, totalCount, pageNumber, pageSize));
    }
}
