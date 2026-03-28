using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;

namespace BeachRehberi.API.Services;

public interface IReservationService
{
    Task<ServiceResult<Reservation>> CreateAsync(CreateReservationDto dto, int userId);
    Task<List<ReservationListItemDto>> GetByUserAsync(int userId);
    Task<Reservation?> GetByCodeAsync(string code);
    Task<bool> CancelAsync(string code, int userId);
}

