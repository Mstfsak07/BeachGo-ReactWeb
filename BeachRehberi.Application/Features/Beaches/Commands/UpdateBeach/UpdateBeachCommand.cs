using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Beaches.Commands.UpdateBeach;

// ── Command ───────────────────────────────────────────────────────────────────
public record UpdateBeachCommand(
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
    bool HasParking,
    bool HasRestaurant,
    bool HasWaterSports,
    bool HasLifeguard,
    bool IsWheelchairAccessible,
    bool AllowsPets
) : IRequest;

// ── Handler ───────────────────────────────────────────────────────────────────
public class UpdateBeachCommandHandler : IRequestHandler<UpdateBeachCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateBeachCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task Handle(UpdateBeachCommand request, CancellationToken cancellationToken)
    {
        var beach = await _unitOfWork.Beaches.GetByIdAsync(request.Id, cancellationToken);

        if (beach is null || beach.IsDeleted)
            throw new NotFoundException("Plaj", request.Id);

        // Admin her plajı düzenleyebilir; BusinessOwner sadece kendi tenant'ının plajını
        if (!_currentUserService.IsAdmin &&
            beach.TenantId != _currentUserService.TenantId)
            throw new ForbiddenException("Bu plajı düzenleme yetkiniz yok.");

        beach.UpdateDetails(
            request.Name,
            request.Description,
            request.Location,
            request.City,
            request.District,
            request.Phone,
            request.Website,
            request.Instagram,
            request.OpenTime,
            request.CloseTime,
            request.PricePerPerson,
            request.Capacity);

        beach.UpdateAmenities(
            request.HasParking,
            request.HasRestaurant,
            request.HasWaterSports,
            request.HasLifeguard,
            request.IsWheelchairAccessible,
            request.AllowsPets);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
