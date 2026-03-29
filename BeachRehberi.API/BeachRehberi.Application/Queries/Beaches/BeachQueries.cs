using BeachRehberi.Application.Common;
using BeachRehberi.Application.DTOs;

namespace BeachRehberi.Application.Queries.Beaches;

/// <summary>
/// Get beach by id query
/// </summary>
public class GetBeachByIdQuery : QueryBase<BeachDto>
{
    public int Id { get; set; }
}

/// <summary>
/// Get all beaches query
/// </summary>
public class GetAllBeachesQuery : QueryBase<List<BeachDto>>
{
    public string? SearchTerm { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public double? RadiusKm { get; set; }
    public bool? HasParking { get; set; }
    public bool? HasRestaurant { get; set; }
    public bool? IsChildFriendly { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Get beaches by location query
/// </summary>
public class GetBeachesByLocationQuery : QueryBase<List<BeachDto>>
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 10;
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

/// <summary>
/// Get nearby beaches query
/// </summary>
public class GetNearbyBeachesQuery : QueryBase<List<BeachDto>>
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusKm { get; set; } = 5;
    public int MaxResults { get; set; } = 10;
}