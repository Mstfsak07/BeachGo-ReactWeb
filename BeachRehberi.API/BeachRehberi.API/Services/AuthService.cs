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

            var refreshToken = new RefreshToken(
                user.Id,
                refreshTokenStr,
                DateTime.UtcNow.AddDays(7),
                ipAddress,
                userAgent
            );

            _db.RefreshTokens.Add(refreshToken);

            user.RecordLogin();
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

            var user = await _db.BusinessUsers.FindAsync(refreshToken.UserId);
            if (user == null || !user.IsActive)
            {
                return ServiceResult<AuthResponse>.FailureResult("Kullanıcı bulunamadı veya pasif.");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshTokenStr = _tokenService.GenerateRefreshToken();

            var newRefreshToken = new RefreshToken(
                user.Id,
                newRefreshTokenStr,
                DateTime.UtcNow.AddDays(7),
                ipAddress,
                userAgent
            );

            refreshToken.Revoke("Replaced by rotation", newRefreshTokenStr);

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
                    rt.Revoke("User logout");
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
        if (request == null)
            return ServiceResult<BusinessUser>.FailureResult("Geçersiz kayıt verisi.");

        using var transaction = await _db.Database.BeginTransactionAsync(IsolationLevel.Serializable);

        try
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return ServiceResult<BusinessUser>.FailureResult("Email ve şifre zorunludur.");

            if (await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email))
            {
                return ServiceResult<BusinessUser>.FailureResult("Bu e-posta adresi zaten kullanımda.");
            }

            int? beachId = request.BeachId;
            if (beachId == 0) beachId = null;

            var normalizedRole = string.IsNullOrWhiteSpace(request.Role) ? UserRoles.User : request.Role.Trim();
            string userRole = UserRoles.User;

            if (normalizedRole == UserRoles.Business)
            {
                if (!beachId.HasValue)
                    return ServiceResult<BusinessUser>.FailureResult("Business kullanıcı kaydı için BeachId gereklidir.");

                userRole = UserRoles.Business;
            }
            else if (normalizedRole == UserRoles.Admin)
            {
                // Sunucudan dışarıya admin kaydı verilmesin, test/belirli şartlara göre kontrol edilebilir
                return ServiceResult<BusinessUser>.FailureResult("Admin rolü ile kayıt yapılamaz.");
            }

            if (!string.IsNullOrWhiteSpace(request.BusinessName) && request.BusinessName.Length > 120)
                return ServiceResult<BusinessUser>.FailureResult("BusinessName çok uzun.");

            if (!string.IsNullOrWhiteSpace(request.ContactName) && request.ContactName.Length > 100)
                return ServiceResult<BusinessUser>.FailureResult("ContactName çok uzun.");

            if (beachId.HasValue)
            {
                var beachExists = await _db.Beaches.AnyAsync(b => b.Id == beachId.Value);
                if (!beachExists)
                {
                    return ServiceResult<BusinessUser>.FailureResult($"Seçilen plaj bulunamadı: {beachId.Value}.");
                }
            }

            var user = new BusinessUser(
                request.Email.Trim(),
                BCrypt.Net.BCrypt.HashPassword(request.Password),
                userRole
            );

            user.AssignToBeach(beachId);
            user.UpdateProfile(request.ContactName?.Trim(), string.IsNullOrWhiteSpace(request.BusinessName) ? null : request.BusinessName.Trim());

            _db.BusinessUsers.Add(user);
            await _db.SaveChangesAsync();

            await transaction.CommitAsync();

            _logger.LogInformation("New user registered: {Email} (Role: {Role}, BeachId: {BeachId})", user.Email, userRole, user.BeachId);
            return ServiceResult<BusinessUser>.SuccessResult(user, "Kayıt başarıyla tamamlandı.");
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync();
            _logger.LogWarning(ex, "Domain validation failed during registration for {Email}", request.Email);
            return ServiceResult<BusinessUser>.FailureResult(ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Unhandled error during registration for {Email} (role={Role}, beachId={BeachId})", request.Email, request.Role, request.BeachId);
            return ServiceResult<BusinessUser>.FailureResult($"Kayıt sırasında bir hata oluştu: {ex.Message}");
        }
    }
}
