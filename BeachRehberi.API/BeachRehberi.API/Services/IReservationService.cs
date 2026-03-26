using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IReservationService
{
    Task<Reservation> CreateAsync(Reservation reservation);
    Task<List<Reservation>> GetByPhoneAsync(string phone);
    Task<Reservation?> GetByCodeAsync(string code);
    Task<bool> CancelAsync(string code);
}