using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;

namespace BeachRehberi.API.Services;

public interface IReviewService
{
    Task<List<PublicReviewDto>> GetByBeachAsync(int beachId);
    Task<ServiceResult<Review>> CreateAsync(CreateReviewDto dto, int userId);
}
