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

    public GuestReservationService(BeachDbContext db, IOtpService otpService)
    {
        _db = db;
        _otpService = otpService;
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
            UserId = 0, // Guest — no user
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
        };

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        return ServiceResult<GuestReservationResponseDto>.SuccessResult(new GuestReservationResponseDto
        {
            ReservationId = reservation.Id,
            ConfirmationCode = confirmationCode,
            Status = reservation.Status.ToString(),
            TotalPrice = price
        }, "Rezervasyon başarıyla oluşturuldu.");
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
