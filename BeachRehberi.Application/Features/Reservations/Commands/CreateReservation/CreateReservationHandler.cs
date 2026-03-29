using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationHandler : IRequestHandler<CreateReservationCommand, Result<CreateReservationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateReservationHandler> _logger;

    public CreateReservationHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IEmailService emailService,
        ILogger<CreateReservationHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<CreateReservationResponse>> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
            return Result<CreateReservationResponse>.Unauthorized();

        var beach = await _unitOfWork.Beaches.GetByIdAsync(request.BeachId, cancellationToken);
        if (beach == null || beach.IsDeleted || !beach.IsActive)
            return Result<CreateReservationResponse>.NotFound("Plaj bulunamadı.");

        // Tenant aylık limit kontrolü
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(beach.TenantId, cancellationToken);
        if (tenant != null)
        {
            var thisMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var monthlyCount = await _unitOfWork.Reservations.CountAsync(
                r => r.TenantId == beach.TenantId && r.CreatedAt >= thisMonthStart && !r.IsDeleted,
                cancellationToken);

            if (monthlyCount >= tenant.MaxReservationsPerMonth)
                throw new TenantLimitExceededException($"Aylık rezervasyon limiti ({tenant.MaxReservationsPerMonth})");
        }

        // Aynı günde kapasite kontrolü
        var existingReservations = await _unitOfWork.Reservations.CountAsync(
            r => r.BeachId == request.BeachId &&
                 r.ReservationDate.Date == request.ReservationDate.Date &&
                 r.Status != Domain.Enums.ReservationStatus.Cancelled &&
                 r.Status != Domain.Enums.ReservationStatus.Rejected &&
                 !r.IsDeleted,
            cancellationToken);

        if (existingReservations >= beach.Capacity)
            return Result<CreateReservationResponse>.Failure("Seçilen tarih için plaj kapasitesi dolmuştur.");

        // Fiyat hesaplama
        var totalPrice = beach.HasEntryFee ? beach.EntryFee * request.GuestCount : 0;

        var reservation = new Reservation(
            _currentUser.UserId!.Value,
            request.BeachId,
            beach.TenantId,
            request.ReservationDate,
            request.GuestCount,
            totalPrice,
            request.Notes);

        await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // E-posta bildirimi
        var user = await _unitOfWork.Users.GetByIdAsync(_currentUser.UserId.Value, cancellationToken);
        if (user != null)
        {
            _ = _emailService.SendReservationConfirmationAsync(
                user.Email, user.FullName, beach.Name, request.ReservationDate, cancellationToken);
        }

        _logger.LogInformation(
            "Rezervasyon oluşturuldu: ReservationId={Id}, BeachId={BeachId}, UserId={UserId}",
            reservation.Id, request.BeachId, _currentUser.UserId);

        return Result<CreateReservationResponse>.Created(new CreateReservationResponse(
            reservation.Id,
            beach.Name,
            reservation.ReservationDate,
            reservation.GuestCount,
            reservation.TotalPrice,
            reservation.Status.ToString()
        ));
    }
}
