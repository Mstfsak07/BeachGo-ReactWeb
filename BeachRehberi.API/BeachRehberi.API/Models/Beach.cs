using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class Beach
{
    public int Id { get; private set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;
    public string Website { get; private set; } = string.Empty;
    public string Instagram { get; private set; } = string.Empty;
    public string OpenTime { get; private set; } = string.Empty;
    public string CloseTime { get; private set; } = string.Empty;

    public bool HasEntryFee { get; private set; }
    public decimal EntryFee { get; private set; }
    public decimal SunbedPrice { get; private set; }

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public double Rating { get; private set; }
    public int ReviewCount { get; private set; }

    public string GooglePlaceId { get; private set; } = string.Empty;
    public string CoverImageUrl { get; private set; } = string.Empty;

    public bool HasSunbeds { get; private set; }
    public bool HasShower { get; private set; }
    public bool HasParking { get; private set; }
    public bool HasRestaurant { get; private set; }
    public bool HasBar { get; private set; }
    public bool HasAlcohol { get; private set; }
    public bool IsChildFriendly { get; private set; }
    public bool HasWaterSports { get; private set; }
    public bool HasWifi { get; private set; }
    public bool HasPool { get; private set; }
    public bool HasDJ { get; private set; }
    public bool HasAccessibility { get; private set; }

    public int OccupancyPercent { get; private set; }
    public OccupancyLevel OccupancyLevel { get; private set; }
    public DateTime LastUpdated { get; private set; } = DateTime.UtcNow;
    public bool IsOpen { get; private set; }
    public string TodaySpecial { get; private set; } = string.Empty;

    public bool IsDeleted { get; private set; }

    public ICollection<BeachPhoto> Photos { get; private set; } = new List<BeachPhoto>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public ICollection<BeachEvent> Events { get; private set; } = new List<BeachEvent>();

    // EF Core constructor
    private Beach() { }

    public Beach(string name, string description, string address, double lat, double lng)
    {
        UpdateDetails(name, description, address);
        Latitude = lat;
        Longitude = lng;
        IsOpen = true;
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, string address)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new DomainException("Plaj adı boş olamaz.");
        Name = name;
        Description = description;
        Address = address;
    }

    public void UpdateFees(bool hasEntryFee, decimal entryFee, decimal sunbedPrice)
    {
        HasEntryFee = hasEntryFee;
        EntryFee = entryFee;
        SunbedPrice = sunbedPrice;
    }

    public void UpdateOccupancy(int percent, OccupancyLevel level)
    {
        if (percent < 0 || percent > 100) throw new DomainException("Doluluk oranı 0-100 arasında olmalıdır.");
        OccupancyPercent = percent;
        OccupancyLevel = level;
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateRating(double newAverageRating, int count)
    {
        Rating = newAverageRating;
        ReviewCount = count;
    }

    public void ToggleOpenStatus() => IsOpen = !IsOpen;
    public void SetTodaySpecial(string special) => TodaySpecial = special;
    public void SoftDelete() => IsDeleted = true;
}

public enum OccupancyLevel
{
    Low = 1,
    Medium = 2,
    High = 3,
    Full = 4
}

