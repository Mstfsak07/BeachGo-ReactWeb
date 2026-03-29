using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class Beach
{
    public int Id { get; private set; }

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

    public double Rating { get; private set; }
    public int ReviewCount { get; private set; }

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

    public bool IsActive { get; set; } = true;
    public int OwnerId { get; set; }
    public int Capacity { get; set; }

    public bool IsDeleted { get; private set; }

    public ICollection<BeachPhoto> Photos { get; set; } = new List<BeachPhoto>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<BeachEvent> Events { get; set; } = new List<BeachEvent>();

    // EF Core constructor
    public Beach() { }

    public Beach(string name, string description, string address, double lat, double lng, int ownerId)
    {
        Name = name;
        Description = description;
        Address = address;
        Latitude = lat;
        Longitude = lng;
        OwnerId = ownerId;
        IsOpen = true;
        IsActive = true;
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateRating(double newAverageRating, int count)
    {
        Rating = newAverageRating;
        ReviewCount = count;
    }

    public void ToggleOpenStatus() => IsOpen = !IsOpen;
    public void SoftDelete() => IsDeleted = true;
}

public enum OccupancyLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Full = 4
}

