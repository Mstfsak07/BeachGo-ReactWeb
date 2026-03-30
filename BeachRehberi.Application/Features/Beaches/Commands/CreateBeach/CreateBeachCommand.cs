using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Exceptions;
using BeachRehberi.Domain.Interfaces;
using MediatR;

namespace BeachRehberi.Application.Features.Beaches.Commands.CreateBeach;

// ── Command ───────────────────────────────────────────────────────────────────
public record CreateBeachCommand(
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
) : IRequest<Result<CreateBeachResponse>>;

public record CreateBeachResponse(int Id, string Name, string City);
