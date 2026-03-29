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
    private readonly ICacheService _cacheService;
    private readonly ILogger<CreateBeachHandler> _logger;

    public CreateBeachHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICacheService cacheService,
        ILogger<CreateBeachHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cacheService = cacheService;
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
            request.Name,
            request.Description,
            request.Address,
            request.Latitude,
            request.Longitude,
            _currentUser.UserId!.Value,
            tenantId,
            request.Capacity
        );

        beach.UpdateInfo(
            request.Name, request.Description, request.Address,
            request.Phone ?? "", request.Website ?? "", request.Instagram ?? "",
            request.OpenTime ?? "", request.CloseTime ?? "", "");

        beach.UpdatePricing(request.HasEntryFee, request.EntryFee, request.SunbedPrice);

        beach.UpdateAmenities(
            request.HasSunbeds, request.HasShower, request.HasParking,
            request.HasRestaurant, request.HasBar, request.HasAlcohol,
            request.IsChildFriendly, request.HasWaterSports, request.HasWifi,
            request.HasPool, request.HasDJ, request.HasAccessibility);

        await _unitOfWork.Beaches.AddAsync(beach, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Cache temizle
        await _cacheService.RemoveByPrefixAsync("beaches:", cancellationToken);

        _logger.LogInformation("Yeni plaj oluşturuldu: {BeachName}, TenantId: {TenantId}", beach.Name, tenantId);

        return Result<CreateBeachResponse>.Created(
            new CreateBeachResponse(beach.Id, beach.Name, beach.Name.ToLower().Replace(" ", "-")));
    }
}
