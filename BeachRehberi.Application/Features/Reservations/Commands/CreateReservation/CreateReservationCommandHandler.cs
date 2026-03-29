using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Reservations.Commands.CreateReservation;

public class CreateReservationCommandHandler
    : IRequestHandler<CreateReservationCommand, Result<CreateReservationResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IEmailService _emailService;
    private readonly ILogger<CreateReservationCommandHandler> _logger;

    public CreateReservationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IEmailService emailService,
        ILogger<CreateReservationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<CreateReservationResponse>> Handle(
        CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId
            ?? throw new UnauthorizedException("Bu işlem için giriş yapmanız gerekiyor.");

        // Beach kontrolü
        var beach = await _unitOfWork.Beaches.GetByIdAsync(request.BeachId, cancellationToken)
            ?? throw new NotFoundException("Beach", request.BeachId);

        if (!beach.IsActive)
            throw new BusinessRuleException("Bu beach şu an rezervasyona kapalıdır.");

        // Tenant & abonelik kontrolü
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(beach.TenantId, cancellationToken)
            ?? throw new NotFoundException("Tenant", beach.TenantId);

        if (!tenant.IsActive)
            throw new BusinessRuleException("Bu beach sahibinin hesabı aktif değil.");

        if (!tenant.IsSubscriptionActive())
            throw new BusinessRuleException("Bu beach sahibinin aboneliği sona ermiştir.");

        // Aylık rezervasyon limiti kontrolü
        var thisMonthStart = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var monthlyCount = await _unitOfWork.Reservations.CountAsync(
            r => r.TenantId == beach.TenantId
                 && r.CreatedAt >= thisMonthStart
                 && !r.IsDeleted,
            cancellationToken);

        if (monthlyCount >= tenant.MaxReservationsPerMonth)
            throw new TenantLimitExceededException("Aylık rezervasyon sayısı");

        // Tarih kontrolü
        if (request.ReservationDate.Date < DateTime.UtcNow.Date)
            throw new BusinessRuleException("Geçmiş bir tarih için rezervasyon yapılamaz.");

        var totalPrice = beach.PricePerPerson * request.GuestCount;

        var reservation = new Reservation(
            userId: userId,
            beachId: request.BeachId,
            tenantId: beach.TenantId,
            reservationDate: request.ReservationDate,
            guestCount: request.GuestCount,
            totalPrice: totalPrice,
            notes: request.Notes
        );

        await _unitOfWork.Reservations.AddAsync(reservation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Onay e-postası (fire and forget)
        var user = await _unitOfWork.Users.GetByIdAsync(userId, cancellationToken);
        if (user is not null)
        {
            _ = _emailService.SendReservationConfirmationAsync(
                user.Email, user.FullName, beach.Name,
                request.ReservationDate, cancellationToken);
        }

        _logger.LogInformation(
            "Rezervasyon oluşturuldu → Id: {Id}, UserId: {UserId}, BeachId: {BeachId}, Tarih: {Date}",
            reservation.Id, userId, request.BeachId, request.ReservationDate.ToString("dd.MM.yyyy"));

        return Result<CreateReservationResponse>.Created(
            new CreateReservationResponse(
                Id: reservation.Id,
                BeachName: beach.Name,
                ReservationDate: reservation.ReservationDate,
                GuestCount: reservation.GuestCount,
                TotalPrice: reservation.TotalPrice,
                Status: reservation.Status.ToString()),
            "Rezervasyon başarıyla oluşturuldu.");
    }
}
