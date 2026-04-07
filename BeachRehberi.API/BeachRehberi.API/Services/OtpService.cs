using System;
using System.Linq;
using System.Threading.Tasks;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class OtpService : IOtpService
{
    private readonly BeachDbContext _db;
    private readonly ISmsService _smsService;

    public OtpService(BeachDbContext db, ISmsService smsService)
    {
        _db = db;
        _smsService = smsService;
    }

    public async Task<string> SendOtpAsync(string email)
    {
        // Rate limit: aynı telefona son 1 saatte max 3 kod
        var recentCount = await _db.VerificationCodes
            .CountAsync(v => v.Email == email && v.CreatedAt > DateTime.UtcNow.AddHours(-1));

        if (recentCount >= 3)
            throw new InvalidOperationException("Çok fazla doğrulama kodu gönderildi. Lütfen 1 saat sonra tekrar deneyin.");

        var code = GenerateCode();
        var verification = new VerificationCode
        {
            Email = email,
            Code = code,
            ExpireDate = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            Attempts = 0
        };

        _db.VerificationCodes.Add(verification);
        await _db.SaveChangesAsync();

        await _smsService.SendAsync(email, $"BeachGo doğrulama kodunuz: {code}");

        return verification.Id.ToString();
    }

    public async Task<bool> VerifyOtpAsync(string verificationId, string code)
    {
        if (!int.TryParse(verificationId, out var id))
            return false;

        var verification = await _db.VerificationCodes.FindAsync(id);
        if (verification == null) return false;

        verification.Attempts++;

        if (verification.Attempts > 5)
        {
            await _db.SaveChangesAsync();
            return false;
        }

        if (verification.IsUsed || verification.IsExpired)
        {
            await _db.SaveChangesAsync();
            return false;
        }

        if (verification.Code != code)
        {
            await _db.SaveChangesAsync();
            return false;
        }

        verification.IsUsed = true;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> IsEmailVerifiedAsync(string verificationId)
    {
        if (!int.TryParse(verificationId, out var id))
            return false;

        var verification = await _db.VerificationCodes.FindAsync(id);
        return verification is { IsUsed: true };
    }

    private static string GenerateCode()
    {
        return Random.Shared.Next(100000, 999999).ToString();
    }
}
