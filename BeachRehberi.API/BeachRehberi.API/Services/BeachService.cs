using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class BeachService : IBeachService
{
    private readonly BeachDbContext _db;
    private readonly IGeoCalculator _geoCalculator;

    public BeachService(BeachDbContext db, IGeoCalculator geoCalculator)
    {
        _db = db;
        _geoCalculator = geoCalculator;
    }

    public async Task<PagedResponse<Beach>> GetAllAsync(int page, int pageSize)
    {
        var query = _db.Beaches
            .Include(b => b.Events.Where(e => e.IsActive && e.StartDate >= DateTime.UtcNow))
            .Include(b => b.Photos)
            .OrderByDescending(b => b.Rating);

        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResponse<Beach>(items, total, page, pageSize);
    }

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
            list = list.OrderBy(b => _geoCalculator.GetDistance(
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

        try 
        {
            existing.UpdateDetails(beach.Name, beach.Description, beach.Address);
            existing.UpdateFees(beach.HasEntryFee, beach.EntryFee, beach.SunbedPrice);
            // ... other updates could be moved to entity methods too
            
            await _db.SaveChangesAsync();
            return existing;
        }
        catch (DomainException)
        {
            return null;
        }
    }

    public async Task<bool> UpdateOccupancyAsync(int id, int percent, OccupancyLevel level)
    {
        var beach = await _db.Beaches.FindAsync(id);
        if (beach == null) return false;

        try 
        {
            beach.UpdateOccupancy(percent, level);
            await _db.SaveChangesAsync();
            return true;
        }
        catch 
        {
            return false;
        }
    }

    public async Task<bool> UpdateTodaySpecialAsync(int id, string special)
    {
        var beach = await _db.Beaches.FindAsync(id);
        if (beach == null) return false;

        beach.SetTodaySpecial(special);
        await _db.SaveChangesAsync();
        return true;
    }
}

