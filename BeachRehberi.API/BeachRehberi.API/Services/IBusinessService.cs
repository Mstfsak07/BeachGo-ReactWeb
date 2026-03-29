using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.Services;

public interface IBusinessService
{
    Task<List<BeachEvent>> GetEventsAsync(int beachId);
    Task<BeachEvent> AddEventAsync(BeachEvent ev);
    Task<bool> DeleteEventAsync(int eventId, int beachId);
    Task<List<Reservation>> GetReservationsAsync(int beachId, DateTime date);
    Task<List<Reservation>> GetAllReservationsAsync(int beachId);
    Task<ServiceResult<object>> UpdateReservationStatusAsync(int id, int beachId, ReservationStatus status, string? comment = null);
}
