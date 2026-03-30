using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Beaches.Queries.GetBeaches;

public class GetBeachesQueryHandler
    : IRequestHandler<GetBeachesQuery, Result<PagedResult<BeachListItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetBeachesQueryHandler> _logger;

    public GetBeachesQueryHandler(IUnitOfWork unitOfWork, ILogger<GetBeachesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedResult<BeachListItemDto>>> Handle(
        GetBeachesQuery request, CancellationToken cancellationToken)
    {
        var pageSize = Math.Clamp(request.PageSize, 1, 50);
        var pageNumber = Math.Max(request.PageNumber, 1);

        var query = _unitOfWork.Beaches.Query()
            .Where(b => b.IsActive && !b.IsDeleted);

        // Arama filtresi
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(b =>
                b.Name.ToLower().Contains(term) ||
                b.Description.ToLower().Contains(term) ||
                b.City.ToLower().Contains(term));
        }

        // Şehir filtresi
        if (!string.IsNullOrWhiteSpace(request.City))
            query = query.Where(b => b.City == request.City);

        // Fiyat filtresi
        if (request.MinPrice.HasValue)
            query = query.Where(b => b.PricePerPerson >= request.MinPrice.Value);
        if (request.MaxPrice.HasValue)
            query = query.Where(b => b.PricePerPerson <= request.MaxPrice.Value);

        // Özellik filtreleri
        if (request.HasParking == true)     query = query.Where(b => b.HasParking);
        if (request.HasRestaurant == true)  query = query.Where(b => b.HasRestaurant);
        if (request.HasWaterSports == true) query = query.Where(b => b.HasWaterSports);
        if (request.HasLifeguard == true)   query = query.Where(b => b.HasLifeguard);
        if (request.IsPetFriendly == true)  query = query.Where(b => b.IsPetFriendly);

        // Sıralama
        query = request.SortBy.ToLower() switch
        {
            "price"  => request.SortDescending
                            ? query.OrderByDescending(b => b.PricePerPerson)
                            : query.OrderBy(b => b.PricePerPerson),
            "name"   => request.SortDescending
                            ? query.OrderByDescending(b => b.Name)
                            : query.OrderBy(b => b.Name),
            "newest" => query.OrderByDescending(b => b.CreatedAt),
            _        => request.SortDescending
                            ? query.OrderByDescending(b => b.AverageRating)
                            : query.OrderBy(b => b.AverageRating)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new BeachListItemDto(
                b.Id,
                b.Name,
                b.City,
                b.District,
                b.CoverImageUrl, // maps to ImageUrl
                b.PricePerPerson,
                b.AverageRating,
                b.TotalReviews,
                b.IsVerified,
                b.HasParking,
                b.HasRestaurant,
                b.HasWaterSports,
                b.HasLifeguard,
                b.IsPetFriendly,
                b.Latitude,
                b.Longitude))
            .ToListAsync(cancellationToken);

        _logger.LogDebug(
            "Beach listesi sorgulandı → Toplam: {Total}, Sayfa: {Page}/{TotalPages}",
            totalCount, pageNumber, (int)Math.Ceiling(totalCount / (double)pageSize));

        return Result<PagedResult<BeachListItemDto>>.Success(
            PagedResult<BeachListItemDto>.Create(items, totalCount, pageNumber, pageSize));
    }
}
