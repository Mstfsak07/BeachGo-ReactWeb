using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class ReviewsController : ControllerBase
{
    private readonly BeachDbContext _db;

    public ReviewsController(BeachDbContext db) => _db = db;

    [HttpGet("beach/{beachId}")]
    public async Task<IActionResult> GetByBeach(int beachId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.BeachId == beachId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .Select(r => new PublicReviewDto {
                UserName = r.UserName,
                Rating = r.Rating,
                Comment = r.Comment,
                CreatedAt = r.CreatedAt
            })
            .ToListAsync();
            
        return reviews.ToOkApiResponse();
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) 
            return "Kullanıcı kimliği doğrulanamadı.".ToUnauthorizedApiResponse();

        if (dto.Rating < 1 || dto.Rating > 5)
            return "Puan 1-5 arasında olmalıdır.".ToBadRequestApiResponse();

        var existingReview = await _db.Reviews.AnyAsync(r => r.BeachId == dto.BeachId && r.UserId == userId);
        if (existingReview)
            return "Bu plaj için zaten yorum yaptınız.".ToBadRequestApiResponse();

        var review = new Review
        {
            UserId = userId,
            BeachId = dto.BeachId,
            UserName = HtmlEncoder.Default.Encode(dto.UserName),
            UserPhone = HtmlEncoder.Default.Encode(dto.UserPhone),
            Rating = dto.Rating,
            Comment = HtmlEncoder.Default.Encode(dto.Comment),
            IsApproved = true
        };

        _db.Reviews.Add(review);

        var beach = await _db.Beaches.FindAsync(dto.BeachId);
        if (beach != null)
        {
            var ratings = await _db.Reviews
                .Where(r => r.BeachId == dto.BeachId && r.IsApproved)
                .Select(r => r.Rating)
                .ToListAsync();

            ratings.Add(dto.Rating);
            beach.Rating = Math.Round(ratings.Average(), 1);
        }

        await _db.SaveChangesAsync();
        return review.ToOkApiResponse("Yorumunuz için teşekkürler.");
    }
}

public class PublicReviewDto
{
    public string UserName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateReviewDto
{
    public int BeachId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}
