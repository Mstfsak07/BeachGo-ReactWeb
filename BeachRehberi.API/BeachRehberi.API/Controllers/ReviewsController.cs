using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewsController : ControllerBase
{
    private readonly BeachDbContext _db;

    public ReviewsController(BeachDbContext db) => _db = db;

    // GET api/reviews/beach/5
    [HttpGet("beach/{beachId}")]
    public async Task<IActionResult> GetByBeach(int beachId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.BeachId == beachId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToListAsync();
        return Ok(reviews);
    }

    // POST api/reviews
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        var review = new Review
        {
            BeachId = dto.BeachId,
            UserName = dto.UserName,
            UserPhone = dto.UserPhone,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Comment = dto.Comment,
            Source = "app"
        };

        _db.Reviews.Add(review);

        // Plaj ortalama puanını güncelle
        var beach = await _db.Beaches.FindAsync(dto.BeachId);
        if (beach != null)
        {
            var allRatings = await _db.Reviews
                .Where(r => r.BeachId == dto.BeachId && r.IsApproved)
                .Select(r => r.Rating)
                .ToListAsync();
            allRatings.Add(dto.Rating);
            beach.Rating = Math.Round(allRatings.Average(), 1);
            beach.ReviewCount = allRatings.Count;
        }

        await _db.SaveChangesAsync();
        return Ok(new { Message = "Yorum eklendi!", review.Id });
    }
}

public class CreateReviewDto
{
    public int BeachId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}