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

    [HttpGet("favorites")]
    public async Task<IActionResult> GetFavorites()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var favorites = await _db.Favorites
            .Where(f => f.UserId == userId.Value)
            .Include(f => f.Beach)
            .OrderByDescending(f => f.CreatedAt)
            .Select(f => new
            {
                f.Beach.Id,
                f.Beach.Name,
                f.Beach.Description,
                f.Beach.Address,
                imageUrl = f.Beach.CoverImageUrl,
                f.Beach.Rating,
                f.Beach.ReviewCount,
                f.Beach.SunbedPrice,
                f.Beach.EntryFee,
                f.Beach.HasEntryFee,
                f.Beach.OpenTime,
                f.Beach.CloseTime,
                f.Beach.OccupancyPercent,
                f.Beach.IsOpen,
                f.Beach.HasBar,
                f.Beach.HasPool,
                f.Beach.HasWifi,
                f.Beach.HasParking,
                f.Beach.HasRestaurant,
                f.Beach.HasWaterSports,
                f.Beach.IsChildFriendly,
                f.Beach.HasSunbeds,
                f.Beach.HasShower,
                f.Beach.HasDJ
            })
            .ToListAsync();

        return favorites.ToOkApiResponse();
    }

    [HttpPost("favorites")]
    public async Task<IActionResult> AddFavorite([FromBody] FavoriteRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var beachExists = await _db.Beaches.AnyAsync(b => b.Id == request.BeachId);
        if (!beachExists) return "Plaj bulunamadı.".ToNotFoundApiResponse();

        var exists = await _db.Favorites.AnyAsync(f => f.UserId == userId.Value && f.BeachId == request.BeachId);
        if (exists) return "Favorilerde zaten mevcut.".ToOkApiResponse();

        _db.Favorites.Add(new Favorite
        {
            UserId = userId.Value,
            BeachId = request.BeachId
        });

        await _db.SaveChangesAsync();
        return "Favorilere eklendi.".ToOkApiResponse();
    }

    [HttpDelete("favorites/{beachId:int}")]
    public async Task<IActionResult> RemoveFavorite(int beachId)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var favorite = await _db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId.Value && f.BeachId == beachId);
        if (favorite == null) return "Favori kaydı bulunamadı.".ToNotFoundApiResponse();

        _db.Favorites.Remove(favorite);
        await _db.SaveChangesAsync();
        return "Favorilerden kaldırıldı.".ToOkApiResponse();
    }

    private int? GetCurrentUserId()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr)) return null;
        return int.Parse(userIdStr);
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

public class FavoriteRequest
{
    public int BeachId { get; set; }
}
