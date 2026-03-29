using BeachRehberi.Application.Common.Behaviors;
using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;

public record GetBeachesQuery(
    string? Search = null,
    string? City = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? HasParking = null,
    bool? HasRestaurant = null,
    bool? HasWaterSports = null,
    bool? HasLifeguard = null,
    bool? IsPetFriendly = null,
    double? Latitude = null,
    double? Longitude = null,
    double? RadiusKm = null,
    string SortBy = "rating",
    bool SortDescending = true,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<BeachListItemDto>>>, ICacheable
{
    public string CacheKey =>
        $"beaches:{City}:{Search}:{MinPrice}:{MaxPrice}:{HasParking}:{HasRestaurant}:" +
        $"{HasWaterSports}:{HasLifeguard}:{IsPetFriendly}:{SortBy}:{SortDescending}:" +
        $"{PageNumber}:{PageSize}";

    public TimeSpan? CacheExpiry => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
}

public record BeachListItemDto(
    int Id,
    string Name,
    string City,
    string? District,
    string? CoverImageUrl,
    decimal PricePerPerson,
    double AverageRating,
    int TotalReviews,
    bool IsVerified,
    bool HasParking,
    bool HasRestaurant,
    bool HasWaterSports,
    bool HasLifeguard,
    bool IsPetFriendly,
    double Latitude,
    double Longitude
);
