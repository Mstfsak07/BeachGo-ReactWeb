using BeachRehberi.Application.Common.Behaviors;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;

// ── Query ─────────────────────────────────────────────────────────────────────
public record GetBeachesQuery(
    string? City = null,
    string? Search = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? HasParking = null,
    bool? HasRestaurant = null,
    bool? HasWaterSports = null,
    bool? HasLifeguard = null,
    bool? IsPetFriendly = null,
    string SortBy = "rating",
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<Result<PagedResult<BeachListItemDto>>>, ICacheable
{
    public string CacheKey =>
        $"beaches:{City}:{Search}:{MinPrice}:{MaxPrice}:" +
        $"{HasParking}:{HasRestaurant}:{HasWaterSports}:{HasLifeguard}:{IsPetFriendly}:" +
        $"{SortBy}:{SortDescending}:{PageNumber}:{PageSize}";

    public TimeSpan? CacheExpiry => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
}

// ── DTO ───────────────────────────────────────────────────────────────────────
public record BeachListItemDto(
    int Id,
    string Name,
    string City,
    string? District,
    string? CoverImageUrl,
    decimal PricePerPerson,
    decimal AverageRating,
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
