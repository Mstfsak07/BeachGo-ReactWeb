using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;

namespace BeachRehberi.API.Services;

public class ReviewService : IReviewService
{
    private readonly BeachDbContext _db;

    public ReviewService(BeachDbContext db) => _db = db;

    public async Task<List<PublicReviewDto>> GetByBeachAsync(int beachId)
    {
        return await _db.Reviews
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
    }

    public async Task<ServiceResult<Review>> CreateAsync(CreateReviewDto dto, int userId)
    {
        var existingReview = await _db.Reviews.AnyAsync(r => r.BeachId == dto.BeachId && r.UserId == userId);
        if (existingReview)
            return ServiceResult<Review>.FailureResult("Bu plaj için zaten yorum yaptınız.");

        try 
        {
            var review = new Review(
                dto.BeachId,
                userId,
                HtmlEncoder.Default.Encode(dto.UserName),
                HtmlEncoder.Default.Encode(dto.UserPhone),
                dto.Rating,
                HtmlEncoder.Default.Encode(dto.Comment)
            );

            _db.Reviews.Add(review);

            var beach = await _db.Beaches.FindAsync(dto.BeachId);
            if (beach != null)
            {
                var ratings = await _db.Reviews
                    .Where(r => r.BeachId == dto.BeachId && r.IsApproved)
                    .Select(r => r.Rating)
                    .ToListAsync();

                ratings.Add(dto.Rating);
                var average = Math.Round(ratings.Average(), 1);
                beach.UpdateRating(average, ratings.Count);
            }

            await _db.SaveChangesAsync();
            return ServiceResult<Review>.SuccessResult(review, "Yorumunuz için teşekkürler.");
        }
        catch (DomainException ex)
        {
            return ServiceResult<Review>.FailureResult(ex.Message);
        }
    }
}

