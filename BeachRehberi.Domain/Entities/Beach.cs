using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Domain.Entities;

public class Beach : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Location { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string? District { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public string? Phone { get; private set; }
    public string? Website { get; private set; }
    public string? Instagram { get; private set; }
    public string? InstagramUsername { get; private set; }
    public string SocialContentSource { get; private set; } = "mock";
    public string? OpenTime { get; private set; }
    public string? CloseTime { get; private set; }

    public string? CoverImageUrl { get; private set; }
    public decimal PricePerPerson { get; private set; }
    public int Capacity { get; private set; }

    public bool IsActive { get; private set; } = true;
    public bool IsVerified { get; private set; } = false;

    public double AverageRating { get; private set; } = 0;
    public int TotalReviews { get; private set; } = 0;

    // Özellikler
    public bool HasParking { get; private set; }
    public bool HasRestaurant { get; private set; }
    public bool HasWaterSports { get; private set; }
    public bool HasLifeguard { get; private set; }
    public bool IsPetFriendly { get; private set; }
    public bool HasShower { get; private set; }
    public bool HasBar { get; private set; }
    public bool HasWifi { get; private set; }
    public bool HasPool { get; private set; }
    public bool IsChildFriendly { get; private set; }

    // Multi-tenant
    public int TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public ICollection<BeachPhoto> Photos { get; private set; } = new List<BeachPhoto>();
    public ICollection<Review> Reviews { get; private set; } = new List<Review>();
    public ICollection<Reservation> Reservations { get; private set; } = new List<Reservation>();
    public ICollection<BeachEvent> Events { get; private set; } = new List<BeachEvent>();

    // EF Core constructor
    private Beach() { }

    public Beach(int tenantId, string name, string description, string location,
                 string city, double latitude, double longitude,
                 decimal pricePerPerson, int capacity)
    {
        TenantId = tenantId;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Location = location ?? throw new ArgumentNullException(nameof(location));
        City = city ?? throw new ArgumentNullException(nameof(city));
        Latitude = latitude;
        Longitude = longitude;
        PricePerPerson = pricePerPerson >= 0
            ? pricePerPerson
            : throw new ArgumentException("Fiyat negatif olamaz.");
        Capacity = capacity > 0
            ? capacity
            : throw new ArgumentException("Kapasite 0'dan büyük olmalı.");
        IsActive = true;
    }

    public void UpdateInfo(string name, string description, string location, string city,
                           string? district, double latitude, double longitude,
                           decimal pricePerPerson, int capacity,
                           string? phone = null, string? website = null,
                           string? instagram = null, string? openTime = null,
                           string? closeTime = null)
    {
        Name = name ?? Name;
        Description = description ?? Description;
        Location = location ?? Location;
        City = city ?? City;
        District = district;
        Latitude = latitude;
        Longitude = longitude;
        PricePerPerson = pricePerPerson;
        Capacity = capacity;
        Phone = phone;
        Website = website;
        Instagram = instagram;
        OpenTime = openTime;
        CloseTime = closeTime;
        SetUpdated();
    }

    public void SetAmenities(bool hasParking, bool hasRestaurant, bool hasWaterSports,
                              bool hasLifeguard, bool isPetFriendly, bool hasShower = false,
                              bool hasBar = false, bool hasWifi = false,
                              bool hasPool = false, bool isChildFriendly = false)
    {
        HasParking = hasParking;
        HasRestaurant = hasRestaurant;
        HasWaterSports = hasWaterSports;
        HasLifeguard = hasLifeguard;
        IsPetFriendly = isPetFriendly;
        HasShower = hasShower;
        HasBar = hasBar;
        HasWifi = hasWifi;
        HasPool = hasPool;
        IsChildFriendly = isChildFriendly;
        SetUpdated();
    }

    public void UpdateRating(double newAverage, int totalReviews)
    {
        AverageRating = Math.Round(newAverage, 2);
        TotalReviews = totalReviews;
        SetUpdated();
    }

    public void Verify() { IsVerified = true; SetUpdated(); }
    public void Activate() { IsActive = true; SetUpdated(); }
    public void Deactivate() { IsActive = false; SetUpdated(); }
    public void SetCoverImage(string url) { CoverImageUrl = url; SetUpdated(); }
}
