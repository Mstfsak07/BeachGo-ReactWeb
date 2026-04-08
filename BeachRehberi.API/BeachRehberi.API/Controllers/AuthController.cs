using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using MediatR;
using BeachRehberi.API.Features.Auth.Commands.Register;
using BeachRehberi.API.Features.Auth.Commands.Login;
using BeachRehberi.API.Features.Auth.Commands.ForgotPassword;
using BeachRehberi.API.Features.Auth.Commands.ResetPassword;
using BeachRehberi.API.Features.Auth.Commands.VerifyEmail;
using BeachRehberi.API.Features.Auth.Commands.ResendVerification;
using BeachRehberi.API.Features.Auth.Commands.RefreshToken;
using BeachRehberi.API.Features.Auth.Commands.Logout;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IWebHostEnvironment _env;

        public AuthController(IMediator mediator, IWebHostEnvironment env)
        {
            _mediator = mediator;
            _env = env;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _mediator.Send(new RegisterCommand(request));
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
                var result = await _mediator.Send(new LoginCommand(request));

                if (!result.Success)
                    return BadRequest(result);

                Response.Cookies.Append("refreshToken", result.RefreshToken ?? "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(),
                    SameSite = SameSiteMode.Lax,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new AuthResult { Success = false, Message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _mediator.Send(new ForgotPasswordCommand(request.Email));
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = "Şifre sıfırlama linki gönderildi." });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _mediator.Send(new ResetPasswordCommand(request.Email, request.Token, request.NewPassword));
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = "Şifre başarıyla sıfırlandı." });
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailGet([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Token gereklidir." });

            var result = await _mediator.Send(new VerifyEmailByTokenCommand(token));
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "E-posta başarıyla doğrulandı." });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var result = await _mediator.Send(new VerifyEmailCommand(request.Email, request.Token));
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = "E-posta başarıyla doğrulandı." });
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            var result = await _mediator.Send(new ResendVerificationCommand(request.Email));
            if (!result.Success)
                return BadRequest(new { message = result.Message });
            return Ok(new { message = "Doğrulama e-postası tekrar gönderildi." });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = Request.Headers["User-Agent"].ToString();
            var result = await _mediator.Send(new RefreshTokenCommand(request.RefreshToken, ipAddress, userAgent));

            if (!result.Success)
                return BadRequest(new AuthResult { Success = false, Message = result.Message });

            var response = result.Data!;

            Response.Cookies.Append("refreshToken", response.RefreshToken ?? "", new CookieOptions
            {
                HttpOnly = true,
                Secure = !_env.IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(new AuthResult
            {
                Success = true,
                Message = "Token yenilendi.",
                Token = response.Token,
                RefreshToken = response.RefreshToken
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            await _mediator.Send(new LogoutCommand(refreshToken));

            Response.Cookies.Delete("refreshToken");
            return Ok(new AuthResult { Success = true, Message = "Çıkış başarılı." });
        }

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest request)
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var result = await _mediator.Send(new RevokeTokenCommand(request.RefreshToken, ipAddress));

            if (result.Success)
                return Ok(new AuthResult { Success = true, Message = result.Message });

            return BadRequest(new AuthResult { Success = false, Message = result.Message });
        }
    }
}
