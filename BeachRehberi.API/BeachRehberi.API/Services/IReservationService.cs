using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IReservationService
{
    Task<ServiceResult<Reservation>> CreateAsync(Reservation reservation);
    Task<List<Reservation>> GetByUserAsync(int userId);
    Task<Reservation?> GetByCodeAsync(string code);
    Task<bool> CancelAsync(string code, int userId);
}
