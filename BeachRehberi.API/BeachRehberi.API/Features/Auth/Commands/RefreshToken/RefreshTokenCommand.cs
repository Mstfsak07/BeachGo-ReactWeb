using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(string RefreshToken, string IpAddress, string UserAgent) : IRequest<ServiceResult<AuthResponse>>;

public class RefreshTokenHandler : IRequestHandler<RefreshTokenCommand, ServiceResult<AuthResponse>>
{
    private readonly BeachDbContext _db;
    private readonly ITokenService _tokenService;

    public RefreshTokenHandler(BeachDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<ServiceResult<AuthResponse>> Handle(RefreshTokenCommand command, CancellationToken cancellationToken)
    {
        var hashedToken = BeachRehberi.API.Models.RefreshToken.HashToken(command.RefreshToken);
        var token = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hashedToken, cancellationToken);

        if (token == null || !token.IsActive)
            return ServiceResult<AuthResponse>.FailureResult("Geçersiz veya süresi dolmuş refresh token.");

        var user = await _db.BusinessUsers.FindAsync(new object[] { token.UserId }, cancellationToken);
        if (user == null || !user.IsActive)
            return ServiceResult<AuthResponse>.FailureResult("Kullanıcı bulunamadı veya pasif durumda.");

        // Token rotation
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

        token.RevokeAndReplace(newRefreshTokenStr, "rotation");

        var newRefreshToken = new BeachRehberi.API.Models.RefreshToken(
            user.Id, newRefreshTokenStr,
            DateTime.UtcNow.AddDays(7), command.IpAddress, command.UserAgent);

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync(cancellationToken);

        return ServiceResult<AuthResponse>.SuccessResult(new AuthResponse
        {
            Success = true,
            Message = "Token yenilendi.",
            Token = newAccessToken,
            RefreshToken = newRefreshTokenStr
        });
    }
}
