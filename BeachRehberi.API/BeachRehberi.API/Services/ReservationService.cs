using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Models.Enums;

using Microsoft.Extensions.Configuration;

namespace BeachRehberi.API.Services
{
    public class ReservationService : IReservationService
    {
        private readonly BeachDbContext _context;
        private readonly IConfiguration _config;

        public ReservationService(BeachDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<ServiceResult<ReservationResponseDto>> CreateAsync(CreateReservationDto dto, int userId)
        {
            // Ödeme sistemi kontrolü
            var useReal = _config.GetValue<bool>("Features:UseRealPayment");
            if (!useReal)
                return ServiceResult<ReservationResponseDto>.Failure("Şu an ödeme sistemi devre dışı olduğundan rezervasyon yapılamıyor.", 503);

            if (dto.ReservationDate.Date < DateTime.UtcNow.Date)
                return ServiceResult<ReservationResponseDto>.FailureResult("Geçmiş bir tarih için rezervasyon yapılamaz.");

            var beach = await _context.Beaches.FirstOrDefaultAsync(b => b.Id == dto.BeachId && !b.IsDeleted);
            if (beach == null)
                return ServiceResult<ReservationResponseDto>.FailureResult("Plaj bulunamadı.");

            var exists = await _context.Reservations.AnyAsync(r =>
                r.UserId == userId &&
                r.BeachId == dto.BeachId &&
                r.ReservationDate.Date == dto.ReservationDate.Date &&
                !r.IsDeleted &&
                r.Status != ReservationStatus.Cancelled &&
                r.Status != ReservationStatus.Rejected);

            if (exists)
                return ServiceResult<ReservationResponseDto>.FailureResult("Bu plaj için bu tarihte zaten aktif bir rezervasyonunuz bulunmaktadır.");

            var reservation = new Reservation
            {
                UserId = userId,
                BeachId = dto.BeachId,
                ReservationDate = dto.ReservationDate.Date,
                CreatedAt = DateTime.UtcNow,
                Status = ReservationStatus.Pending,
                PersonCount = dto.PersonCount,
                SunbedCount = dto.SunbedCount,
                Notes = dto.Notes,
                TotalPrice = CalculatePrice(beach, dto.PersonCount, dto.SunbedCount)
            };

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            var responseDto = new ReservationResponseDto
            {
                Id = reservation.Id,
                ReservationDate = reservation.ReservationDate,
                Status = reservation.Status.ToString(),
                BeachId = reservation.BeachId,
                BeachName = beach.Name
            };

            return ServiceResult<ReservationResponseDto>.SuccessResult(responseDto, "Rezervasyon başarıyla oluşturuldu.");
        }

        public async Task<List<ReservationListItemDto>> GetByUserAsync(int userId)
        {
            return await _context.Reservations
                .Include(r => r.Beach)
                .Where(r => r.UserId == userId && !r.IsDeleted &&
                            r.Status != ReservationStatus.Cancelled &&
                            r.Status != ReservationStatus.Rejected)
                .OrderByDescending(r => r.ReservationDate)
                .Select(r => new ReservationListItemDto
                {
                    Id = r.Id,
                    BeachName = r.Beach.Name,
                    ReservationDate = r.ReservationDate,
                    CreatedAt = r.CreatedAt,
                    PersonCount = r.PersonCount,
                    SunbedCount = r.SunbedCount,
                    Status = r.Status
                })
                .ToListAsync();
        }

        public async Task<ServiceResult<bool>> CancelAsync(int id, int userId)
        {
            var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            
            if (reservation == null)
                return ServiceResult<bool>.FailureResult("Rezervasyon bulunamadı.");

            // Güvenlik Duvarı: Kullanıcı bir başkasının yetki sınırlarını ihlal ederse net 403 mesajı verilecek
            if (reservation.UserId != userId)
                return ServiceResult<bool>.FailureResult("Bu rezerve işlemi başkasının adına kayıtlı işlemi silme teşebbüsüdür. Erişim izniniz reddedildi.");

            reservation.Cancel();
            await _context.SaveChangesAsync();

            return ServiceResult<bool>.SuccessResult(true, "Rezervasyon iptal edildi.");
        }

    public async Task<ReservationLookupDto?> GetByCodeAsync(string code)
        {
            var r = await _context.Reservations
                .Include(x => x.Beach)
                .Include(x => x.User)
                .FirstOrDefaultAsync(x => x.ConfirmationCode == code && !x.IsDeleted);

            if (r == null) return null;

            return new ReservationLookupDto
            {
                Id = r.Id,
                Code = r.ConfirmationCode ?? "",
                BeachName = r.Beach.Name,
                CustomerName = r.IsGuest ? $"{r.GuestFirstName} {r.GuestLastName}".Trim() : (r.User?.Email ?? ""),
                Pax = r.PersonCount,
                ReservationDate = r.ReservationDate,
                Status = r.Status.ToString(),
                PaymentStatus = r.PaymentStatus.ToString(),
                GuestPhone = r.GuestPhone ?? "",
                GuestEmail = r.GuestEmail ?? ""
            };
        }

        private static decimal CalculatePrice(Beach beach, int personCount, int sunbedCount)
        {
            var personPrice = beach.HasEntryFee ? beach.EntryFee * personCount : 0m;
            var sunbedPrice = beach.SunbedPrice * Math.Max(sunbedCount, 0);
            return personPrice + sunbedPrice;
        }
    }
}
