using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Application.DTOs;

/// <summary>
/// Reservation data transfer object
/// </summary>
public class ReservationDto
{
    public int Id { get; set; }
    public int BeachId { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;

    public DateTime ReservationDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public ReservationStatus Status { get; set; }

    public int? GuestCount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Create reservation DTO
/// </summary>
public class CreateReservationDto
{
    public int BeachId { get; set; }
    public DateTime ReservationDate { get; set; }
    public int? GuestCount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Update reservation DTO
/// </summary>
public class UpdateReservationDto
{
    public int? GuestCount { get; set; }
    public string? Notes { get; set; }
}

/// <summary>
/// Reservation response DTO
/// </summary>
public class ReservationResponseDto
{
    public int Id { get; set; }
    public int BeachId { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public ReservationStatus Status { get; set; }
    public string StatusText { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool CanBeModified { get; set; }
    public bool CanBeCancelled { get; set; }
}