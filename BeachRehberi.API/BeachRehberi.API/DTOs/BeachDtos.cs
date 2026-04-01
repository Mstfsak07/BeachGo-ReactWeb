using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.DTOs;

public class CreateBeachRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    public int Capacity { get; set; }
}

public class CreateBeachDto
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    [Required]
    [Range(-90, 90)]
    public double Latitude { get; set; }

    [Required]
    [Range(-180, 180)]
    public double Longitude { get; set; }

    public int Capacity { get; set; }
}

public class UpdateBeachDto
{
    [Required]
    [MaxLength(200)]
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

    public int Capacity { get; set; }
    
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

    public string TodaySpecial { get; set; } = string.Empty;
}

public class BusinessStatsDto
{
    public int TotalReservations { get; set; }
    public int TodayCheckins { get; set; }
    public int MonthlyReservations { get; set; }
    public int ActiveCustomers { get; set; }
    public decimal EstimatedEarnings { get; set; }
    public List<WeeklyStatDto> WeeklyData { get; set; } = new();
}

public class WeeklyStatDto
{
    public string Day { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class BeachResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsActive { get; set; }
    public int Capacity { get; set; }
    public bool IsOpen { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public int OccupancyPercent { get; set; }
    public string OpenTime { get; set; } = string.Empty;
    public string CloseTime { get; set; } = string.Empty;
    public bool HasEntryFee { get; set; }
    public decimal EntryFee { get; set; }
    public decimal SunbedPrice { get; set; }
    public List<string> Facilities { get; set; } = new();
}
