using System.ComponentModel.DataAnnotations;
namespace BeachRehberi.API.Models;

public enum ReservationStatus
{
    Pending,
    Approved,
    Confirmed,
    Rejected,
    Cancelled
}

public class Reservation {
    public int Id { get; set; }
    public int BeachId { get; set; }
    public Beach? Beach { get; set; }
    public int? UserId { get; set; } 
    public required string UserName { get; set; }
    public required string UserPhone { get; set; }
    public int PersonCount { get; set; }
    public string? ConfirmationCode { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime ReservationDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public string? BusinessComment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}