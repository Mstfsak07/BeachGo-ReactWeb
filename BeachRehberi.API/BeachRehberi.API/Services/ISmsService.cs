using System.Threading.Tasks;

namespace BeachRehberi.API.Services;

public interface ISmsService
{
    Task<bool> SendAsync(string phone, string message);
}

public class MockSmsService : ISmsService
{
    private readonly ILogger<MockSmsService> _logger;

    public MockSmsService(ILogger<MockSmsService> logger)
    {
        _logger = logger;
    }

    public Task<bool> SendAsync(string phone, string message)
    {
        // Mock: SMS gönderilmez, sadece log'a yazılır
        _logger.LogInformation("[MOCK SMS] To: {Phone} | Message: {Message}", phone, message);
        return Task.FromResult(true);
    }
}
