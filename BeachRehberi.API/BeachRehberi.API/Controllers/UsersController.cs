using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using BeachRehberi.API.Extensions;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly BeachDbContext _db;

    public UsersController(BeachDbContext db)
    {
        _db = db;
    }

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = int.Parse(userIdStr);
        var user = await _db.BusinessUsers
            .Select(u => new 
            {
                u.Id,
                u.Email,
                u.ContactName,
                u.BusinessName,
                u.Role,
                u.CreatedAt
            })
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        return user.ToOkApiResponse();
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = int.Parse(userIdStr);
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        user.UpdateProfile(request.ContactName, request.BusinessName);
        
        await _db.SaveChangesAsync();

        return "Profil başarıyla güncellendi.".ToOkApiResponse();
    }

    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

        var userId = int.Parse(userIdStr);
        var user = await _db.BusinessUsers.FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return NotFound("Kullanıcı bulunamadı.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        {
            return BadRequest("Mevcut şifre hatalı.");
        }

        if (request.NewPassword != request.ConfirmPassword)
        {
            return BadRequest("Yeni şifreler eşleşmiyor.");
        }

        // Reflection or adding a method to BusinessUser would be better, 
        // but for now I'll use reflection if PasswordHash is private set.
        // Looking at BusinessUser.cs, PasswordHash is private set.
        
        var field = typeof(BusinessUser).GetProperty("PasswordHash");
        field?.SetValue(user, BCrypt.Net.BCrypt.HashPassword(request.NewPassword));

        await _db.SaveChangesAsync();

        return "Şifre başarıyla değiştirildi.".ToOkApiResponse();
    }
}

public class UpdateProfileRequest
{
    public string? ContactName { get; set; }
    public string? BusinessName { get; set; }
}

public class ChangePasswordRequest
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
    public required string ConfirmPassword { get; set; }
}
