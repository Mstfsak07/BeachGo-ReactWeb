using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<AuthResult>;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordCommand, AuthResult>
{
    private readonly BeachDbContext _db;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ForgotPasswordHandler(BeachDbContext db, IOtpService otpService, IEmailService emailService)
    {
        _db = db;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<AuthResult> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (user != null)
        {
            var token = await _otpService.GenerateTokenAsync(command.Email, "PasswordReset");
            var displayName = $"{user.FirstName} {user.LastName}".Trim();
            await _emailService.SendPasswordResetAsync(command.Email, displayName, token);
        }

        return new AuthResult { Success = true };
    }
}
