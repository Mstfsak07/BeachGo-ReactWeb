using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Data;

namespace BeachRehberi.API.Services;

public class AuthService : IAuthService
{
    private readonly BeachDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthService> _logger;

    public AuthService(BeachDbContext db, ITokenService tokenService, ILogger<AuthService> logger)
    {
        _db = db;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(string email, string password)
    {
        _logger.LogInformation("Login attempt for email: {Email}", email);
        
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email}", email);
            return ApiResponse<AuthResponse>.FailureResult("E-posta adresi veya şifre hatalı.");
        }

        if (!user.IsActive)
        {
            return ApiResponse<AuthResponse>.FailureResult("Hesabınız pasif durumdadır.");
        }

        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenStr = _tokenService.GenerateRefreshToken();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenStr,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        // Requirement 2: DB-backed refresh tokens
        _db.RefreshTokens.Add(refreshToken);
        
        user.LastLoginAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return ApiResponse<AuthResponse>.SuccessResult(new AuthResponse
        {
            Email = user.Email,
            Token = accessToken,
            RefreshToken = refreshTokenStr,
            Role = user.Role
        }, "Giriş başarılı.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(string refreshTokenStr)
    {
        var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshTokenStr);

        if (refreshToken == null)
        {
            return ApiResponse<AuthResponse>.FailureResult("Geçersiz refresh token.");
        }

        // Requirement 4: Token Reuse Detection
        if (refreshToken.IsRevoked)
        {
            _logger.LogCritical("SECURITY BREACH: Refresh token reuse detected! Token: {Token}, User: {UserId}", refreshTokenStr, refreshToken.UserId);
            
            // Revoke all tokens for this user for safety - Compromised session detection
            await _db.RefreshTokens
                .Where(rt => rt.UserId == refreshToken.UserId)
                .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true)
                                         .SetProperty(rt => rt.ReasonRevoked, "Compromised: Refresh token reused"));
                                         
            return ApiResponse<AuthResponse>.FailureResult("Güvenlik ihlali nedeniyle tüm oturumlar sonlandırıldı.");
        }

        if (refreshToken.IsExpired)
        {
            return ApiResponse<AuthResponse>.FailureResult("Refresh token süresi dolmuş.");
        }

        var user = await _db.BusinessUsers.FindAsync(refreshToken.UserId);
        if (user == null || !user.IsActive)
        {
            return ApiResponse<AuthResponse>.FailureResult("Kullanıcı bulunamadı veya pasif.");
        }

        // Requirement 2: Refresh Token Rotation
        var newAccessToken = _tokenService.GenerateAccessToken(user);
        var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

        var newRefreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = newRefreshTokenStr,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        // Invalidate old token properly
        refreshToken.IsRevoked = true;
        refreshToken.ReplacedByToken = newRefreshTokenStr;
        refreshToken.ReasonRevoked = "Replaced by rotation";

        _db.RefreshTokens.Add(newRefreshToken);
        await _db.SaveChangesAsync();

        return ApiResponse<AuthResponse>.SuccessResult(new AuthResponse
        {
            Email = user.Email,
            Token = newAccessToken,
            RefreshToken = newRefreshTokenStr,
            Role = user.Role
        }, "Token yenilendi.");
    }

    public async Task LogoutAsync(string? accessTokenStr, string? refreshTokenStr)
    {
        int? userId = null;

        if (!string.IsNullOrEmpty(accessTokenStr))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(accessTokenStr);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "sub" || c.Type == JwtRegisteredClaimNames.Sub)?.Value;
                if (int.TryParse(userIdClaim, out int id)) userId = id;

                // Requirement 3: Blacklist access token
                await _tokenService.BlacklistTokenAsync(accessTokenStr, jwtToken.ValidTo);
            }
            catch { /* Ignore invalid token */ }
        }

        // Requirement 2: Revoke refresh token
        if (!string.IsNullOrEmpty(refreshTokenStr))
        {
            var rt = await _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == refreshTokenStr);
            if (rt != null)
            {
                rt.IsRevoked = true;
                rt.ReasonRevoked = "User logout";
                userId ??= rt.UserId;
            }
        }

        await _db.SaveChangesAsync();
    }

    public async Task<ApiResponse<BusinessUser>> RegisterAsync(RegisterRequest request)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        
        try
        {
            if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email))
            {
                return ApiResponse<BusinessUser>.FailureResult("Bu e-posta adresi zaten kullanımda.");
            }

            int? beachId = request.BeachId;
            if (beachId == 0) beachId = null;

            if (beachId.HasValue)
            {
                var beachExists = await _db.Beaches.AnyAsync(b => b.Id == beachId.Value);
                if (!beachExists)
                {
                    return ApiResponse<BusinessUser>.FailureResult($"Seçilen plaj bulunamadı: {beachId.Value}.");
                }
            }

            var user = new BusinessUser
            {
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                BusinessName = request.BusinessName,
                BeachId = beachId,
                ContactName = request.ContactName,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                Role = "BusinessOwner"
            };

            _db.BusinessUsers.Add(user);
            await _db.SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            _logger.LogInformation("New business user registered: {Email}", user.Email);
            return ApiResponse<BusinessUser>.SuccessResult(user, "Kayıt başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return ApiResponse<BusinessUser>.FailureResult("Kayıt sırasında bir hata oluştu.");
        }
    }
}
