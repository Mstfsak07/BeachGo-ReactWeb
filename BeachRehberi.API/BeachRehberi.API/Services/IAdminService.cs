using BeachRehberi.API.DTOs;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IAdminService
{
    Task<AdminStatsDto> GetGlobalStatsAsync();
    Task<List<AdminBeachListItemDto>> GetAllBeachesAsync();
    Task<List<AdminUserListItemDto>> GetAllUsersAsync();
    Task<List<AdminReservationListItemDto>> GetAllReservationsAsync();
    Task<Beach?> GetBeachByIdAsync(int id);
    Task<bool> ToggleBeachStatusAsync(int id);
    Task<bool> UpdateBeachAsync(int id, UpdateBeachDto dto);
    Task<AdminBeachImportResultDto> ImportBeachesAsync(List<UpdateBeachDto> beaches);
}
