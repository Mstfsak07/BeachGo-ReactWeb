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
            var response = await _authService.LoginAsync(request.Email, request.Password);
            if (response == null)
                return Unauthorized(ApiResponse<string>.FailureResult("E-posta veya şifre hatalı."));

            return Ok(ApiResponse<AuthResponse>.SuccessResult(response));
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var response = await _authService.RefreshTokenAsync(request.RefreshToken);
            if (response == null)
                return Unauthorized(ApiResponse<string>.FailureResult("Geçersiz veya süresi dolmuş refresh token."));

            return Ok(ApiResponse<AuthResponse>.SuccessResult(response));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            var authHeader = Request.Headers["Authorization"].ToString();
            string? accessToken = null;
            if (authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) {
                 accessToken = authHeader.Substring("Bearer ".Length).Trim();
            }

            await _authService.LogoutAsync(accessToken, request.RefreshToken);
            return Ok(ApiResponse<string>.SuccessResult(null, "Çıkış başarılı."));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var user = await _authService.RegisterAsync(request);
            if (user == null)
                return BadRequest(ApiResponse<string>.FailureResult("Bu e-posta adresi zaten kullanımda."));   

            return Ok(ApiResponse<BusinessUser>.SuccessResult(user, "Kayıt başarılı."));
        }
    }
}
