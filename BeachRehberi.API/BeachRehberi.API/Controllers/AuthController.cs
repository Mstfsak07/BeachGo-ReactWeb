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

        public AuthController(IAuthService authService, IConfiguration configuration)
        {
            _authService = authService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var ipAddress = GetIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            var result = await _authService.LoginAsync(request.Email, request.Password, ipAddress, userAgent);

            if (result.Success)
            {
                // Set refresh token as HttpOnly cookie
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                // Remove refreshToken from response body
                result.Data.RefreshToken = null;
            }

            return result.ToActionResult();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest? request)
        {
            var ipAddress = GetIpAddress();
            var userAgent = Request.Headers["User-Agent"].ToString();

            // Get refresh token from HttpOnly cookie
            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken))
            {
                return "Refresh token bulunamadı.".ToUnauthorizedApiResponse();
            }

            var result = await _authService.RefreshTokenAsync(refreshToken, ipAddress, userAgent);

            if (result.Success)
            {
                // Update refresh token cookie
                Response.Cookies.Append("refreshToken", result.Data.RefreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Set to true in production with HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(7)
                });

                // Remove from response body
                result.Data.RefreshToken = null;
            }

            return result.ToActionResult();
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            string? accessToken = null;
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                accessToken = authHeader.Substring("Bearer ".Length).Trim();
            }

            // Get refresh token from cookie
            var refreshToken = Request.Cookies["refreshToken"];

            await _authService.LogoutAsync(accessToken, refreshToken);

            // Clear refresh token cookie
            Response.Cookies.Delete("refreshToken");

            return ((object?)null).ToOkApiResponse("Çıkış başarılı.");
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
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
                // Log the exception here if needed
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = _configuration["Auth:ServerErrorMessage"] ?? "Sunucu hatası oluştu."
                });
            }
        }

        private string GetIpAddress()
        {
            // Check X-Forwarded-For for proxy/load balancer
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].ToString();

            return HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        }
    }
}
