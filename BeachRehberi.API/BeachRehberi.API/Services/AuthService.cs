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

    public async Task<ServiceResult<AuthResponse>> LoginAsync(string email, string password, string ipAddress, string userAgent)
    {
        _logger.LogInformation("Login attempt for email: {Email} from IP: {IP}", email, ipAddress);
        
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Email == email);
        
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogWarning("Invalid login attempt for email: {Email} from IP: {IP}", email, ipAddress);
            return ServiceResult<AuthResponse>.FailureResult("E-posta adresi veya şifre hatalı.");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive account: {Email}", email);
            return ServiceResult<AuthResponse>.FailureResult("Hesabınız pasif durumdadır.");
        }

        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshTokenStr = _tokenService.GenerateRefreshToken();

            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshTokenStr,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                CreatedByUserAgent = userAgent
            };

            _db.RefreshTokens.Add(refreshToken);
            
            user.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Successful login for user: {Email} (Role: {Role})", email, user.Role);

            return ServiceResult<AuthResponse>.SuccessResult(new AuthResponse
            {
                Email = user.Email,
                Token = accessToken,
                RefreshToken = refreshTokenStr,
                Role = user.Role
            }, "Giriş başarılı.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during Login for {Email}", email);
            return ServiceResult<AuthResponse>.FailureResult("Giriş işlemi sırasında bir hata oluştu.");
        }
    }

    public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string refreshTokenStr, string ipAddress, string userAgent)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.Token == refreshTokenStr);

            if (refreshToken == null)
            {
                return ServiceResult<AuthResponse>.FailureResult("Geçersiz refresh token.");
            }

            if (refreshToken.IsRevoked)
            {
                _logger.LogCritical("SECURITY BREACH: Refresh token reuse detected! User: {UserId}, IP: {IP}", refreshToken.UserId, ipAddress);
                
                await _db.RefreshTokens
                    .Where(rt => rt.UserId == refreshToken.UserId)
                    .ExecuteUpdateAsync(s => s.SetProperty(rt => rt.IsRevoked, true)
                                             .SetProperty(rt => rt.ReasonRevoked, "Compromised: Refresh token reused"));
                                             
                await transaction.CommitAsync();
                return ServiceResult<AuthResponse>.FailureResult("Güvenlik ihlali nedeniyle tüm oturumlar sonlandırıldı.");
            }

            if (refreshToken.IsExpired)
            {
                return ServiceResult<AuthResponse>.FailureResult("Refresh token süresi dolmuş.");
            }

            if (refreshToken.CreatedByIp != ipAddress)
            {
                _logger.LogWarning("Suspicious IP change on refresh: Original {OldIP}, New {NewIP}", refreshToken.CreatedByIp, ipAddress);
            }

            var user = await _db.BusinessUsers.FindAsync(refreshToken.UserId);
            if (user == null || !user.IsActive)
            {
                return ServiceResult<AuthResponse>.FailureResult("Kullanıcı bulunamadı veya pasif.");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = newRefreshTokenStr,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress,
                CreatedByUserAgent = userAgent
            };

            refreshToken.IsRevoked = true;
            refreshToken.ReplacedByToken = newRefreshTokenStr;
            refreshToken.ReasonRevoked = "Replaced by rotation";

            _db.RefreshTokens.Add(newRefreshToken);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Token rotated for user: {Email}", user.Email);

            return ServiceResult<AuthResponse>.SuccessResult(new AuthResponse
            {
                Email = user.Email,
                Token = newAccessToken,
                RefreshToken = newRefreshTokenStr,
                Role = user.Role
            }, "Token yenilendi.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during RefreshToken");
            return ServiceResult<AuthResponse>.FailureResult("Token yenileme sırasında bir hata oluştu.");
        }
    }

    public async Task LogoutAsync(string? accessTokenStr, string? refreshTokenStr)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
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

                    await _tokenService.BlacklistTokenAsync(accessTokenStr, jwtToken.ValidTo);
                }
                catch { /* Ignore */ }
            }

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
            await transaction.CommitAsync();
            _logger.LogInformation("Successful logout for user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during Logout");
        }
    }

    public async Task<ServiceResult<BusinessUser>> RegisterAsync(RegisterRequest request)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        
        try
        {
            if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email))
            {
                return ServiceResult<BusinessUser>.FailureResult("Bu e-posta adresi zaten kullanımda.");
            }

            int? beachId = request.BeachId;
            if (beachId == 0) beachId = null;

            // Simple security: Registration defaults to User role, Admin/Business can be granted later or through specific logic
            string userRole = UserRoles.User;
            if (request.Role == UserRoles.Business && beachId.HasValue) 
            {
                userRole = UserRoles.Business;
            }

            if (beachId.HasValue)
            {
                var beachExists = await _db.Beaches.AnyAsync(b => b.Id == beachId.Value);
                if (!beachExists)
                {
                    return ServiceResult<BusinessUser>.FailureResult($"Seçilen plaj bulunamadı: {beachId.Value}.");
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
                Role = userRole // Use mapped role
            };

            _db.BusinessUsers.Add(user);
            await _db.SaveChangesAsync();
            
            await transaction.CommitAsync();
            
            _logger.LogInformation("New user registered: {Email} (Role: {Role})", user.Email, userRole);
            return ServiceResult<BusinessUser>.SuccessResult(user, "Kayıt başarıyla tamamlandı.");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error during registration for {Email}", request.Email);
            return ServiceResult<BusinessUser>.FailureResult("Kayıt sırasında bir hata oluştu.");
        }
    }
}
