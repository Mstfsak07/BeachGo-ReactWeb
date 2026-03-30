using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Application.DTOs;

/// <summary>
/// Beach data transfer object
/// </summary>
public class BeachDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;

    public bool HasEntryFee { get; set; }
    public decimal EntryFee { get; set; }
    public decimal SunbedPrice { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public double Rating { get; set; }
    public int ReviewCount { get; set; }

    public string GooglePlaceId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    // Facilities
    public bool HasSunbeds { get; set; }
    public bool HasShower { get; set; }
    public bool HasParking { get; set; }
    public bool HasRestaurant { get; set; }
    public bool HasBar { get; set; }
    public bool HasAlcohol { get; set; }
    public bool IsChildFriendly { get; set; }
    public bool HasWaterSports { get; set; }
    public bool HasWifi { get; set; }
    public bool HasPool { get; set; }
    public bool HasDJ { get; set; }
    public bool HasAccessibility { get; set; }

    public int OccupancyPercent { get; set; }
    public OccupancyLevel OccupancyLevel { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Beach creation DTO
/// </summary>
public class CreateBeachDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;

    public bool HasEntryFee { get; set; }
    public decimal EntryFee { get; set; }
    public decimal SunbedPrice { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string GooglePlaceId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    // Facilities
    public bool HasSunbeds { get; set; }
    public bool HasShower { get; set; }
    public bool HasParking { get; set; }
    public bool HasRestaurant { get; set; }
    public bool HasBar { get; set; }
    public bool HasAlcohol { get; set; }
    public bool IsChildFriendly { get; set; }
    public bool HasWaterSports { get; set; }
    public bool HasWifi { get; set; }
    public bool HasPool { get; set; }
    public bool HasDJ { get; set; }
    public bool HasAccessibility { get; set; }
}

/// <summary>
/// Beach update DTO
/// </summary>
public class UpdateBeachDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;

    public bool HasEntryFee { get; set; }
    public decimal EntryFee { get; set; }
    public decimal SunbedPrice { get; set; }

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public string GooglePlaceId { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;

    // Facilities
    public bool HasSunbeds { get; set; }
    public bool HasShower { get; set; }
    public bool HasParking { get; set; }
    public bool HasRestaurant { get; set; }
    public bool HasBar { get; set; }
    public bool HasAlcohol { get; set; }
    public bool IsChildFriendly { get; set; }
    public bool HasWaterSports { get; set; }
    public bool HasWifi { get; set; }
    public bool HasPool { get; set; }
    public bool HasDJ { get; set; }
    public bool HasAccessibility { get; set; }
}