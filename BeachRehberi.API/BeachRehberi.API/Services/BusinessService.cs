using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs;
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

    public async Task<List<Reservation>> GetAllReservationsAsync(int beachId) =>
        await _db.Reservations
            .Where(r => r.BeachId == beachId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

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
