using System;
using System.Linq;
using System.Threading.Tasks;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.API.Services;

public class OtpService : BeachRehberi.Application.Common.Interfaces.IOtpService, IOtpService
{
    private readonly BeachDbContext _db;
    private readonly ILogger<OtpService> _logger;

    public OtpService(BeachDbContext db, ILogger<OtpService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<string> GenerateTokenAsync(string email, string purpose, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<OtpPurpose>(purpose, out var purposeEnum))
        {
            throw new ArgumentException("Invalid purpose");
        }

        var oldCodes = await _db.VerificationCodes
            .Where(x => x.Email == email && x.Purpose == purposeEnum && !x.IsUsed)
            .ToListAsync(cancellationToken);
            
        foreach(var c in oldCodes)
            c.IsUsed = true;

        var token = Guid.NewGuid().ToString("N");

        var expiry = purpose == "EmailVerification" 
            ? DateTime.UtcNow.AddHours(24) 
            : DateTime.UtcNow.AddMinutes(15);

        var verificationCode = new VerificationCode
        {
            Email = email,
            CodeHash = ComputeSha256Hash(token),
            Purpose = purposeEnum,
            ExpiresAt = expiry,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _db.VerificationCodes.Add(verificationCode);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Token generated for {Email}, Purpose: {Purpose}, Token: {Token}", email, purpose, token);
        
        return token;
    }

    // Explicit implementation for API interface
    Task<string> IOtpService.GenerateTokenAsync(string email, string purpose) 
        => GenerateTokenAsync(email, purpose);

    public async Task<bool> ValidateTokenAsync(string email, string purpose, string token, CancellationToken cancellationToken = default)
    {
        var tokenHash = ComputeSha256Hash(token);
        if (!Enum.TryParse<OtpPurpose>(purpose, out var purposeEnum)) return false;

        var verificationCode = await _db.VerificationCodes
            .Where(x => x.Email == email && x.CodeHash == tokenHash && x.Purpose == purposeEnum && !x.IsUsed)
            .FirstOrDefaultAsync(cancellationToken);

        if (verificationCode == null || verificationCode.ExpiresAt <= DateTime.UtcNow)
            return false;

        return true;
    }

    Task<bool> IOtpService.ValidateTokenAsync(string email, string purpose, string token)
        => ValidateTokenAsync(email, purpose, token);

    public async Task InvalidateTokenAsync(string email, string purpose, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<OtpPurpose>(purpose, out var purposeEnum)) return;

        var codes = await _db.VerificationCodes
            .Where(x => x.Email == email && x.Purpose == purposeEnum && !x.IsUsed)
            .ToListAsync(cancellationToken);
        
        foreach(var code in codes)
        {
            code.IsUsed = true;
        }
        await _db.SaveChangesAsync(cancellationToken);
    }

    Task IOtpService.InvalidateTokenAsync(string email, string purpose)
        => InvalidateTokenAsync(email, purpose);

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
            CodeHash = ComputeSha256Hash(otpCode),
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
        var otpHash = ComputeSha256Hash(otpCode);
        var verificationCode = await _db.VerificationCodes
            .Where(x => x.Email == email && x.CodeHash == otpHash && x.Purpose == purpose && !x.IsUsed)
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
        var codeHash = ComputeSha256Hash(code);
        var verificationCode = await _db.VerificationCodes
            .Where(x => x.CodeHash == codeHash && x.Purpose == OtpPurpose.EmailVerification && !x.IsUsed)
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
    private static string ComputeSha256Hash(string rawData)
    {
        using (System.Security.Cryptography.SHA256 sha256Hash = System.Security.Cryptography.SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawData));
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }
}

