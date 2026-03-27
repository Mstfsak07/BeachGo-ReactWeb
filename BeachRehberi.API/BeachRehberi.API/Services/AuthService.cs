using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService {
    private readonly BeachDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(BeachDbContext db, ITokenService tokenService) { 
        _db = db; 
        _tokenService = tokenService;
    }

    public async Task<AuthResponse?> LoginAsync(string email, string password) {
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _db.RefreshTokens.Add(refreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse {
            Email = user.Email,
            Token = accessToken,
            RefreshToken = refreshTokenStr,
            Role = user.Role
        };
    }

    public async Task<AuthResponse?> RefreshTokenAsync(string refreshTokenStr) {
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshTokenStr);
        if (refreshToken == null || refreshToken.ExpiryDate < DateTime.UtcNow || refreshToken.IsRevoked) return null;

        var user = await _db.BusinessUsers.FindAsync(refreshToken.UserId);
        if (user == null) return null;

        // Revoke old token
        refreshToken.IsRevoked = true;

        // Generate new pair
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken {
            UserId = user.Id,
            Token = newRefreshTokenStr,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        return new AuthResponse {
            Email = user.Email,
            Token = newAccessToken,
            RefreshToken = newRefreshTokenStr,
            Role = user.Role
        };
    }

    public async Task LogoutAsync(string? accessTokenStr, string? refreshTokenStr) {
        if (!string.IsNullOrEmpty(refreshTokenStr)) {
            var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshTokenStr);
            if (refreshToken != null) {
                refreshToken.IsRevoked = true;
                // Revoke all other refresh tokens for this user for safety
                var otherTokens = await _db.RefreshTokens.Where(rt => rt.UserId == refreshToken.UserId && !rt.IsRevoked).ToListAsync();
                foreach(var t in otherTokens) t.IsRevoked = true;
            }
        }

        if (!string.IsNullOrEmpty(accessTokenStr)) {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessTokenStr);
            await _tokenService.BlacklistTokenAsync(accessTokenStr, jwtToken.ValidTo);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<BusinessUser?> RegisterAsync(RegisterRequest request) {
        if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email)) return null;

        var user = new BusinessUser {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            BusinessName = request.BusinessName,
            BeachId = request.BeachId,
            CreatedAt = DateTime.UtcNow
        };
        _db.BusinessUsers.Add(user);
        await _db.SaveChangesAsync();
        return user;
    }
}
