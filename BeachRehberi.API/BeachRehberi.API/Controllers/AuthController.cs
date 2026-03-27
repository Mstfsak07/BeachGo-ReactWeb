using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("auth")] // 5 req / 1 min policy
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