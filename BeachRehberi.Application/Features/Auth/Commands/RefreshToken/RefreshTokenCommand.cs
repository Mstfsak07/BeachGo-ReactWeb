using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Auth.Commands.RefreshToken;

// ── Command ──────────────────────────────────────────────────────────────────
public record RefreshTokenCommand(
    string AccessToken,
    string RefreshToken
) : IRequest<RefreshTokenResult>;

// ── Result ───────────────────────────────────────────────────────────────────
public record RefreshTokenResult(
    string AccessToken,
    string RefreshToken
);

// ── Handler ──────────────────────────────────────────────────────────────────
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, RefreshTokenResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork;
        _jwtService = jwtService;
    }

    public async Task<RefreshTokenResult> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Süresi dolmuş token'dan userId al
        var userId = _jwtService.GetUserIdFromToken(request.AccessToken);

        if (userId is null)
            throw new UnauthorizedException("Geçersiz access token.");

        var user = await _unitOfWork.Users.GetByIdAsync(userId.Value, cancellationToken);

        if (user is null || user.IsDeleted)
            throw new UnauthorizedException("Kullanıcı bulunamadı.");

        if (!user.IsRefreshTokenValid(request.RefreshToken))
            throw new UnauthorizedException("Geçersiz veya süresi dolmuş refresh token.");

        // Yeni token çifti üret
        var newAccessToken = _jwtService.GenerateAccessToken(user);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.SetRefreshToken(newRefreshToken, DateTime.UtcNow.AddDays(7));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new RefreshTokenResult(newAccessToken, newRefreshToken);
    }
}
