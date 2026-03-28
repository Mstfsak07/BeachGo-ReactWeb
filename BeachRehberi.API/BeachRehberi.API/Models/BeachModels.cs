namespace BeachRehberi.API.Models;

public class BeachFilter
{
    public double? MinRating { get; set; }
    public bool? HasBar { get; set; }
    public bool? HasWaterSports { get; set; }
    public bool? IsChildFriendly { get; set; }
    public bool? HasPool { get; set; }
    public bool? FreeEntry { get; set; }
    public bool? IsOpen { get; set; }
    public string? SortBy { get; set; }
    public double? UserLat { get; set; }
    public double? UserLng { get; set; }
}
