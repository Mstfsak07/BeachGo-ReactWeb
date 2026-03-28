using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class BusinessService : IBusinessService
{
    private readonly BeachDbContext _db;       

    public BusinessService(BeachDbContext db) => _db = db;

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
        
        ev.Cancel();
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
        var res = await _db.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.BeachId == beachId);

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
        catch (DomainException ex)
        {
            return ServiceResult<object>.FailureResult(ex.Message);
        }
    }
}

