using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BeachRehberi.API.Features.Auth.Commands.Login;

public record LoginCommand(LoginRequest Request) : IRequest<AuthResult>;

public class LoginHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly BeachDbContext _db;
    private readonly ITokenService _tokenService;

    public LoginHandler(BeachDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<AuthResult> Handle(LoginCommand command, CancellationToken cancellationToken)
    {
        var request = command.Request;
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _db.BusinessUsers
            .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && u.IsActive, cancellationToken);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new AuthResult { Success = false, Message = "Geçersiz kullanıcı bilgileri." };

        if (string.Equals(user.Role, UserRoles.Business, StringComparison.OrdinalIgnoreCase) && !user.IsEmailVerified)
            return new AuthResult { Success = false, Message = "İşletme hesabı için e-posta doğrulaması zorunludur." };

        await InvalidateAllSessionsAsync(user.Id, "new_login", cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        _db.RefreshTokens.Add(new BeachRehberi.API.Models.RefreshToken(
            user.Id, refreshTokenStr,
            DateTime.UtcNow.AddDays(7), "unknown", "unknown"));

        await _db.SaveChangesAsync(cancellationToken);

        return new AuthResult
        {
            Success = true,
            Message = "Giriş başarılı.",
            Token = accessToken,
            RefreshToken = refreshTokenStr,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                FirstName = user.FirstName ?? "",
                LastName = user.LastName ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                Role = user.Role,
                AccountType = user.Role,
                BeachId = user.BeachId,
                IsEmailVerified = user.IsEmailVerified
            }
        };
    }

    private async Task InvalidateAllSessionsAsync(int userId, string reason, CancellationToken cancellationToken)
    {
        var refreshTokens = await _db.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        if (refreshTokens.Count == 0)
            return;

        foreach (var refreshToken in refreshTokens)
        {
            refreshToken.Revoke(reason);
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}
