using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Enums;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.Register;

// ── Command ──────────────────────────────────────────────────────────────────
public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string PasswordConfirm,
    string? Phone = null
) : IRequest<Result<RegisterResponse>>;

// ── Response ──────────────────────────────────────────────────────────────────
public record RegisterResponse(
    int UserId,
    string Email,
    string FullName,
    string AccessToken,
    string RefreshToken
);

// ── Handler ──────────────────────────────────────────────────────────────────
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailService _emailService;
    private readonly IOtpService _otpService;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IPasswordHasher passwordHasher,
        IEmailService emailService,
        IOtpService otpService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
        _emailService = emailService;
        _otpService = otpService;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // E-posta benzersizlik kontrolü
        var emailExists = await _unitOfWork.Users.AnyAsync(
            u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (emailExists)
            return Result<RegisterResponse>.Failure("Bu e-posta adresi zaten kullanılıyor.");

        // Şifre hash'leme
        var passwordHash = _passwordHasher.Hash(request.Password);

        // Kullanıcı oluştur
        var user = new User(
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            UserRole.User);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Verification Token üret
        var verificationToken = await _otpService.GenerateTokenAsync(user.Email, "EmailVerification", cancellationToken);

        // Token üret (JWT)
        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(refreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // E-posta gönderimi (hata olursa kaydı engelleme)
        try
        {
            await _emailService.SendEmailVerificationAsync(
                user.Email, user.FullName, verificationToken, cancellationToken);
        }
        catch
        {
            // E-posta hatası kaydı engellemez
        }

        return Result<RegisterResponse>.Created(new RegisterResponse(
            user.Id,
            user.Email,
            user.FullName,
            accessToken,
            refreshToken));
    }
}
