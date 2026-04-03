using System;
using System.Threading.Tasks;
using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class GuestReservationService : IGuestReservationService
{
    private readonly BeachDbContext _db;
    private readonly IOtpService _otpService;
    private readonly IPaymentService _paymentService;

    public GuestReservationService(BeachDbContext db, IOtpService otpService, IPaymentService paymentService)
    {
        _db = db;
        _otpService = otpService;
        _paymentService = paymentService;
    }

    public async Task<ServiceResult<GuestReservationResponseDto>> CreateAsync(CreateGuestReservationDto dto)
    {
        // 1. Telefon doğrulaması kontrolü
        var isVerified = await _otpService.IsPhoneVerifiedAsync(dto.VerificationId);
        if (!isVerified)
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Telefon doğrulaması tamamlanmamış.");

        // 2. Beach kontrolü
        var beach = await _db.Beaches.FindAsync(dto.BeachId);
        if (beach == null)
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Plaj bulunamadı.");

        // 3. Tarih kontrolü
        if (dto.ReservationDate.Date < DateTime.UtcNow.Date)
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Geçmiş bir tarihe rezervasyon yapılamaz.");

        // 4. Fiyat hesapla
        var price = CalculatePrice(beach, dto.ReservationType, dto.PersonCount);

        // 5. Onay kodu üret
        var confirmationCode = GenerateConfirmationCode();

        // 6. Saat parse
        TimeSpan? resTime = null;
        if (TimeSpan.TryParse(dto.ReservationTime, out var parsed))
            resTime = parsed;

        // 7. Reservation oluştur
        var reservation = new Reservation
        {
            BeachId = dto.BeachId,
            UserId = beach.OwnerId, // Guest
            ReservationDate = dto.ReservationDate,
            PersonCount = dto.PersonCount,
            SunbedCount = 0,
            Notes = dto.Note,
            TotalPrice = price,
            Status = ReservationStatus.Pending,
            IsGuest = true,
            GuestFirstName = dto.FirstName,
            GuestLastName = dto.LastName,
            GuestPhone = dto.Phone,
            GuestEmail = dto.Email,
            ConfirmationCode = confirmationCode,
            ReservationType = dto.ReservationType,
            ReservationTime = resTime,
            PaymentStatus = "Pending"
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return ServiceResult<GuestReservationResponseDto>.SuccessResult(new GuestReservationResponseDto
        {
            ReservationId = reservation.Id,
            ConfirmationCode = confirmationCode,
            Status = reservation.Status.ToString(),
            TotalPrice = price,
            PaymentStatus = reservation.PaymentStatus
        }, "Rezervasyon başarıyla oluşturuldu.");
    }

    public async Task<ServiceResult<GuestReservationDetailDto>> GetByConfirmationCodeAsync(string confirmationCode)
    {
        var reservation = await _db.Reservations
            .Include(r => r.Beach)
            .FirstOrDefaultAsync(r => r.ConfirmationCode == confirmationCode && r.IsGuest);

        if (reservation == null)
            return ServiceResult<GuestReservationDetailDto>.FailureResult("Rezervasyon bulunamadı.");

        return ServiceResult<GuestReservationDetailDto>.SuccessResult(new GuestReservationDetailDto
        {
            ConfirmationCode = reservation.ConfirmationCode!,
            GuestName = $"{reservation.GuestFirstName} {reservation.GuestLastName}",
            BeachName = reservation.Beach?.Name ?? "Bilinmiyor",
            ReservationDate = reservation.ReservationDate.ToString("yyyy-MM-dd"),
            ReservationTime = reservation.ReservationTime?.ToString(@"hh\:mm") ?? "Belirtilmemiş",
            PersonCount = reservation.PersonCount,
            ReservationType = reservation.ReservationType ?? "Bilinmiyor",
            Status = reservation.Status.ToString()
        });
    }

    public async Task<ServiceResult<GuestReservationResponseDto>> CancelAsync(string confirmationCode)
    {
        var reservation = await _db.Reservations.FirstOrDefaultAsync(r => r.ConfirmationCode == confirmationCode && r.IsGuest);
        if (reservation == null)
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Rezervasyon bulunamadı.");

        if (reservation.Status == ReservationStatus.Cancelled)
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Rezervasyon zaten iptal edilmiş.");

        if (reservation.Status != ReservationStatus.Pending && reservation.Status.ToString() != "Waiting")
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Sadece Pending veya Waiting durumundaki rezervasyonlar iptal edilebilir.");

        reservation.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();

        return ServiceResult<GuestReservationResponseDto>.SuccessResult(new GuestReservationResponseDto
        {
            ReservationId = reservation.Id,
            ConfirmationCode = reservation.ConfirmationCode,
            Status = reservation.Status.ToString(),
            TotalPrice = reservation.TotalPrice,
            PaymentStatus = reservation.PaymentStatus
        }, "Rezervasyon başarıyla iptal edildi.");
    }

    public async Task<ServiceResult<GuestReservationResponseDto>> MockPayAsync(string confirmationCode)
    {
        var reservation = await _db.Reservations.FirstOrDefaultAsync(r => r.ConfirmationCode == confirmationCode && r.IsGuest);
        if (reservation == null)
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Rezervasyon bulunamadı.");

        if (reservation.PaymentStatus == "Paid")
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Bu rezervasyon zaten ödenmiş.");

        var paymentResult = await _paymentService.ProcessPaymentAsync(reservation.Id, reservation.TotalPrice);
        
        if (!paymentResult)
            return ServiceResult<GuestReservationResponseDto>.FailureResult("Ödeme işlemi başarısız oldu.");

        reservation.PaymentStatus = "Paid";
        await _db.SaveChangesAsync();

        return ServiceResult<GuestReservationResponseDto>.SuccessResult(new GuestReservationResponseDto
        {
            ReservationId = reservation.Id,
            ConfirmationCode = reservation.ConfirmationCode!,
            Status = reservation.Status.ToString(),
            TotalPrice = reservation.TotalPrice,
            PaymentStatus = reservation.PaymentStatus
        }, "Ödeme başarılı.");
    }

    private static decimal CalculatePrice(Beach beach, string reservationType, int personCount)
    {
        // Mock fiyat hesaplama — gerçek implementasyon ileride
        var basePrice = beach.EntryFee > 0 ? beach.EntryFee : 0;
        return basePrice * personCount;
    }

    private static string GenerateConfirmationCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[8];
        for (int i = 0; i < code.Length; i++)
            code[i] = chars[Random.Shared.Next(chars.Length)];
        return new string(code);
    }
}
