using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;
using MediatR;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/auth")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IWebHostEnvironment _env;
        private readonly IMediator _mediator;

        public AuthController(IAuthService authService, IWebHostEnvironment env, IMediator mediator)
        {
            _authService = authService;
            _env = env;
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
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var command = new BeachRehberi.Application.Features.Auth.Commands.ForgotPassword.ForgotPasswordCommand(request.Email);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = result.Message });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var command = new BeachRehberi.Application.Features.Auth.Commands.ResetPassword.ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = result.Message });
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailGet([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return BadRequest(new { message = "Token gereklidir." });

            var result = await _authService.VerifyEmailByTokenAsync(token);
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            return Ok(new { message = "E-posta başarıyla doğrulandı." });
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
        {
            var command = new BeachRehberi.Application.Features.Auth.Commands.VerifyEmail.VerifyEmailCommand(request.Email, request.Token);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = result.Message });
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequest request)
        {
            var command = new BeachRehberi.Application.Features.Auth.Commands.ResendVerification.ResendVerificationCommand(request.Email);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
                return BadRequest(new { message = result.Error });

            return Ok(new { message = result.Message });
        }

        [AllowAnonymous]
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var command = new BeachRehberi.Application.Features.Auth.Commands.RefreshToken.RefreshTokenCommand(request.AccessToken, request.RefreshToken);
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
                Message = "Token yenilendi.",
                Token = response.AccessToken,
                RefreshToken = response.RefreshToken
            });
        }

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

        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest request)
        {
            var command = new BeachRehberi.Application.Features.Auth.Commands.RevokeToken.RevokeTokenCommand(request.RefreshToken);
            var result = await _mediator.Send(command);

            if (result.IsSuccess)
                return Ok(new AuthResult { Success = true, Message = result.Message });

            return BadRequest(new AuthResult { Success = false, Message = result.Error });
        }
    }
}
