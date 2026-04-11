using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Exceptions;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using MediatR;
using BeachRehberi.API.Services;
using BeachRehberi.API.Features.Auth.Commands.Register;
using BeachRehberi.API.Features.Auth.Commands.Login;
using BeachRehberi.API.Features.Auth.Commands.ForgotPassword;
using BeachRehberi.API.Features.Auth.Commands.ResetPassword;
using BeachRehberi.API.Features.Auth.Commands.VerifyEmail;
using BeachRehberi.API.Features.Auth.Commands.ResendVerification;
using BeachRehberi.API.Features.Auth.Commands.RefreshToken;
using BeachRehberi.API.Features.Auth.Commands.Logout;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, ITokenService tokenService, IWebHostEnvironment env)
        {
            _authService = authService;
            _tokenService = tokenService;
            _env = env;
        }

        private CookieOptions BuildRefreshTokenCookieOptions()
        {
            var isCrossSiteDeployment = !_env.IsDevelopment();

            return new CookieOptions
            {
                HttpOnly = true,
                Secure = isCrossSiteDeployment,
                SameSite = isCrossSiteDeployment ? SameSiteMode.None : SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7),
                Path = "/"
            };
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
                throw new DomainException(result.Message);

            return Ok(ApiResponse<AuthResult>.Ok(result, "Kayıt başarıyla tamamlandı."));
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request);

            if (!result.Success)
                throw new DomainException(result.Message);

            Response.Cookies.Append("refreshToken", result.RefreshToken ?? "", BuildRefreshTokenCookieOptions());

            return Ok(ApiResponse<AuthResult>.Ok(result, "Giriş başarılı."));
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            if (!result.Success)
                throw new DomainException(result.Message);

            return Ok(ApiResponse.OkResult("Şifre sıfırlama linki gönderildi."));
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!result.Success)
                throw new DomainException(result.Message);

            return Ok(ApiResponse.OkResult("Şifre başarıyla sıfırlandı."));
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailGet([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                throw new ValidationException("Token gereklidir.");

            var result = await _authService.VerifyEmailByTokenAsync(token);
            if (!result.Success)
                throw new DomainException(result.Message);

            return Ok(ApiResponse.OkResult("E-posta başarıyla doğrulandı."));
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var result = await _authService.VerifyEmailAsync(request.Email, request.Token);
            if (!result.Success)
                throw new DomainException(result.Message);

            return Ok(ApiResponse.OkResult("E-posta başarıyla doğrulandı."));
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            var result = await _authService.ResendVerificationAsync(request.Email);
            if (!result.Success)
                throw new DomainException(result.Message);

            return Ok(ApiResponse.OkResult("Doğrulama e-postası tekrar gönderildi."));
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var refreshToken = request.RefreshToken ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
                throw new DomainException("Refresh token gerekli.");

            var result = await _tokenService.RefreshTokenAsync(refreshToken);

            if (!result.Success)
                throw new DomainException(result.Message);

            Response.Cookies.Append("refreshToken", result.RefreshToken ?? "", BuildRefreshTokenCookieOptions());

            return Ok(ApiResponse<AuthResult>.Ok(result, "Token başarıyla yenilendi."));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var jti = User.FindFirstValue(JwtRegisteredClaimNames.Jti);

            if (string.IsNullOrEmpty(jti))
            {
                var accessToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
                await _tokenService.RevokeAccessTokenAsync(accessToken);
            }
            
            if (!string.IsNullOrEmpty(jti))
            {
                await _tokenService.RevokeAccessToken(jti);
            }

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _tokenService.RevokeRefreshTokenAsync(refreshToken);
            }

            Response.Cookies.Delete("refreshToken", BuildRefreshTokenCookieOptions());
            return Ok(ApiResponse.OkResult("Çıkış başarılı."));
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress, request.Reason ?? "manual_revoke");

            if (!result.Success)
                throw new DomainException(result.Message);

            return Ok(ApiResponse.OkResult("Token başarıyla iptal edildi."));
        }
    }
}
