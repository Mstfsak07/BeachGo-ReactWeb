namespace BeachRehberi.API.Models;

public class Beach
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Website { get; set; } = string.Empty;
    public string Instagram { get; set; } = string.Empty;
    public string OpenTime { get; set; } = "08:00";
    public string CloseTime { get; set; } = "23:00";
    public bool HasEntryFee { get; set; }
    public decimal EntryFee { get; set; }
    public decimal SunbedPrice { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public string GooglePlaceId { get; set; } = string.Empty;
    public string CoverImageUrl { get; set; } = string.Empty;

    // İmkanlar
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

    // Katman 3 - Canlı veri (işletme girer)
    public int OccupancyPercent { get; set; }  // 0-100 doluluk
    public OccupancyLevel OccupancyLevel { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public bool IsOpen { get; set; } = true;
    public string TodaySpecial { get; set; } = string.Empty; // "Bugün happy hour 17:00-19:00"

    // İlişkiler
    public ICollection<BeachEvent> Events { get; set; } = new List<BeachEvent>();
    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<BeachPhoto> Photos { get; set; } = new List<BeachPhoto>();
}

public enum OccupancyLevel
{
    Empty,   // Boş
    Low,     // Az
    Medium,  // Orta
    High,    // Dolu
    Full     // Tam dolu
}