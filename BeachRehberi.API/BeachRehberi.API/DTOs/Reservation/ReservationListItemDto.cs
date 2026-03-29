using System;

namespace BeachRehberi.API.DTOs.Reservation;

public class ReservationListItemDto
{
    public int Id { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
}
