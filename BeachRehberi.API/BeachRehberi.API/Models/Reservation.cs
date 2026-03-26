namespace BeachRehberi.API.Models;

public class Reservation
{
    public int Id { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;
    public int BeachId { get; set; }
    public Beach Beach { get; set; } = null!;
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public string Notes { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public enum ReservationStatus
{
    Pending,
    Confirmed,
    Cancelled
}