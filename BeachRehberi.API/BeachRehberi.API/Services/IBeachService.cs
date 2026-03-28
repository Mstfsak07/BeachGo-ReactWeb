using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IBeachService
{
    Task<PagedResponse<Beach>> GetAllAsync(int page, int pageSize);
    Task<Beach?> GetByIdAsync(int id);
    Task<List<Beach>> SearchAsync(string query);
    Task<List<Beach>> FilterAsync(BeachFilter filter);
    Task<Beach> CreateAsync(Beach beach);
    Task<Beach?> UpdateAsync(int id, Beach beach);
    Task<bool> UpdateOccupancyAsync(int id, int percent, OccupancyLevel level);
    Task<bool> UpdateTodaySpecialAsync(int id, string special);
}
