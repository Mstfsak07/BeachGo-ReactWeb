using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class Beach
{
    public int Id { get; set; }

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

    public double Rating { get; set; }
    public int ReviewCount { get; set; }

    public string GooglePlaceId { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;

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
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsOpen { get; set; }
    public string TodaySpecial { get; set; } = string.Empty;
    
    public bool IsDeleted { get; set; } = false; // Soft Delete

    public ICollection<BeachPhoto> Photos { get; set; } = new List<BeachPhoto>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<BeachEvent> Events { get; set; } = new List<BeachEvent>();
}

public enum OccupancyLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Full = 4
}
