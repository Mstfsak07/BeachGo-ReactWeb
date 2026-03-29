using BeachRehberi.Application.Common.Behaviors;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Application.Features.Beaches.Queries.GetBeachById;

// ── Query ─────────────────────────────────────────────────────────────────────
public record GetBeachByIdQuery(int Id) : IRequest<BeachDetailDto>, ICacheable
{
    public string CacheKey => $"beach:{Id}";
    public TimeSpan? CacheExpiry => TimeSpan.FromMinutes(10);
    public bool BypassCache => false;
}

// ── DTOs ──────────────────────────────────────────────────────────────────────
public record BeachDetailDto(
    int Id,
    string Name,
    string Description,
    string Location,
    string City,
    string? District,
    double Latitude,
    double Longitude,
    string? Phone,
    string? Website,
    string? Instagram,
    string? OpenTime,
    string? CloseTime,
    decimal PricePerPerson,
    int Capacity,
    decimal AverageRating,
    int ReviewCount,
    string? CoverImageUrl,
    bool HasParking,
    bool HasRestaurant,
    bool HasWaterSports,
    bool HasLifeguard,
    bool IsWheelchairAccessible,
    bool AllowsPets,
    List<BeachPhotoDto> Photos,
    List<BeachReviewSummaryDto> Reviews
);

public record BeachPhotoDto(
    int Id,
    string Url,
    string? Caption,
    bool IsCover,
    int DisplayOrder
);

public record BeachReviewSummaryDto(
    int Id,
    string UserFullName,
    int Rating,
    string Comment,
    DateTime CreatedAt
);

// ── Handler ───────────────────────────────────────────────────────────────────
public class GetBeachByIdQueryHandler : IRequestHandler<GetBeachByIdQuery, BeachDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetBeachByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<BeachDetailDto> Handle(
        GetBeachByIdQuery request, CancellationToken cancellationToken)
    {
        var beach = await _unitOfWork.Beaches.Query()
            .Include(b => b.Photos)
            .Include(b => b.Reviews.Where(r => r.IsApproved && !r.IsDeleted))
                .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.Id == request.Id && !b.IsDeleted, cancellationToken);

        if (beach is null)
            throw new NotFoundException("Plaj", request.Id);

        return new BeachDetailDto(
            beach.Id,
            beach.Name,
            beach.Description,
            beach.Location,
            beach.City,
            beach.District,
            beach.Latitude,
            beach.Longitude,
            beach.Phone,
            beach.Website,
            beach.Instagram,
            beach.OpenTime,
            beach.CloseTime,
            beach.PricePerPerson,
            beach.Capacity,
            beach.AverageRating,
            beach.ReviewCount,
            beach.CoverImageUrl,
            beach.HasParking,
            beach.HasRestaurant,
            beach.HasWaterSports,
            beach.HasLifeguard,
            beach.IsWheelchairAccessible,
            beach.AllowsPets,
            beach.Photos
                .Where(p => !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .Select(p => new BeachPhotoDto(
                    p.Id, p.Url, p.Caption, p.IsCover, p.DisplayOrder))
                .ToList(),
            beach.Reviews
                .Select(r => new BeachReviewSummaryDto(
                    r.Id,
                    r.User!.FullName,
                    r.Rating,
                    r.Comment,
                    r.CreatedAt))
                .ToList());
    }
}
