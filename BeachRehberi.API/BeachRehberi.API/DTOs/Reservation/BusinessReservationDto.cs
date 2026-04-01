namespace BeachRehberi.API.DTOs.Reservation;

public class BusinessReservationDto
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
