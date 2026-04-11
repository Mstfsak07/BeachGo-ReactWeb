using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.DTOs;

public class AdminStatsDto
{
    public int TotalBeaches { get; set; }
    public int TotalUsers { get; set; }
    public int TotalReservations { get; set; }
    public int PendingBeaches { get; set; }
    public decimal Revenue { get; set; }
}

public class AdminBeachListItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Capacity { get; set; }
    public bool IsActive { get; set; }
    public double Rating { get; set; }
    public string? InstagramUsername { get; set; }
    public string? SocialContentSource { get; set; }
}

public class AdminUserListItemDto
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? BusinessName { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class AdminReservationListItemDto
{
    public int Id { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public TimeSpan? ReservationTime { get; set; }
    public int PersonCount { get; set; }
    public string ReservationType { get; set; } = string.Empty;
    public ReservationStatus Status { get; set; }
    public bool IsGuest { get; set; }
    public string? ConfirmationCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class AdminBeachImportResultDto
{
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
}
