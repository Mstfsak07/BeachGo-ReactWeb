using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;

public class GetBeachesHandler : IRequestHandler<GetBeachesQuery, Result<PagedResult<BeachListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetBeachesHandler> _logger;

    public GetBeachesHandler(IUnitOfWork unitOfWork, ICacheService cacheService, ILogger<GetBeachesHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<BeachListItemDto>>> Handle(GetBeachesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Beaches.Query()
            .Where(b => !b.IsDeleted && b.IsActive);

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(search) ||
                b.Address.ToLower().Contains(search) ||
                b.Description.ToLower().Contains(search));
        }

        // Özellik filtreleri
        if (request.HasParking == true) query = query.Where(b => b.HasParking);
        if (request.HasRestaurant == true) query = query.Where(b => b.HasRestaurant);
        if (request.HasWifi == true) query = query.Where(b => b.HasWifi);
        if (request.IsChildFriendly == true) query = query.Where(b => b.IsChildFriendly);
        if (request.HasPool == true) query = query.Where(b => b.HasPool);
        if (request.MaxEntryFee.HasValue) query = query.Where(b => !b.HasEntryFee || b.EntryFee <= request.MaxEntryFee.Value);
        if (request.MinRating.HasValue) query = query.Where(b => b.Rating >= request.MinRating.Value);

        // Sıralama
        query = request.SortBy?.ToLower() switch
        {
            "rating" => request.SortDesc ? query.OrderByDescending(b => b.Rating) : query.OrderBy(b => b.Rating),
            "name" => request.SortDesc ? query.OrderByDescending(b => b.Name) : query.OrderBy(b => b.Name),
            "entryfee" => request.SortDesc ? query.OrderByDescending(b => b.EntryFee) : query.OrderBy(b => b.EntryFee),
            _ => query.OrderByDescending(b => b.Rating)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var beaches = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BeachListItemDto(
                b.Id,
                b.Name,
                b.Address,
                b.CoverImageUrl,
                b.Rating,
                b.ReviewCount,
                b.HasEntryFee,
                b.EntryFee,
                b.IsOpen,
                b.OccupancyPercent,
                b.OccupancyLevel.ToString(),
                b.Latitude,
                b.Longitude,
                null,
                b.HasParking,
                b.HasWifi,
                b.HasPool,
                b.IsChildFriendly
            ))
            .ToListAsync(cancellationToken);

        // Mesafe hesapla
        if (request.Lat.HasValue && request.Lng.HasValue)
        {
            var beachesWithDistance = beaches
                .Select(b => b with
                {
                    DistanceKm = CalculateDistance(request.Lat.Value, request.Lng.Value, b.Latitude, b.Longitude)
                })
                .ToList();

            if (request.RadiusKm.HasValue)
                beachesWithDistance = beachesWithDistance
                    .Where(b => b.DistanceKm <= request.RadiusKm.Value)
                    .ToList();

            beachesWithDistance = beachesWithDistance.OrderBy(b => b.DistanceKm).ToList();

            return Result<PagedResult<BeachListItemDto>>.Success(
                PagedResult<BeachListItemDto>.Create(beachesWithDistance, totalCount, request.Page, request.PageSize));
        }

        return Result<PagedResult<BeachListItemDto>>.Success(
            PagedResult<BeachListItemDto>.Create(beaches, totalCount, request.Page, request.PageSize));
    }

    private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = ToRad(lat2 - lat1);
        var dLon = ToRad(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRad(lat1)) * Math.Cos(ToRad(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return Math.Round(R * c, 2);
    }

    private static double ToRad(double deg) => deg * (Math.PI / 180);
}
