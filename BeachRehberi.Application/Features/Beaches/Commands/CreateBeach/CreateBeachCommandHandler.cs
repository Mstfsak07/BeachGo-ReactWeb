using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

public class CreateBeachCommandHandler : IRequestHandler<CreateBeachCommand, Result<CreateBeachResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateBeachCommandHandler> _logger;

    public CreateBeachCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<CreateBeachCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<CreateBeachResponse>> Handle(
        CreateBeachCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _currentUser.TenantId
            ?? throw new UnauthorizedException("Bu işlem için geçerli bir tenant gereklidir.");

        // Tenant varlık ve plan limiti kontrolü
        var tenant = await _unitOfWork.Tenants.GetByIdAsync(tenantId, cancellationToken)
            ?? throw new NotFoundException(nameof(Tenant), tenantId);

        if (!tenant.IsActive)
            throw new BusinessRuleException("Tenant hesabı aktif değil.");

        var beachCount = await _unitOfWork.Beaches.CountAsync(
            b => b.TenantId == tenantId && !b.IsDeleted, cancellationToken);

        if (beachCount >= tenant.MaxBeaches)
            throw new TenantLimitExceededException("Beach sayısı");

        // Beach oluştur
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

        // İletişim ve saat bilgilerini güncelle
        beach.UpdateInfo(
            request.Name, request.Description, request.Location, request.City,
            request.District, request.Latitude, request.Longitude,
            request.PricePerPerson, request.Capacity,
            request.Phone, request.Website, request.Instagram,
            request.OpenTime, request.CloseTime);

        // Özellikler
        beach.SetAmenities(
            request.HasParking, request.HasRestaurant, request.HasWaterSports,
            request.HasLifeguard, request.IsPetFriendly, request.HasShower,
            request.HasBar, request.HasWifi, request.HasPool, request.IsChildFriendly);

        await _unitOfWork.Beaches.AddAsync(beach, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Beach oluşturuldu → Id: {BeachId}, Ad: {Name}, TenantId: {TenantId}",
            beach.Id, beach.Name, tenantId);

        return Result<CreateBeachResponse>.Created(
            new CreateBeachResponse(beach.Id, beach.Name, beach.City),
            "Beach başarıyla oluşturuldu.");
    }
}
