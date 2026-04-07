using System.Threading.Tasks;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IOtpService
{
    Task<string> GenerateOtpAsync(string email, OtpPurpose purpose);
    Task<bool> ValidateOtpAsync(string email, string otpCode, OtpPurpose purpose);
    
    // New methods requested
    Task<string> GenerateTokenAsync(string email, string purpose);
    Task<bool> ValidateTokenAsync(string email, string purpose, string token);
    Task InvalidateTokenAsync(string email, string purpose);
    
    // Legacy methods
    Task<string> SendOtpAsync(string email);
    Task<bool> VerifyOtpAsync(string verificationId, string code);
    Task<bool> IsEmailVerifiedAsync(string email);
}