using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Enums;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Auth.Commands.Register;

public class RegisterHandler : IRequestHandler<RegisterCommand, Result<RegisterResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<RegisterHandler> _logger;

    public RegisterHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<RegisterHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // E-posta kontrolü
        var existingUser = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant(), cancellationToken);

        if (existingUser != null)
            return Result<RegisterResponse>.Failure("Bu e-posta adresi zaten kullanılıyor.");

        // Şifre hash
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password, workFactor: 12);

        // Tenant oluştur (Business Owner ise)
        int? tenantId = null;
        if (!string.IsNullOrEmpty(request.BusinessName))
        {
            var slug = GenerateSlug(request.BusinessName);
            var tenant = new Tenant(request.BusinessName, slug, request.Email);
            await _unitOfWork.Tenants.AddAsync(tenant, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            tenantId = tenant.Id;
        }

        // Kullanıcı oluştur
        var role = !string.IsNullOrEmpty(request.BusinessName) ? UserRole.BusinessOwner : UserRole.User;
        var user = new User(request.Email, passwordHash, request.FirstName, request.LastName, role);

        if (!string.IsNullOrEmpty(request.Phone))
            user.UpdateProfile(request.FirstName, request.LastName, request.Phone);

        if (tenantId.HasValue)
            user.AssignToTenant(tenantId.Value);

        // Refresh token oluştur
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        user.SetRefreshToken(refreshToken, refreshTokenExpiry);

        await _unitOfWork.Users.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Access token oluştur
        var accessToken = _jwtService.GenerateAccessToken(user);
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(60);

        // Hoş geldin e-postası gönder (fire & forget)
        _ = _emailService.SendWelcomeEmailAsync(user.Email, user.FullName, cancellationToken);

        _logger.LogInformation("Yeni kullanıcı kaydoldu: {Email}, Role: {Role}", user.Email, role);

        return Result<RegisterResponse>.Created(new RegisterResponse(
            user.Id,
            user.Email,
            user.FullName,
            accessToken,
            refreshToken,
            accessTokenExpiry
        ), "Kayıt başarıyla tamamlandı.");
    }

    private static string GenerateSlug(string name)
    {
        return name.ToLowerInvariant()
                   .Replace(" ", "-")
                   .Replace("ş", "s").Replace("ğ", "g")
                   .Replace("ü", "u").Replace("ö", "o")
                   .Replace("ç", "c").Replace("ı", "i");
    }
}
