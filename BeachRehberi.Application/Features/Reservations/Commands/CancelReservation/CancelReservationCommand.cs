using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Reservations.Commands.CancelReservation;

// ── Command ───────────────────────────────────────────────────────────────────
public record CancelReservationCommand(
    int ReservationId,
    string? Reason = null
) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────
public class CancelReservationCommandHandler : IRequestHandler<CancelReservationCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public CancelReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await _unitOfWork.Reservations.GetByIdAsync(
            request.ReservationId, cancellationToken);

        if (reservation is null || reservation.IsDeleted)
            throw new NotFoundException("Rezervasyon", request.ReservationId);

        // Sadece kendi rezervasyonunu veya Admin iptal edebilir
        if (!_currentUserService.IsAdmin &&
            reservation.UserId != _currentUserService.UserId)
            throw new ForbiddenException("Bu rezervasyonu iptal etme yetkiniz yok.");

        reservation.Cancel(request.Reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
