namespace BeachRehberi.API.Services;


public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendToBusinessAsync(int beachId, string message)
    {
        // Firebase FCM entegrasyonu buraya gelecek
        _logger.LogInformation("İşletme {BeachId} bildirimi: {Message}", beachId, message);
        await Task.CompletedTask;
    }

    public async Task SendToUserAsync(string userPhone, string message)
    {
        _logger.LogInformation("Kullanıcı {Phone} bildirimi: {Message}", userPhone, message);
        await Task.CompletedTask;
    }
}