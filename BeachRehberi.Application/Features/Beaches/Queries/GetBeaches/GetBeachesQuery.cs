using BeachRehberi.Application.Common.Behaviors;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;

// ── Query ─────────────────────────────────────────────────────────────────────
public record GetBeachesQuery(
    string? City = null,
    string? SearchTerm = null,
    decimal? MinPrice = null,
    decimal? MaxPrice = null,
    bool? HasParking = null,
    bool? HasRestaurant = null,
    bool? HasWaterSports = null,
    string? SortBy = null,
    bool SortDescending = false,
    int PageNumber = 1,
    int PageSize = 20
) : IRequest<PagedResult<BeachListDto>>, ICacheable
{
    public string CacheKey =>
        $"beaches:{City}:{SearchTerm}:{MinPrice}:{MaxPrice}:" +
        $"{HasParking}:{HasRestaurant}:{HasWaterSports}:" +
        $"{SortBy}:{SortDescending}:{PageNumber}:{PageSize}";

    public TimeSpan? CacheExpiry => TimeSpan.FromMinutes(5);
    public bool BypassCache => false;
}

// ── DTO ───────────────────────────────────────────────────────────────────────
public record BeachListDto(
    int Id,
    string Name,
    string City,
    string? District,
    string Location,
    decimal PricePerPerson,
    decimal AverageRating,
    int ReviewCount,
    string? CoverImageUrl,
    bool HasParking,
    bool HasRestaurant,
    bool HasWaterSports,
    bool HasLifeguard,
    double Latitude,
    double Longitude
);

// ── Handler ───────────────────────────────────────────────────────────────────
public class GetBeachesQueryHandler : IRequestHandler<GetBeachesQuery, PagedResult<BeachListDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBeachesQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<PagedResult<BeachListDto>> Handle(
        GetBeachesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Beaches.Query()
            .Where(b => !b.IsDeleted);

        // ── Filtreleme ────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(b =>
                b.City.ToLower().Contains(request.City.ToLower()));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query = query.Where(b =>
                b.Name.ToLower().Contains(request.SearchTerm.ToLower()) ||
                b.Description.ToLower().Contains(request.SearchTerm.ToLower()) ||
                b.Location.ToLower().Contains(request.SearchTerm.ToLower()));

        if (request.MinPrice.HasValue)
            query = query.Where(b => b.PricePerPerson >= request.MinPrice.Value);

        if (request.MaxPrice.HasValue)
            query = query.Where(b => b.PricePerPerson <= request.MaxPrice.Value);

        if (request.HasParking.HasValue)
            query = query.Where(b => b.HasParking == request.HasParking.Value);

        if (request.HasRestaurant.HasValue)
            query = query.Where(b => b.HasRestaurant == request.HasRestaurant.Value);

        if (request.HasWaterSports.HasValue)
            query = query.Where(b => b.HasWaterSports == request.HasWaterSports.Value);

        // ── Sıralama ──────────────────────────────────────────────
        query = request.SortBy?.ToLowerInvariant() switch
        {
            "price" => request.SortDescending
                ? query.OrderByDescending(b => b.PricePerPerson)
                : query.OrderBy(b => b.PricePerPerson),
            "name" => request.SortDescending
                ? query.OrderByDescending(b => b.Name)
                : query.OrderBy(b => b.Name),
            "rating" => request.SortDescending
                ? query.OrderByDescending(b => b.AverageRating)
                : query.OrderBy(b => b.AverageRating),
            _ => query.OrderByDescending(b => b.AverageRating)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(b => new BeachListDto(
                b.Id,
                b.Name,
                b.City,
                b.District,
                b.Location,
                b.PricePerPerson,
                b.AverageRating,
                b.ReviewCount,
                b.CoverImageUrl,
                b.HasParking,
                b.HasRestaurant,
                b.HasWaterSports,
                b.HasLifeguard,
                b.Latitude,
                b.Longitude))
            .ToListAsync(cancellationToken);

        return PagedResult<BeachListDto>.Create(
            items, totalCount, request.PageNumber, request.PageSize);
    }
}
