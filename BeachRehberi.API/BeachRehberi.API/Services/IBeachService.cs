using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IBeachService
{
    // Requirement: Pagination
    Task<PagedResponse<Beach>> GetAllAsync(int page, int pageSize);
    Task<Beach?> GetByIdAsync(int id);
    Task<List<Beach>> SearchAsync(string query);
    Task<List<Beach>> FilterAsync(BeachFilter filter);
}
