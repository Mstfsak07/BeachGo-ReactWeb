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
        ev.CreatedAt = DateTime.UtcNow;
        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
        return ev;
    }

    public async Task<bool> DeleteEventAsync(int eventId, int beachId)
    {
        var ev = await _db.Events
            .FirstOrDefaultAsync(e => e.Id == eventId && e.BeachId == beachId);
        if (ev == null) return false;
        ev.IsActive = false;
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
}