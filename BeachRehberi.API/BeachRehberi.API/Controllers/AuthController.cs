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

                private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
        private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
        private readonly IValidator<VerifyEmailRequest> _verifyEmailValidator;
        private readonly IValidator<ResendVerificationRequest> _resendVerificationValidator;

        public AuthController(
            IAuthService authService, 
            IConfiguration configuration, 
            IWebHostEnvironment env,
            IValidator<ForgotPasswordRequest> forgotPasswordValidator,
            IValidator<ResetPasswordRequest> resetPasswordValidator,
            IValidator<VerifyEmailRequest> verifyEmailValidator,
            IValidator<ResendVerificationRequest> resendVerificationValidator)
        {
            _authService = authService;
            _configuration = configuration;
            _env = env;
            _forgotPasswordValidator = forgotPasswordValidator;
            _resetPasswordValidator = resetPasswordValidator;
            _verifyEmailValidator = verifyEmailValidator;
            _resendVerificationValidator = resendVerificationValidator;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (result.Success)
                return Ok(result);
            return BadRequest(result);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                
                Response.Cookies.Append("refreshToken", response.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(),
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });
                
                return Ok(response);
            }
            catch(Exception ex)
            {
                return BadRequest(new AuthResult { Success = false, Message = ex.Message });
            }
        }

                [AllowAnonymous]
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var validation = await _forgotPasswordValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.ForgotPasswordAsync(request);
            return Ok(result); // Her zaman 200 d�n (g�venlik)
        }

                [AllowAnonymous]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var validation = await _resetPasswordValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.ResetPasswordAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

                [AllowAnonymous]
        [HttpPost("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var validation = await _verifyEmailValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.VerifyEmailAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

                [AllowAnonymous]
        [HttpPost("resend-verification")]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            var validation = await _resendVerificationValidator.ValidateAsync(request);
            if (!validation.IsValid)
                return BadRequest(new { errors = validation.Errors.Select(e => e.ErrorMessage) });

            var result = await _authService.ResendVerificationAsync(request);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ===================== LOGOUT =====================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            await _authService.LogoutAsync(null, refreshToken);
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
