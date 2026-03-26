using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Mvc;
using static System.Net.WebRequestMethods;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    // POST api/auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var token = await _authService.LoginAsync(dto.Email, dto.Password);
        if (token == null)
            return Unauthorized(new { Message = "E-posta veya şifre hatalı." });

        return Ok(new { Token = token, Message = "Giriş başarılı!" });
    }
    // POST api/auth/hash - geçici, test için
    [HttpPost("hash")]
    public IActionResult HashPassword([FromBody] string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Ok(Convert.ToBase64String(hash));
    }
    // GET api/auth/testuser - geçici test
    [HttpGet("testuser")]
    public async Task<IActionResult> TestUser()
    {
        var user = await _authService.GetUserAsync("kalypso@beach.com");
        if (user == null) return NotFound("Kullanıcı bulunamadı!");
        return Ok(new { user.Email, user.IsActive, user.BeachId, HashStart = user.PasswordHash[..10] });
    }


}

public class LoginDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}