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
        var normalizedName = NormalizeName(request);
        var isBusinessRegistration = !string.IsNullOrWhiteSpace(request.BusinessName) && !string.IsNullOrWhiteSpace(request.ContactName);

        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email, cancellationToken))
            return new AuthResult { Success = false, Message = "Bu email adresi zaten kayıtlı." };

        var user = new BusinessUser(
            request.Email,
            BCrypt.Net.BCrypt.HashPassword(request.Password),
            isBusinessRegistration ? UserRoles.Business : UserRoles.User);

        user.UpdatePersonalInfo(normalizedName.firstName, normalizedName.lastName, request.PhoneNumber?.Trim() ?? string.Empty);
        if (isBusinessRegistration)
        {
            user.UpdateProfile(request.ContactName?.Trim(), request.BusinessName?.Trim());
            user.AssignToBeach(request.BeachId);
        }

        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        var token = await _otpService.GenerateTokenAsync(user.Email, "EmailVerification");
        var displayName = $"{normalizedName.firstName} {normalizedName.lastName}".Trim();
        await _emailService.SendEmailVerificationAsync(user.Email, displayName, token);

        return new AuthResult { Success = true, Message = "Kayıt başarılı. Lütfen email adresinize gönderilen doğrulama linkine tıklayın." };
    }

    private static (string firstName, string lastName) NormalizeName(RegisterRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.ContactName))
        {
            return SplitName(request.ContactName);
        }

        var firstName = request.FirstName?.Trim() ?? string.Empty;
        var lastName = request.LastName?.Trim() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName))
        {
            return (firstName, lastName);
        }

        var fullName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (string.Empty, string.Empty);
        }

        return SplitName(fullName);
    }

    private static (string firstName, string lastName) SplitName(string fullName)
    {
        var parts = fullName.Split(' ', 2, System.StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return (parts[0], string.Empty);
        }

        return (parts[0], parts[1]);
    }
}
