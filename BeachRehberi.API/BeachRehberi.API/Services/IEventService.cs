using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IEventService
{
    Task<List<BeachEvent>> GetAllAsync();
    Task<List<BeachEvent>> GetTodayAsync();
    Task<List<BeachEvent>> GetByBeachAsync(int beachId);
}
