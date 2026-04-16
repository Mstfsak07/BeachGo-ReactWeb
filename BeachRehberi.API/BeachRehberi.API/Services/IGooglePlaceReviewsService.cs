using BeachRehberi.API.DTOs;

namespace BeachRehberi.API.Services;

public interface IGooglePlaceReviewsService
{
    Task<GoogleReviewsResponseDto?> GetBeachReviewsAsync(int beachId, CancellationToken cancellationToken = default);
}
