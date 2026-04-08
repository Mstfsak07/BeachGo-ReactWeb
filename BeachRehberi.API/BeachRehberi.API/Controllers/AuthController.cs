using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ITokenService _tokenService;
        private readonly BeachRehberi.API.Data.BeachDbContext _db;
        private readonly MediatR.IMediator _mediator;

        public AuthController(IAuthService authService, IConfiguration configuration, IWebHostEnvironment env, ITokenService tokenService, BeachRehberi.API.Data.BeachDbContext db, MediatR.IMediator mediator)
        {
            _authService = authService;
            _configuration = configuration;
            _env = env;
            _tokenService = tokenService;
            _db = db;
            _mediator = mediator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var command = new BeachRehberi.Application.Features.Auth.Commands.Register.RegisterCommand(
                request.FirstName, request.LastName, request.Email, request.Password, request.Password, request.PhoneNumber);
            
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
            {
                var response = result.Value;
                return Ok(new AuthResult
                {
                    Success = true,
                    Message = "Kayıt başarılı. Lütfen email adresinize gönderilen doğrulamayı kontrol edin.",
                    Token = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                    User = new UserDto
                    {
                        Id = response.UserId,
                        Email = response.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        PhoneNumber = request.PhoneNumber,
                        IsEmailVerified = false
                    }
                });
            }

            return BadRequest(new AuthResult { Success = false, Message = result.Error });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var command = new BeachRehberi.Application.Features.Auth.Commands.Login.LoginCommand(request.Email, request.Password);
                var result = await _mediator.Send(command);

                if (!result.IsSuccess)
                    return BadRequest(new AuthResult { Success = false, Message = result.Error });

                var response = result.Value;

                Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(),
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(new AuthResult
                {
                    Success = true,
                    Message = "Giriş başarılı.",
                    Token = response.AccessToken,
                    RefreshToken = response.RefreshToken,
                    ExpiresAt = response.AccessTokenExpiry,
                    User = new UserDto
                    {
                        Id = response.UserId,
                        Email = response.Email,
                        FirstName = response.FullName.Split(' ')[0],
                        LastName = response.FullName.Contains(' ') ? response.FullName.Split(' ', 2)[1] : "",
                        PhoneNumber = "",
                        IsEmailVerified = true
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResult { Success = false, Message = ex.Message });
            }
        }

                [HttpPost("forgot-password")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _authService.ForgotPasswordAsync(request.Email);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            
            return Ok(new { message = "If the email exists, a reset link has been sent." });
        }

                [HttpPost("reset-password")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _authService.ResetPasswordAsync(request.Email, request.Token, request.NewPassword);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            
            return Ok(new { message = "Password has been reset successfully." });
        }

                // GET: /api/auth/verify-email?token=...  (frontend link-click flow)
        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailGet([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Token gereklidir." });

            // Token hash'i ile DB'den email'i bul
            var tokenHash = ComputeSha256(token);
            var code = await _db.VerificationCodes
                .Where(c => c.CodeHash == tokenHash
                         && c.Purpose == BeachRehberi.API.Models.OtpPurpose.EmailVerification
                         && !c.IsUsed)
                .FirstOrDefaultAsync();

            if (code == null || code.ExpiresAt <= DateTime.UtcNow)
                return BadRequest(new { message = "Doğrulama bağlantısı geçersiz veya süresi dolmuş." });

            var result = await _authService.VerifyEmailAsync(code.Email, token);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "E-posta başarıyla doğrulandı." });
        }

        // POST: /api/auth/verify-email  (body flow — backward compat)
        [HttpPost("verify-email")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyEmailAsync(request.Email, request.Token);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "Email verified successfully." });
        }

        private static string ComputeSha256(string raw)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(raw));
            return string.Concat(bytes.Select(b => b.ToString("x2")));
        }

                [HttpPost("resend-verification")]
        [AllowAnonymous]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            
            var result = await _authService.ResendVerificationAsync(request.Email);
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            
            return Ok(new { message = "Verification email resent." });
        }

        // ===================== LOGOUT =====================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdStr, out var userId))
            {
                var command = new BeachRehberi.Application.Features.Auth.Commands.Logout.LogoutCommand(userId);
                await _mediator.Send(command);
            }

            Response.Cookies.Delete("refreshToken");
            return Ok(new AuthResult { Success = true, Message = "Çıkış başarılı." });
        }

        // ===================== REVOKE =====================
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest request)
        {
            var result = await _authService.RevokeTokenAsync(request.RefreshToken, GetIpAddress(), "manual_revoke");
            if (result.Success) return Ok(new AuthResult { Success = true, Message = result.Message });
            return BadRequest(new AuthResult { Success = false, Message = result.Message });
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].ToString();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}

