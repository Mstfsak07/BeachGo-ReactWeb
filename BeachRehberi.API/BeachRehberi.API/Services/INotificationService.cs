namespace BeachRehberi.API.Services;

public interface INotificationService
{
    Task SendToBusinessAsync(int beachId, string message);
    Task SendToUserAsync(string userPhone, string message);
}