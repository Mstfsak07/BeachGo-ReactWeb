using MediatR;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace BeachRehberi.API.Services;

public record CreateReservationCommand(CreateReservationDto ReservationDto) : IRequest<ServiceResult<Reservation>>;
public record GetMyReservationsQuery() : IRequest<ServiceResult<List<ReservationListItemDto>>>;
public record CancelReservationCommand(string ConfirmationCode) : IRequest<ServiceResult<bool>>;

public class ReservationCommandHandler :
    IRequestHandler<CreateReservationCommand, ServiceResult<Reservation>>,
    IRequestHandler<GetMyReservationsQuery, ServiceResult<List<ReservationListItemDto>>>,
    IRequestHandler<CancelReservationCommand, ServiceResult<bool>>
{
    private readonly IReservationService _reservationService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ReservationCommandHandler(IReservationService reservationService, IHttpContextAccessor httpContextAccessor)
    {
        _reservationService = reservationService;
        _httpContextAccessor = httpContextAccessor;
    }

    private int? GetAuthenticatedUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !user.Identity?.IsAuthenticated == true)
            return null;

        var userIdValue = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdValue, out int userId))
            return userId;

        return null;
    }

    public async Task<ServiceResult<Reservation>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return ServiceResult<Reservation>.FailureResult("Yetkisiz erişim. Lütfen giriş yapın.");

        return await _reservationService.CreateAsync(request.ReservationDto, userId.Value);
    }

    public async Task<ServiceResult<List<ReservationListItemDto>>> Handle(GetMyReservationsQuery request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return ServiceResult<List<ReservationListItemDto>>.FailureResult("Yetkisiz erişim. Kullanıcı kimliği belirlenemedi.");

        var reservations = await _reservationService.GetByUserAsync(userId.Value);
        return ServiceResult<List<ReservationListItemDto>>.SuccessResult(reservations, "Rezervasyonlar getirildi.");
    }

    public async Task<ServiceResult<bool>> Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var userId = GetAuthenticatedUserId();
        if (!userId.HasValue)
            return ServiceResult<bool>.FailureResult("Yetkisiz erişim. Kullanıcı kimliği belirlenemedi.");

        var canCancel = await _reservationService.CancelAsync(request.ConfirmationCode, userId.Value);
        if (!canCancel)
            return ServiceResult<bool>.FailureResult("Rezervasyon iptal edilemedi veya yetkiniz yok.");

        return ServiceResult<bool>.SuccessResult(true, "Rezervasyon iptal edildi.");
    }
}
