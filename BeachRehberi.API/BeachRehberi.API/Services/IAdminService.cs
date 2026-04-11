using BeachRehberi.API.DTOs;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAdminService
{
    Task<AdminStatsDto> GetGlobalStatsAsync();
    Task<List<AdminBeachListItemDto>> GetAllBeachesAsync(int page, int pageSize);
    Task<List<AdminUserListItemDto>> GetAllUsersAsync(int page, int pageSize);
    Task<List<AdminReservationListItemDto>> GetAllReservationsAsync(int page, int pageSize);
    Task<Beach?> GetBeachByIdAsync(int id);
    Task<bool> ToggleBeachStatusAsync(int id);
    Task<bool> UpdateBeachAsync(int id, UpdateBeachDto dto);
    Task<AdminBeachImportResultDto> ImportBeachesAsync(List<UpdateBeachDto> beaches);
}
