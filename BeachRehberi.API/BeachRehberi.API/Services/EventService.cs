using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class EventService : IEventService
{
    private readonly BeachDbContext _db;

    public EventService(BeachDbContext db) => _db = db;

    public async Task<List<BeachEvent>> GetAllAsync() =>
        await _db.Events
            .Include(e => e.Beach)
            .Where(e => e.IsActive && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .ToListAsync();

    public async Task<List<BeachEvent>> GetTodayAsync()
    {
        var today = DateTime.UtcNow.Date;
        return await _db.Events
            .Include(e => e.Beach)
            .Where(e => e.IsActive && e.StartDate.Date == today)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
    }

    public async Task<List<BeachEvent>> GetByBeachAsync(int beachId) =>
        await _db.Events
            .Where(e => e.BeachId == beachId && e.IsActive && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
}
