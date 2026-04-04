using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class BusinessService : IBusinessService
{
    private readonly BeachDbContext _db;

    public BusinessService(BeachDbContext db) => _db = db;

    public async Task<Beach?> GetBeachByIdAsync(int beachId) =>
        await _db.Beaches
            .Include(b => b.Photos)
            .Include(b => b.Reviews)
            .FirstOrDefaultAsync(b => b.Id == beachId && !b.IsDeleted);

    public async Task<ServiceResult<object>> UpdateBeachDetailsAsync(int beachId, UpdateBeachDto dto)
    {
        var beach = await _db.Beaches
            .FirstOrDefaultAsync(b => b.Id == beachId && !b.IsDeleted);

        if (beach == null)
            return ServiceResult<object>.FailureResult("Plaj bulunamadı.");

        try
        {
            beach.Name            = dto.Name;
            beach.Description     = dto.Description;
            beach.Address         = dto.Address;
            beach.Phone           = dto.Phone;
            beach.Website         = dto.Website;
            beach.Instagram       = dto.Instagram;
            beach.OpenTime        = dto.OpenTime;
            beach.CloseTime       = dto.CloseTime;
            beach.HasEntryFee     = dto.HasEntryFee;
            beach.EntryFee        = dto.EntryFee;
            beach.SunbedPrice     = dto.SunbedPrice;
            beach.Latitude        = dto.Latitude;
            beach.Longitude       = dto.Longitude;
            beach.Capacity        = dto.Capacity;

            // Olanaklar
            beach.HasSunbeds       = dto.HasSunbeds;
            beach.HasShower        = dto.HasShower;
            beach.HasParking       = dto.HasParking;
            beach.HasRestaurant    = dto.HasRestaurant;
            beach.HasBar           = dto.HasBar;
            beach.HasAlcohol       = dto.HasAlcohol;
            beach.IsChildFriendly  = dto.IsChildFriendly;
            beach.HasWaterSports   = dto.HasWaterSports;
            beach.HasWifi          = dto.HasWifi;
            beach.HasPool          = dto.HasPool;
            beach.HasDJ            = dto.HasDJ;
            beach.HasAccessibility = dto.HasAccessibility;

            beach.TodaySpecial     = dto.TodaySpecial;
            beach.LastUpdated      = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return ServiceResult<object>.SuccessResult(null!, "Plaj bilgileri güncellendi.");
        }
        catch (Exception ex)
        {
            return ServiceResult<object>.FailureResult(ex.Message);
        }
    }

    public async Task<List<BeachEvent>> GetEventsAsync(int beachId) =>
        await _db.Events
            .Where(e => e.BeachId == beachId && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

    public async Task<BeachEvent> AddEventAsync(BeachEvent ev)
    {
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return ev;
    }

    public async Task<bool> DeleteEventAsync(int eventId, int beachId)
    {
        var ev = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == eventId && e.BeachId == beachId);

        if (ev == null) return false;

        ev.SoftDelete();
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Reservation>> GetReservationsAsync(int beachId, DateTime date) =>
        await _db.Reservations
            .Where(r => r.BeachId == beachId &&
                        r.ReservationDate.Date == date.Date &&
                        r.Status != ReservationStatus.Cancelled)
            .OrderBy(r => r.ReservationDate)
            .ToListAsync();

    public async Task<List<BusinessReservationDto>> GetAllReservationsAsync(int beachId) =>
        await _db.Reservations
            .Include(r => r.User)
            .Include(r => r.Beach)
            .Where(r => r.BeachId == beachId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new BusinessReservationDto
            {
                Id = r.Id,
                UserEmail = r.User != null ? r.User.Email : (r.GuestEmail ?? ""),
                ReservationDate = r.ReservationDate,
                PersonCount = r.PersonCount,
                SunbedCount = r.SunbedCount,
                Status = r.Status.ToString(),
                CreatedAt = r.CreatedAt,
                IsGuest = r.IsGuest,
                GuestName = (r.GuestFirstName + " " + r.GuestLastName).Trim(),
                GuestPhone = r.GuestPhone ?? "",
                GuestEmail = r.GuestEmail ?? "",
                PaymentStatus = r.PaymentStatus ?? "Mock",
                
                IsGuestReservation = r.IsGuest,
                ConfirmationCode = r.ConfirmationCode ?? "",
                CustomerName = r.IsGuest ? (r.GuestFirstName + " " + r.GuestLastName).Trim() : (r.User != null ? (r.User.ContactName ?? r.User.Email) : "Bilinmeyen"),
                Phone = r.IsGuest ? (r.GuestPhone ?? "") : "",

                BeachName = r.Beach != null ? r.Beach.Name : "",
                TotalPrice = r.TotalPrice,
                
                EmailSent = r.IsGuest && _db.VerificationCodes.Any(v => v.Email == r.GuestEmail),
                EmailVerified = r.IsGuest && _db.VerificationCodes.Any(v => v.Email == r.GuestEmail && v.IsUsed),
                EmailLastSentTime = r.IsGuest ? _db.VerificationCodes.Where(v => v.Email == r.GuestEmail).Max(v => (DateTime?)v.CreatedAt) : null,

                PaymentCreatedAt = _db.ReservationPayments.Where(p => p.ReservationId == r.Id).Select(p => (DateTime?)p.CreatedAt).FirstOrDefault(),
                CancelledAt = r.CancelledAt
            })
            .ToListAsync();

    public async Task<BusinessStatsDto> GetStatsAsync(int beachId)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);
        var weekStart = today.AddDays(-6);

        var total = await _db.Reservations
            .CountAsync(r => r.BeachId == beachId && !r.IsDeleted);

        var todayCheckins = await _db.Reservations
            .CountAsync(r => r.BeachId == beachId && !r.IsDeleted && r.ReservationDate.Date == today);

        var monthly = await _db.Reservations
            .CountAsync(r => r.BeachId == beachId && !r.IsDeleted && r.ReservationDate.Date >= monthStart);

        var activeCustomers = await _db.Reservations
            .Where(r => r.BeachId == beachId && !r.IsDeleted &&
                        (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Approved))
            .Select(r => r.UserId)
            .Distinct()
            .CountAsync();

        var estimatedEarnings = await _db.Reservations
            .Where(r => r.BeachId == beachId && !r.IsDeleted)
            .SumAsync(r => r.TotalPrice);

        var weeklyRaw = await _db.Reservations
            .Where(r => r.BeachId == beachId && !r.IsDeleted && r.ReservationDate.Date >= weekStart)
            .GroupBy(r => r.ReservationDate.Date)
            .Select(g => new { Date = g.Key, Count = g.Count() })
            .ToListAsync();

        var dayNames = new[] { "Paz", "Pzt", "Sal", "Çar", "Per", "Cum", "Cmt" };
        var weeklyData = Enumerable.Range(0, 7)
            .Select(i =>
            {
                var date = weekStart.AddDays(i);
                var count = weeklyRaw.FirstOrDefault(w => w.Date == date)?.Count ?? 0;
                return new WeeklyStatDto { Day = dayNames[(int)date.DayOfWeek], Count = count };
            })
            .ToList();

        return new BusinessStatsDto
        {
            TotalReservations = total,
            TodayCheckins = todayCheckins,
            MonthlyReservations = monthly,
            ActiveCustomers = activeCustomers,
            EstimatedEarnings = estimatedEarnings,
            WeeklyData = weeklyData
        };
    }

    public async Task<ServiceResult<object>> UpdateReservationStatusAsync(int id, int beachId, ReservationStatus status, string? comment = null)
    {
        var res = await _db.Reservations
            .FirstOrDefaultAsync(r => r.Id == id && r.BeachId == beachId);

        if (res == null)
            return ServiceResult<object>.FailureResult("Rezervasyon bulunamadı.");

        try
        {
            switch (status)
            {
                case ReservationStatus.Approved:
                    res.Approve(comment);
                    break;
                case ReservationStatus.Rejected:
                    res.Reject(comment);
                    break;
                case ReservationStatus.Cancelled:
                    res.Cancel();
                    break;
                case ReservationStatus.NoShow:
                    res.MarkAsNoShow();
                    break;
                case ReservationStatus.Completed:
                    res.MarkAsCompleted();
                    break;
                default:
                    return ServiceResult<object>.FailureResult("Geçersiz durum değişimi.");
            }

            await _db.SaveChangesAsync();
            return ServiceResult<object>.SuccessResult(null!, $"Rezervasyon {status} durumuna getirildi.");
        }
        catch (Exception ex)
        {
            return ServiceResult<object>.FailureResult(ex.Message);
        }
    }
}
