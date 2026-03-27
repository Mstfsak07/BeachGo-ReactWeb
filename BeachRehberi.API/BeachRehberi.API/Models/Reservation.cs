using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class Reservation
{
    public int Id { get; set; }

    [Required]
    public string ConfirmationCode { get; set; } = string.Empty;

    public int BeachId { get; set; }
    public Beach? Beach { get; set; }

    public int? UserId { get; set; }
    public BusinessUser? User { get; set; }

    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string UserPhone { get; set; } = string.Empty;

    [Required]
    public string UserEmail { get; set; } = string.Empty;

    public DateTime ReservationDate { get; set; }
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public string Notes { get; set; } = string.Empty;

    public decimal TotalPrice { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public string? BusinessComment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false; // Soft Delete
}

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Approved = 3,
    Rejected = 4,
    NoShow = 5,
    Completed = 6
}
