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

    [HttpGet("beach/{beachId}")]
    public async Task<IActionResult> GetByBeach(int beachId)
    {
        var reviews = await _db.Reviews
            .Where(r => r.BeachId == beachId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(20)
            .ToListAsync();
        return Ok(ApiResponse<List<Review>>.SuccessResult(reviews));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReviewDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5) 
            return BadRequest(ApiResponse<string>.FailureResult("Puan 1-5 arasında olmalıdır."));

        var review = new Review
        {
            BeachId = dto.BeachId,
            UserName = dto.UserName,
            UserPhone = dto.UserPhone,
            Rating = dto.Rating,
            Comment = dto.Comment,
            IsApproved = true // Otomatik onay (Üretim ortamında false yapılabilir)
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
        return Ok(ApiResponse<Review>.SuccessResult(review, "Yorumunuz için teşekkürler."));
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
