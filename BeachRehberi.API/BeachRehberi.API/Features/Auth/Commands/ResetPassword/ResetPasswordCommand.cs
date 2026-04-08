using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.ResetPassword;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<AuthResult>;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, AuthResult>
{
    private readonly BeachDbContext _db;
    private readonly IOtpService _otpService;

    public ResetPasswordHandler(BeachDbContext db, IOtpService otpService)
    {
        _db = db;
        _otpService = otpService;
    }

    public async Task<AuthResult> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var isValid = await _otpService.ValidateTokenAsync(command.Email, "PasswordReset", command.Token);
        if (!isValid)
            return new AuthResult { Success = false, Message = "Invalid or expired token" };

        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (user != null)
        {
            user.ChangePassword(BCrypt.Net.BCrypt.HashPassword(command.NewPassword));
            await _db.SaveChangesAsync(cancellationToken);
            await _otpService.InvalidateTokenAsync(command.Email, "PasswordReset");
        }

        return new AuthResult { Success = true };
    }
}
