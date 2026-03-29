using System;

namespace BeachRehberi.API.DTOs.Reservation;

public class CreateReservationDto
{
    public int BeachId { get; set; }
    public DateTime ReservationDate { get; set; }
}
