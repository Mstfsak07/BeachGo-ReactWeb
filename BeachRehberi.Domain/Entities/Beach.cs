using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Domain.Entities;

public class Beach : BaseEntity
{
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

    // Özellikler
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

    public bool IsActive { get; private set; } = true;
    public int Capacity { get; private set; }

    // Multi-tenant
    public int TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public int OwnerId { get; private set; }
    public User? Owner { get; private set; }

    public ICollection<BeachPhoto> Photos { get; private set; } = new List<BeachPhoto>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public ICollection<BeachEvent> Events { get; private set; } = new List<BeachEvent>();

    // EF Core constructor
    private Beach() { }

    public Beach(string name, string description, string address,
                 double lat, double lng, int ownerId, int tenantId, int capacity = 100)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Address = address ?? string.Empty;
        Latitude = lat;
        Longitude = lng;
        OwnerId = ownerId;
        TenantId = tenantId;
        Capacity = capacity;
        IsOpen = true;
        IsActive = true;
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateInfo(string name, string description, string address,
                           string phone, string website, string instagram,
                           string openTime, string closeTime, string todaySpecial)
    {
        Name = name ?? Name;
        Description = description ?? Description;
        Address = address ?? Address;
        Phone = phone ?? Phone;
        Website = website ?? Website;
        Instagram = instagram ?? Instagram;
        OpenTime = openTime ?? OpenTime;
        CloseTime = closeTime ?? CloseTime;
        TodaySpecial = todaySpecial ?? TodaySpecial;
        LastUpdated = DateTime.UtcNow;
        SetUpdated();
    }

    public void UpdatePricing(bool hasEntryFee, decimal entryFee, decimal sunbedPrice)
    {
        HasEntryFee = hasEntryFee;
        EntryFee = entryFee;
        SunbedPrice = sunbedPrice;
        SetUpdated();
    }

    public void UpdateAmenities(bool hasSunbeds, bool hasShower, bool hasParking,
                                bool hasRestaurant, bool hasBar, bool hasAlcohol,
                                bool isChildFriendly, bool hasWaterSports, bool hasWifi,
                                bool hasPool, bool hasDJ, bool hasAccessibility)
    {
        HasSunbeds = hasSunbeds;
        HasShower = hasShower;
        HasParking = hasParking;
        HasRestaurant = hasRestaurant;
        HasBar = hasBar;
        HasAlcohol = hasAlcohol;
        IsChildFriendly = isChildFriendly;
        HasWaterSports = hasWaterSports;
        HasWifi = hasWifi;
        HasPool = hasPool;
        HasDJ = hasDJ;
        HasAccessibility = hasAccessibility;
        SetUpdated();
    }

    public void UpdateLocation(double lat, double lng)
    {
        Latitude = lat;
        Longitude = lng;
        SetUpdated();
    }

    public void UpdateOccupancy(int percent)
    {
        OccupancyPercent = Math.Clamp(percent, 0, 100);
        OccupancyLevel = OccupancyPercent switch
        {
            <= 30 => OccupancyLevel.Low,
            <= 60 => OccupancyLevel.Medium,
            <= 90 => OccupancyLevel.High,
            _ => OccupancyLevel.Full
        };
        LastUpdated = DateTime.UtcNow;
    }

    public void UpdateRating(double newAverageRating, int count)
    {
        Rating = Math.Round(newAverageRating, 2);
        ReviewCount = count;
        SetUpdated();
    }

    public void SetCoverImage(string url)
    {
        CoverImageUrl = url;
        SetUpdated();
    }

    public void ToggleOpenStatus()
    {
        IsOpen = !IsOpen;
        LastUpdated = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
