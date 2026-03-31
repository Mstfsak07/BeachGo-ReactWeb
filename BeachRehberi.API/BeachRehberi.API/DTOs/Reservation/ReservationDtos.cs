using System;

namespace BeachRehberi.API.DTOs.Reservation;

public class CreateReservationDto
{
    public int BeachId { get; set; }
    public DateTime ReservationDate { get; set; }
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public string? Notes { get; set; }
    public decimal TotalPrice { get; set; }
}

public class ReservationResponseDto
{
    public int Id { get; set; }
    public DateTime ReservationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public int BeachId { get; set; }
    public string BeachName { get; set; } = string.Empty;
}
