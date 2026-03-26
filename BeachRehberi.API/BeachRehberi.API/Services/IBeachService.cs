using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IBeachService
{
    Task<List<Beach>> GetAllAsync();
    Task<Beach?> GetByIdAsync(int id);
    Task<List<Beach>> SearchAsync(string query);
    Task<List<Beach>> FilterAsync(BeachFilter filter);
    Task<Beach> CreateAsync(Beach beach);
    Task<Beach?> UpdateAsync(int id, Beach beach);
    Task<bool> UpdateOccupancyAsync(int id, int percent, OccupancyLevel level);
    Task<bool> UpdateTodaySpecialAsync(int id, string special);
}

public class BeachFilter
{
    public double? MinRating { get; set; }
    public bool? HasBar { get; set; }
    public bool? HasWaterSports { get; set; }
    public bool? IsChildFriendly { get; set; }
    public bool? HasPool { get; set; }
    public bool? FreeEntry { get; set; }
    public bool? IsOpen { get; set; }
    public string? SortBy { get; set; }
    public double? UserLat { get; set; }
    public double? UserLng { get; set; }
}