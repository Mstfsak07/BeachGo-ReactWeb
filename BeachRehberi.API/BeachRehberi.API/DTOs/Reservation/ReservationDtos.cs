using System;
using System.Collections.Generic;

namespace BeachRehberi.API.DTOs.Reservation;

public class CreateReservationDto
{
    public int BeachId { get; set; }
    public DateTime ReservationDate { get; set; }
    public string ReservationTime { get; set; } = string.Empty;
    public string ReservationType { get; set; } = string.Empty;
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public List<string> SelectedSeats { get; set; } = new();
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
}

public class ReservationResponseDto
{
    public int Id { get; set; }
    public DateTime ReservationDate { get; set; }
    public string ReservationTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int BeachId { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public List<string> SelectedSeats { get; set; } = new();
}

public class ReservationSeatAvailabilityDto
{
    public List<string> ReservedSeats { get; set; } = new();
}
