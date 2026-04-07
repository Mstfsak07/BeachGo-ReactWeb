using System.Threading.Tasks;

namespace BeachRehberi.API.Services;

public interface IOtpService
{
    Task<string> SendOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string verificationId, string code);
    Task<bool> IsEmailVerifiedAsync(string verificationId);
}
