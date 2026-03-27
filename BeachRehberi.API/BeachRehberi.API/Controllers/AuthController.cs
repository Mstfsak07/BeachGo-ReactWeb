using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authService.LoginAsync(request.Email, request.Password);
            if (!result.Success)
            {
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var result = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (!result.Success)
            {
                // If it's a security violation or generic failure
                return Unauthorized(result);
            }

            return Ok(result);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest? request)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            string? accessToken = null;
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                accessToken = authHeader.Substring("Bearer ".Length).Trim();
            }

            await _authService.LogoutAsync(accessToken, request?.RefreshToken);
            return Ok(ApiResponse<string>.SuccessResult(null, "Çıkış başarılı."));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _authService.RegisterAsync(request);
            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
    }
}
