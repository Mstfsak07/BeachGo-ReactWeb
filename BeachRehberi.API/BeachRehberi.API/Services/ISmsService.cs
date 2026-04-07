using System.Threading.Tasks;

namespace BeachRehberi.API.Services;

public interface ISmsService
{
    Task<bool> SendAsync(string phone, string message);
}
