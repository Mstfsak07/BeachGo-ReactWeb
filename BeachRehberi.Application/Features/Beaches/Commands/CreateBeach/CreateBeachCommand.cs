using BeachRehberi.Domain.Common;
using MediatR;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

public record CreateBeachCommand(
    string Name,
    string Description,
    string Location,
    string City,
    string? District,
    double Latitude,
    double Longitude,
    decimal PricePerPerson,
    int Capacity,
    string? Phone = null,
    string? Website = null,
    string? Instagram = null,
    string? OpenTime = null,
    string? CloseTime = null,
    bool HasParking = false,
    bool HasRestaurant = false,
    bool HasWaterSports = false,
    bool HasLifeguard = false,
    bool IsPetFriendly = false,
    bool HasShower = false,
    bool HasBar = false,
    bool HasWifi = false,
    bool HasPool = false,
    bool IsChildFriendly = false
) : IRequest<Result<CreateBeachResponse>>;

public record CreateBeachResponse(int Id, string Name, string City);
