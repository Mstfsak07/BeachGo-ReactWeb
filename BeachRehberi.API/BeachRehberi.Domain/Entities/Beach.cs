using System.ComponentModel.DataAnnotations;
using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Enums;
using BeachRehberi.Domain.ValueObjects;

namespace BeachRehberi.Domain.Entities;

/// <summary>
/// Beach entity - plaj bilgilerini temsil eder
/// </summary>
public class Beach : BaseEntity
{
    [Required]
    [MaxLength(200)]
    public string Name { get; private set; }

    public string Description { get; private set; }
    public string Address { get; private set; }
    public string Phone { get; private set; }
    public string Website { get; private set; }
    public string Instagram { get; private set; }
    public string OpenTime { get; private set; }
    public string CloseTime { get; private set; }

    public bool HasEntryFee { get; private set; }
    public Money EntryFee { get; private set; }
    public Money SunbedPrice { get; private set; }

    public double Latitude { get; private set; }
    public double Longitude { get; private set; }

    public double Rating { get; private set; }
    public int ReviewCount { get; private set; }

    public string GooglePlaceId { get; private set; }
    public string CoverImageUrl { get; private set; }

    // Facilities
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

    // Navigation properties
    private readonly List<Reservation> _reservations = new();
    public IReadOnlyCollection<Reservation> Reservations => _reservations.AsReadOnly();

    private readonly List<Review> _reviews = new();
    public IReadOnlyCollection<Review> Reviews => _reviews.AsReadOnly();

    private readonly List<BeachEvent> _events = new();
    public IReadOnlyCollection<BeachEvent> Events => _events.AsReadOnly();

    private readonly List<BeachPhoto> _photos = new();
    public IReadOnlyCollection<BeachPhoto> Photos => _photos.AsReadOnly();

    // EF Core constructor
    private Beach() : base()
    {
        Name = string.Empty;
        EntryFee = Money.Zero();
        SunbedPrice = Money.Zero();
    }

    public Beach(Guid tenantId, string name, string address, double latitude, double longitude)
        : base(tenantId)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Latitude = latitude;
        Longitude = longitude;
        EntryFee = Money.Zero();
        SunbedPrice = Money.Zero();
        Rating = 0;
        ReviewCount = 0;
        OccupancyPercent = 0;
        OccupancyLevel = OccupancyLevel.Empty;
    }

    public void UpdateBasicInfo(string name, string description, string address, string phone)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Description = description ?? string.Empty;
        Address = address ?? throw new ArgumentNullException(nameof(address));
        Phone = phone ?? string.Empty;
        MarkAsUpdated();
    }

    public void UpdateContactInfo(string website, string instagram)
    {
        Website = website ?? string.Empty;
        Instagram = instagram ?? string.Empty;
        MarkAsUpdated();
    }

    public void UpdateOperatingHours(string openTime, string closeTime)
    {
        OpenTime = openTime ?? string.Empty;
        CloseTime = closeTime ?? string.Empty;
        MarkAsUpdated();
    }

    public void UpdatePricing(bool hasEntryFee, Money entryFee, Money sunbedPrice)
    {
        HasEntryFee = hasEntryFee;
        EntryFee = entryFee ?? Money.Zero();
        SunbedPrice = sunbedPrice ?? Money.Zero();
        MarkAsUpdated();
    }

    public void UpdateLocation(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
        MarkAsUpdated();
    }

    public void UpdateFacilities(
        bool hasSunbeds, bool hasShower, bool hasParking, bool hasRestaurant,
        bool hasBar, bool hasAlcohol, bool isChildFriendly, bool hasWaterSports,
        bool hasWifi, bool hasPool, bool hasDJ, bool hasAccessibility)
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
        MarkAsUpdated();
    }

    public void UpdateRating(double newRating, int newReviewCount)
    {
        Rating = newRating;
        ReviewCount = newReviewCount;
        MarkAsUpdated();
    }

    public void UpdateOccupancy(int occupancyPercent)
    {
        OccupancyPercent = Math.Clamp(occupancyPercent, 0, 100);
        OccupancyLevel = occupancyPercent switch
        {
            <= 20 => OccupancyLevel.Empty,
            <= 40 => OccupancyLevel.Low,
            <= 60 => OccupancyLevel.Medium,
            <= 80 => OccupancyLevel.High,
            _ => OccupancyLevel.Full
        };
        MarkAsUpdated();
    }

    public void SetGooglePlaceId(string googlePlaceId)
    {
        GooglePlaceId = googlePlaceId ?? string.Empty;
        MarkAsUpdated();
    }

    public void SetCoverImage(string imageUrl)
    {
        CoverImageUrl = imageUrl ?? string.Empty;
        MarkAsUpdated();
    }
}