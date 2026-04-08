using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.Register;

public record RegisterCommand(RegisterRequest Request) : IRequest<AuthResult>;

public class RegisterHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly BeachDbContext _db;
    private readonly IOtpService _otpService;
    private readonly IEmailService _emailService;

    public RegisterHandler(BeachDbContext db, IOtpService otpService, IEmailService emailService)
    {
        _db = db;
        _otpService = otpService;
        _emailService = emailService;
    }

    public async Task<AuthResult> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;

        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return new AuthResult { Success = false, Message = "Bu email adresi zaten kayıtlı." };

        var user = new BusinessUser(
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            UserRoles.User);

        user.UpdatePersonalInfo(request.FirstName, request.LastName, request.PhoneNumber);

        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var token = await _otpService.GenerateTokenAsync(user.Email, "EmailVerification");
        var displayName = $"{request.FirstName} {request.LastName}".Trim();
        await _emailService.SendEmailVerificationAsync(user.Email, displayName, token);

        return new AuthResult { Success = true, Message = "Kayıt başarılı. Lütfen email adresinize gönderilen doğrulama linkine tıklayın." };
    }
}
