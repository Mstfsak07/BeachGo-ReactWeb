using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using FluentValidation;
using Microsoft.Extensions.Configuration;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;

        public AuthController(IAuthService authService, IConfiguration configuration, IWebHostEnvironment env)
        {
            _authService = authService;
            _configuration = configuration;
            _env = env;
        }

        // ===================== LOGIN =====================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(
                request.Email,
                request.Password,
                GetIpAddress(),
                Request.Headers["User-Agent"].ToString()
            );

            if (result.Success && result.Data != null)
            {
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(),
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                result.Data.RefreshToken = null;
            }

            return result.ToActionResult();
        }

        // ===================== REFRESH =====================
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh()
        {
            // Authorization header'dan accessToken al
            var authHeader = Request.Headers.Authorization.ToString();
            var accessToken = authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? authHeader["Bearer ".Length..].Trim()
                : null;

            var refreshToken = Request.Cookies["refreshToken"];

            if (string.IsNullOrWhiteSpace(accessToken) || string.IsNullOrWhiteSpace(refreshToken))
                return Unauthorized("Access token ya da refresh token bulunamadı");

            var result = await _authService.RefreshTokenAsync(
                accessToken,
                refreshToken,
                GetIpAddress(),
                Request.Headers["User-Agent"].ToString()
            );

            if (result.Success && result.Data != null)
            {
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken!, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = !_env.IsDevelopment(),
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                result.Data.RefreshToken = null;
            }

            return result.ToActionResult();
        }

        // ===================== LOGOUT =====================
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            await _authService.LogoutAsync(null, refreshToken);

            Response.Cookies.Delete("refreshToken");

            return ((object?)null).ToOkApiResponse("Çıkış başarılı.");
        }

        // ===================== REVOKE =====================
        [HttpPost("revoke")]
        [Authorize]
        public async Task<IActionResult> Revoke([FromBody] RevokeRequest request)
        {
            var result = await _authService.RevokeTokenAsync(
                request.RefreshToken,
                GetIpAddress(),
                "manual_revoke"
            );

            return result.ToActionResult();
        }

        // ===================== REGISTER USER =====================
        [HttpPost("register-user")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterUser([FromBody] UserRegisterRequest request)
        {
            try
            {
                var registerRequest = new RegisterRequest
                {
                    Email = request.Email,
                    Password = request.Password,
                    BusinessName = request.Username,
                    ContactName = request.Username,
                    BeachId = null,
                    Role = UserRoles.User
                };

                var result = await _authService.RegisterAsync(
                    registerRequest,
                    GetIpAddress(),
                    Request.Headers["User-Agent"].ToString()
                );

                // 🔥 COOKIE EKLE (ÖNEMLİ)
                if (result.Success && result.Data != null)
                {
                    Response.Cookies.Append("refreshToken", result.Data.RefreshToken!, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = !_env.IsDevelopment(),
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });

                    result.Data.RefreshToken = null;
                }

                return result.ToActionResult();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return UnprocessableEntity(new ApiResponse<object>
                {
                    Success = false,
                    Message = _configuration["Auth:ValidationErrorMessage"] ?? "Doğrulama hataları var.",
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = _configuration["Auth:ServerErrorMessage"] ?? "Sunucu hatası oluştu."
                });
            }
        }

        // ===================== REGISTER BUSINESS =====================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            request.Role = UserRoles.Business;

            try
            {
                var result = await _authService.RegisterAsync(
                    request,
                    GetIpAddress(),
                    Request.Headers["User-Agent"].ToString()
                );

                // 🔥 COOKIE EKLE (ÖNEMLİ)
                if (result.Success && result.Data != null)
                {
                    Response.Cookies.Append("refreshToken", result.Data.RefreshToken!, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = !_env.IsDevelopment(),
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTime.UtcNow.AddDays(7)
                    });

                    result.Data.RefreshToken = null;
                }

                return result.ToActionResult();
            }
            catch (FluentValidation.ValidationException ex)
            {
                return UnprocessableEntity(new ApiResponse<object>
                {
                    Success = false,
                    Message = _configuration["Auth:ValidationErrorMessage"] ?? "Doğrulama hataları var.",
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = _configuration["Auth:ServerErrorMessage"] ?? "Sunucu hatası oluştu."
                });
            }
        }

        // ===================== IP =====================
        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].ToString();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}