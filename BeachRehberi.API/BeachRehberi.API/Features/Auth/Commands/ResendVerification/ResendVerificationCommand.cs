using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.ResendVerification;

public record ResendVerificationCommand(string Email) : IRequest<AuthResult>;

public class ResendVerificationHandler : IRequestHandler<ResendVerificationCommand, AuthResult>
{
    private readonly BeachDbContext _db;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public ResendVerificationHandler(BeachDbContext db, IOtpService otpService, IEmailService emailService)
    {
        _db = db;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<AuthResult> Handle(ResendVerificationCommand command, CancellationToken cancellationToken)
    {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == command.Email, cancellationToken);
        if (user == null)
            return new AuthResult { Success = true };

        if (user.IsEmailVerified)
            return new AuthResult { Success = false, Message = "Already verified" };

        var token = await _otpService.GenerateTokenAsync(command.Email, "EmailVerification");
        var displayName = $"{user.FirstName} {user.LastName}".Trim();
        await _emailService.SendEmailVerificationAsync(command.Email, displayName, token);

        return new AuthResult { Success = true };
    }
}
