using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;

public record GetBeachesQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null,
    double? Lat = null,
    double? Lng = null,
    double? RadiusKm = null,
    bool? HasParking = null,
    bool? HasRestaurant = null,
    bool? HasWifi = null,
    bool? IsChildFriendly = null,
    bool? HasPool = null,
    decimal? MaxEntryFee = null,
    double? MinRating = null,
    string? SortBy = null,
    bool SortDesc = false
) : IRequest<Result<PagedResult<BeachListItemDto>>>;

public record BeachListItemDto(
    int Id,
    string Name,
    string Address,
    string CoverImageUrl,
    double Rating,
    int ReviewCount,
    bool HasEntryFee,
    decimal EntryFee,
    bool IsOpen,
    int OccupancyPercent,
    string OccupancyLevel,
    double Latitude,
    double Longitude,
    double? DistanceKm,
    bool HasParking,
    bool HasWifi,
    bool HasPool,
    bool IsChildFriendly
);
