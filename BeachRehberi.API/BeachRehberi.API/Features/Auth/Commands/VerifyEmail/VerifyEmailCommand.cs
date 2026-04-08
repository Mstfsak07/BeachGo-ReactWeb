using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Email, string Token) : IRequest<AuthResult>;
public record VerifyEmailByTokenCommand(string Token) : IRequest<AuthResult>;

public class VerifyEmailHandler : 
    IRequestHandler<VerifyEmailCommand, AuthResult>,
    IRequestHandler<VerifyEmailByTokenCommand, AuthResult>
{
    private readonly BeachDbContext _db;
    private readonly IOtpService _otpService;

    public VerifyEmailHandler(BeachDbContext db, IOtpService otpService)
    {
        _db = db;
        _otpService = otpService;
    }

    public async Task<AuthResult> Handle(VerifyEmailCommand command, CancellationToken cancellationToken)
    {
        var isValid = await _otpService.ValidateTokenAsync(command.Email, "EmailVerification", command.Token);
        if (!isValid)
            return new AuthResult { Success = false, Message = "Invalid or expired token" };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (user != null)
        {
            user.VerifyEmail();
            await _db.SaveChangesAsync(cancellationToken);
            await _otpService.InvalidateTokenAsync(command.Email, "EmailVerification");
        }

        return new AuthResult { Success = true };
    }

    public async Task<AuthResult> Handle(VerifyEmailByTokenCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Token))
            return new AuthResult { Success = false, Message = "Token gereklidir." };

        var tokenHash = ComputeSha256(command.Token);
        var code = await _db.VerificationCodes
            .Where(c => c.CodeHash == tokenHash
                     && c.Purpose == OtpPurpose.EmailVerification
                     && !c.IsUsed)
            .FirstOrDefaultAsync(cancellationToken);

        if (code == null || code.ExpiresAt <= DateTime.UtcNow)
            return new AuthResult { Success = false, Message = "Doğrulama bağlantısı geçersiz veya süresi dolmuş." };

        return await Handle(new VerifyEmailCommand(code.Email, command.Token), cancellationToken);
    }

    private static string ComputeSha256(string raw)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
        return string.Concat(bytes.Select(b => b.ToString("x2")));
    }
}
