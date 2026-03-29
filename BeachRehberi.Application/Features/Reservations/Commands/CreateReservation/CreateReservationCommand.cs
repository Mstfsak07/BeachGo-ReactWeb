using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Enums;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

// ── Command ───────────────────────────────────────────────────────────────────
public record CreateReservationCommand(
    int BeachId,
    DateTime ReservationDate,
    int GuestCount,
    string? Notes = null
) : IRequest<int>;

// ── Handler ───────────────────────────────────────────────────────────────────
public class CreateReservationCommandHandler : IRequestHandler<CreateReservationCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    private readonly IEmailService _emailService;

    public CreateReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService,
        IEmailService emailService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
        _emailService = emailService;
    }

    public async Task<int> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId
            ?? throw new UnauthorizedException(
                "Rezervasyon yapabilmek için giriş yapmanız gerekmektedir.");

        // Plaj kontrolü
        var beach = await _unitOfWork.Beaches.GetByIdAsync(request.BeachId, cancellationToken);

        if (beach is null || beach.IsDeleted)
            throw new NotFoundException("Plaj", request.BeachId);

        if (!beach.IsAvailable())
            throw new BusinessRuleException("Bu plaj şu an rezervasyona kapalıdır.");

        // Geçmiş tarih kontrolü
        if (request.ReservationDate.Date < DateTime.UtcNow.Date)
            throw new BusinessRuleException("Geçmiş tarihli rezervasyon yapılamaz.");

        // Aynı gün + aynı plaj için mükerrer rezervasyon kontrolü
        var alreadyExists = await _unitOfWork.Reservations.AnyAsync(
            r => r.UserId == userId &&
                 r.BeachId == request.BeachId &&
                 r.ReservationDate.Date == request.ReservationDate.Date &&
                 r.Status != ReservationStatus.Cancelled &&
                 r.Status != ReservationStatus.Rejected &&
                 !r.IsDeleted,
            cancellationToken);

        if (alreadyExists)
            throw new BusinessRuleException(
                "Bu plaj için seçilen tarihe ait aktif bir rezervasyonunuz zaten bulunmaktadır.");

        // Rezervasyon oluştur
        var reservation = new Reservation(
            userId,
            beach.Id,
            beach.TenantId,
            request.ReservationDate,
            request.GuestCount,
            beach.PricePerPerson,
            request.Notes);

        await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // E-posta bildirimi (hata rezervasyonu engellemez)
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is not null)
        {
            try
            {
                await _emailService.SendReservationConfirmationAsync(
                    user.Email,
                    user.FullName,
                    beach.Name,
                    request.ReservationDate,
                    cancellationToken);
            }
            catch
            {
                // E-posta hatası iş akışını engellemez
            }
        }

        return reservation.Id;
    }
}
