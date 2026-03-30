using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

public class CreateBeachHandler : IRequestHandler<CreateBeachCommand, Result<CreateBeachResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateBeachHandler> _logger;

    public CreateBeachHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<CreateBeachHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<CreateBeachResponse>> Handle(CreateBeachCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated || !_currentUser.TenantId.HasValue)
            return Result<CreateBeachResponse>.Unauthorized();

        var tenantId = _currentUser.TenantId.Value;

        // Tenant plan limitini kontrol et
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null)
            return Result<CreateBeachResponse>.NotFound("Tenant bulunamadı.");

        var currentBeachCount = await _unitOfWork.Beaches.CountAsync(
            b => b.TenantId == tenantId && !b.IsDeleted, cancellationToken);

        if (currentBeachCount >= tenant.MaxBeaches)
            throw new TenantLimitExceededException($"Maksimum plaj sayısı ({tenant.MaxBeaches})");

        // Plaj oluştur
        var beach = new Beach(
            tenantId: tenantId,
            name: request.Name,
            description: request.Description,
            location: request.Location,
            city: request.City,
            latitude: request.Latitude,
            longitude: request.Longitude,
            pricePerPerson: request.PricePerPerson,
            capacity: request.Capacity
        );

        // Update additional info
        beach.UpdateInfo(
            request.Name, request.Description, request.Location, request.City,
            request.District, request.Latitude, request.Longitude,
            request.PricePerPerson, request.Capacity,
            request.Phone, request.Website, request.Instagram,
            request.OpenTime, request.CloseTime);

        // Set amenities
        beach.SetAmenities(
            request.HasParking, request.HasRestaurant, request.HasWaterSports,
            request.HasLifeguard, request.AllowsPets, hasShower: false,
            hasBar: false, hasWifi: false, hasPool: false, isChildFriendly: false);

        await _unitOfWork.Beaches.AddAsync(beach, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Yeni plaj oluşturuldu: {BeachName}, TenantId: {TenantId}", beach.Name, tenantId);

        return Result<CreateBeachResponse>.Created(
            new CreateBeachResponse(beach.Id, beach.Name, beach.City));
    }
}
