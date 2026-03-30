using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.RefreshToken;

// ── Command ──────────────────────────────────────────────────────────────────
public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<Result<RefreshTokenResponse>>;

// ── Response ──────────────────────────────────────────────────────────────────
public record RefreshTokenResponse(
    string AccessToken,
    string RefreshToken
);

// ── Handler ──────────────────────────────────────────────────────────────────
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Süresi dolmuş token'dan userId al
        var userId = _jwtService.GetUserIdFromToken(request.AccessToken);

        if (userId is null)
            return Result<RefreshTokenResponse>.Unauthorized("Geçersiz access token.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);

        if (user is null || user.IsDeleted)
            return Result<RefreshTokenResponse>.NotFound("Kullanıcı bulunamadı.");

        if (!user.IsRefreshTokenValid(request.RefreshToken))
            return Result<RefreshTokenResponse>.Unauthorized("Refresh token geçersiz veya süresi dolmuş. Lütfen tekrar giriş yapın.");

        // Yeni token çifti üret
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<RefreshTokenResponse>.Success(new RefreshTokenResponse(newAccessToken, newRefreshToken));
    }
}
