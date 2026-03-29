using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

public record CreateBeachCommand(
    string Name,
    string Description,
    string Address,
    double Latitude,
    double Longitude,
    string? Phone,
    string? Website,
    string? Instagram,
    string? OpenTime,
    string? CloseTime,
    bool HasEntryFee,
    decimal EntryFee,
    decimal SunbedPrice,
    int Capacity,
    bool HasSunbeds,
    bool HasShower,
    bool HasParking,
    bool HasRestaurant,
    bool HasBar,
    bool HasAlcohol,
    bool IsChildFriendly,
    bool HasWaterSports,
    bool HasWifi,
    bool HasPool,
    bool HasDJ,
    bool HasAccessibility
) : IRequest<Result<CreateBeachResponse>>;

public record CreateBeachResponse(int Id, string Name, string Slug);
