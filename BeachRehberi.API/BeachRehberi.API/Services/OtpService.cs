using System;
using System.Linq;
using System.Threading.Tasks;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Services;

public class OtpService : IOtpService
{
    private readonly BeachDbContext _db;
    private readonly ILogger<OtpService> _logger;

    public OtpService(BeachDbContext db, ILogger<OtpService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(string email, string purpose)
    {
        if (!Enum.TryParse<OtpPurpose>(purpose, out var purposeEnum))
        {
            throw new ArgumentException("Invalid purpose");
        }

        var oldCodes = await _db.VerificationCodes
            .Where(x => x.Email == email && x.Purpose == purposeEnum && !x.IsUsed)
            .ToListAsync();
            
        foreach(var c in oldCodes)
            c.IsUsed = true;

        var token = Guid.NewGuid().ToString("N");

        var expiry = purpose == "EmailVerification" 
            ? DateTime.UtcNow.AddHours(24) 
            : DateTime.UtcNow.AddMinutes(15);

        var verificationCode = new VerificationCode
        {
            Email = email,
            Code = token,
            Purpose = purposeEnum,
            ExpiresAt = expiry,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.VerificationCodes.Add(verificationCode);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Token generated for {Email}, Purpose: {Purpose}, Token: {Token}", email, purpose, token);
        
        return token;
    }

    public async Task<bool> ValidateTokenAsync(string email, string purpose, string token)
    {
        if (!Enum.TryParse<OtpPurpose>(purpose, out var purposeEnum)) return false;

        var verificationCode = await _db.VerificationCodes
            .Where(x => x.Email == email && x.Code == token && x.Purpose == purposeEnum && !x.IsUsed)
            .FirstOrDefaultAsync();

        if (verificationCode == null || verificationCode.ExpiresAt <= DateTime.UtcNow)
            return false;

        return true;
    }

    public async Task InvalidateTokenAsync(string email, string purpose)
    {
        if (!Enum.TryParse<OtpPurpose>(purpose, out var purposeEnum)) return;

        var codes = await _db.VerificationCodes
            .Where(x => x.Email == email && x.Purpose == purposeEnum && !x.IsUsed)
            .ToListAsync();
        
        foreach(var code in codes)
        {
            code.IsUsed = true;
        }
        await _db.SaveChangesAsync();
    }

    public async Task<string> GenerateOtpAsync(string email, OtpPurpose purpose)
    {
        var oldCodes = await _db.VerificationCodes
            .Where(x => x.Email == email && x.Purpose == purpose && !x.IsUsed)
            .ToListAsync();
            
        foreach(var code in oldCodes)
            code.IsUsed = true;

        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        var verificationCode = new VerificationCode
        {
            Email = email,
            Code = otpCode,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.VerificationCodes.Add(verificationCode);
        await _db.SaveChangesAsync();

        _logger.LogInformation("OTP generated for {Email}, Purpose: {Purpose}, Code: {OtpCode}", email, purpose, otpCode);
        
        return otpCode;
    }

    public async Task<bool> ValidateOtpAsync(string email, string otpCode, OtpPurpose purpose)
    {
        var verificationCode = await _db.VerificationCodes
            .Where(x => x.Email == email && x.Code == otpCode && x.Purpose == purpose && !x.IsUsed)
            .FirstOrDefaultAsync();

        if (verificationCode == null || verificationCode.ExpiresAt <= DateTime.UtcNow)
            return false;

        verificationCode.IsUsed = true;
        await _db.SaveChangesAsync();

        return true;
    }

    // Legacy Support
    public Task<string> SendOtpAsync(string email)
    {
        return GenerateOtpAsync(email, OtpPurpose.EmailVerification);
    }

    public async Task<bool> VerifyOtpAsync(string verificationId, string code)
    {
        var verificationCode = await _db.VerificationCodes
            .Where(x => x.Code == code && x.Purpose == OtpPurpose.EmailVerification && !x.IsUsed)
            .FirstOrDefaultAsync();

        if (verificationCode == null || verificationCode.ExpiresAt <= DateTime.UtcNow)
            return false;

        verificationCode.IsUsed = true;
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsEmailVerifiedAsync(string email)
    {
        return await Task.FromResult(true); 
    }
}