namespace BeachRehberi.API.DTOs.Reservation;
using System;

public class ReservationLookupDto
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string BeachName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int Pax { get; set; }
    public DateTime ReservationDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
}