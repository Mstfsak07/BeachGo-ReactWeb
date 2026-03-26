using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IBusinessService
{
    Task<List<BeachEvent>> GetEventsAsync(int beachId);
    Task<BeachEvent> AddEventAsync(BeachEvent ev);
    Task<bool> DeleteEventAsync(int eventId, int beachId);
    Task<List<Reservation>> GetReservationsAsync(int beachId, DateTime date);
}