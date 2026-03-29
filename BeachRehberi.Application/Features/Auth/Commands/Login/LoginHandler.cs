using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Auth.Commands.Login;

public class LoginHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<LoginHandler> _logger;

    public LoginHandler(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<LoginHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Email == request.Email.ToLowerInvariant() && !u.IsDeleted, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Başarısız giriş denemesi: {Email}", request.Email);
            return Result<LoginResponse>.Failure("E-posta veya şifre hatalı.", 401);
        }

        if (!user.IsActive)
            return Result<LoginResponse>.Failure("Hesabınız devre dışı bırakılmıştır. Lütfen destek ekibiyle iletişime geçin.", 403);

        var accessToken = _jwtService.GenerateAccessToken(user);
        var refreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(60);

        user.SetRefreshToken(refreshToken, refreshTokenExpiry);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Kullanıcı giriş yaptı: {Email}", user.Email);

        return Result<LoginResponse>.Success(new LoginResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            accessToken,
            refreshToken,
            accessTokenExpiry,
            user.TenantId
        ));
    }
}
