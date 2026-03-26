using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class BeachService : IBeachService
{
    private readonly BeachDbContext _db;

    public BeachService(BeachDbContext db) => _db = db;

    public async Task<List<Beach>> GetAllAsync() =>
        await _db.Beaches
            .Include(b => b.Events.Where(e => e.IsActive && e.StartDate >= DateTime.UtcNow))
            .Include(b => b.Photos)
            .OrderByDescending(b => b.Rating)
            .ToListAsync();

    public async Task<Beach?> GetByIdAsync(int id) =>
        await _db.Beaches
            .Include(b => b.Events.Where(e => e.IsActive))
            .Include(b => b.Photos)
            .Include(b => b.Reviews.Where(r => r.IsApproved)
                .OrderByDescending(r => r.CreatedAt).Take(10))
            .FirstOrDefaultAsync(b => b.Id == id);

    public async Task<List<Beach>> SearchAsync(string query) =>
        await _db.Beaches
            .Where(b => b.Name.ToLower().Contains(query.ToLower()) ||
                        b.Description.ToLower().Contains(query.ToLower()))
            .ToListAsync();

    public async Task<List<Beach>> FilterAsync(BeachFilter filter)
    {
        var query = _db.Beaches.Include(b => b.Events).AsQueryable();

        if (filter.MinRating.HasValue)
            query = query.Where(b => b.Rating >= filter.MinRating.Value);
        if (filter.HasBar.HasValue)
            query = query.Where(b => b.HasBar == filter.HasBar.Value);
        if (filter.HasWaterSports.HasValue)
            query = query.Where(b => b.HasWaterSports == filter.HasWaterSports.Value);
        if (filter.IsChildFriendly.HasValue)
            query = query.Where(b => b.IsChildFriendly == filter.IsChildFriendly.Value);
        if (filter.HasPool.HasValue)
            query = query.Where(b => b.HasPool == filter.HasPool.Value);
        if (filter.FreeEntry.HasValue && filter.FreeEntry.Value)
            query = query.Where(b => !b.HasEntryFee);
        if (filter.IsOpen.HasValue)
            query = query.Where(b => b.IsOpen == filter.IsOpen.Value);

        var list = await query.ToListAsync();

        if (filter.SortBy == "distance" && filter.UserLat.HasValue && filter.UserLng.HasValue)
            list = list.OrderBy(b => GetDistance(
                filter.UserLat.Value, filter.UserLng.Value,
                b.Latitude, b.Longitude)).ToList();
        else if (filter.SortBy == "occupancy")
            list = list.OrderBy(b => b.OccupancyPercent).ToList();
        else
            list = list.OrderByDescending(b => b.Rating).ToList();

        return list;
    }

    public async Task<Beach> CreateAsync(Beach beach)
    {
        _db.Beaches.Add(beach);
        await _db.SaveChangesAsync();
        return beach;
    }

    public async Task<Beach?> UpdateAsync(int id, Beach beach)
    {
        var existing = await _db.Beaches.FindAsync(id);
        if (existing == null) return null;

        existing.Name = beach.Name;
        existing.Description = beach.Description;
        existing.Phone = beach.Phone;
        existing.OpenTime = beach.OpenTime;
        existing.CloseTime = beach.CloseTime;
        existing.EntryFee = beach.EntryFee;
        existing.SunbedPrice = beach.SunbedPrice;
        existing.HasBar = beach.HasBar;
        existing.HasRestaurant = beach.HasRestaurant;
        existing.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> UpdateOccupancyAsync(int id, int percent, OccupancyLevel level)
    {
        var beach = await _db.Beaches.FindAsync(id);
        if (beach == null) return false;

        beach.OccupancyPercent = Math.Clamp(percent, 0, 100);
        beach.OccupancyLevel = level;
        beach.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateTodaySpecialAsync(int id, string special)
    {
        var beach = await _db.Beaches.FindAsync(id);
        if (beach == null) return false;

        beach.TodaySpecial = special;
        beach.LastUpdated = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    private static double GetDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var d1 = lat1 * Math.PI / 180.0;
        var d2 = lat2 * Math.PI / 180.0;
        var d3 = (lat2 - lat1) * Math.PI / 180.0;
        var d4 = (lon2 - lon1) * Math.PI / 180.0;
        var a = Math.Sin(d3 / 2) * Math.Sin(d3 / 2) +
                Math.Cos(d1) * Math.Cos(d2) * Math.Sin(d4 / 2) * Math.Sin(d4 / 2);
        return 6371 * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}