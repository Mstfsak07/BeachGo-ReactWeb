using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly ILogger<RefreshTokenHandler> _logger;

    public RefreshTokenHandler(IUnitOfWork unitOfWork, IJwtService jwtService, ILogger<RefreshTokenHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
        _logger = logger;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userId = _jwtService.GetUserIdFromToken(request.AccessToken);
        if (userId == null)
            return Result<RefreshTokenResponse>.Unauthorized("Geçersiz access token.");

        var user = await _unitOfWork.Users.FirstOrDefaultAsync(
            u => u.Id == userId && !u.IsDeleted, cancellationToken);

        if (user == null)
            return Result<RefreshTokenResponse>.NotFound("Kullanıcı bulunamadı.");

        if (!user.IsRefreshTokenValid(request.RefreshToken))
        {
            _logger.LogWarning("Geçersiz refresh token kullanımı: UserId={UserId}", userId);
            return Result<RefreshTokenResponse>.Unauthorized("Refresh token geçersiz veya süresi dolmuş. Lütfen tekrar giriş yapın.");
        }

        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);
        var accessTokenExpiry = DateTime.UtcNow.AddMinutes(60);

        user.SetRefreshToken(newRefreshToken, refreshTokenExpiry);
        _unitOfWork.Users.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(
            newAccessToken,
            newRefreshToken,
            accessTokenExpiry
        ));
    }
}
