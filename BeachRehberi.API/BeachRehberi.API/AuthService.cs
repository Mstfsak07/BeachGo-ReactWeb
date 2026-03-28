using BeachRehberi.API.Models;
using BeachRehberi.API.Data;
using Microsoft.EntityFrameworkCore;
using BCrypt.Net;

namespace BeachRehberi.API.Services
{
    public class AuthService : IAuthService
    {
        private readonly BeachDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthService(BeachDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        public async Task<ServiceResult<AuthResponse>> RegisterAsync(RegisterRequest request, string ipAddress, string userAgent)
        {
            var existingUser = await _db.BusinessUsers.AnyAsync(u => u.Email == request.Email);
            if (existingUser)
                return ServiceResult<AuthResponse>.FailureResult("Bu e-posta adresi zaten kullanımda.");

            var user = new BusinessUser();
            user.SetEmail(request.Email);
            user.UpdateProfile(request.BusinessName, request.BusinessName);
            user.AssignToBeach(request.BeachId);
            
            // KRİTİK GÜVENLİK DÜZELTMESİ: İstemciden gelen rol bilgisi kesinlikle reddediliyor.
            // Yeni kayıt olan her kullanıcı varsayılan olarak sadece 'Business' rolünü alabilir.
            user.Role = UserRoles.Business; 

            // Şifre güvenli bir şekilde hash'leniyor.
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            _db.BusinessUsers.Add(user);
            await _db.SaveChangesAsync();

            return await LoginAsync(request.Email, request.Password, ipAddress, userAgent);
        }

        public async Task<ServiceResult<AuthResponse>> LoginAsync(string email, string password, string ipAddress, string userAgent)
        {
            // Login mantığı...
            return ServiceResult<AuthResponse>.FailureResult("Metot henüz tam olarak implemente edilmedi.");
        }

        public async Task LogoutAsync(string? accessToken, string? refreshToken)
        {
            // Logout mantığı...
        }

        public async Task<ServiceResult<AuthResponse>> RefreshTokenAsync(string accessToken, string refreshToken, string ipAddress)
        {
            // Refresh mantığı...
            return ServiceResult<AuthResponse>.FailureResult("Metot henüz tam olarak implemente edilmedi.");
        }

        public async Task<ServiceResult<bool>> RevokeTokenAsync(string refreshToken, string ipAddress, string reason)
        {
            // Revoke mantığı...
            return ServiceResult<bool>.SuccessResult(true);
        }
    }
}
