using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.Models;

public class Reservation
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual BusinessUser User { get; set; } = null!;

    [Required]
    public int BeachId { get; set; }
    
    [ForeignKey("BeachId")]
    public virtual Beach Beach { get; set; } = null!;

    [Required]
    public DateTime ReservationDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsDeleted { get; set; } = false;

    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

    public int PersonCount { get; set; }

    public int SunbedCount { get; set; }

    public string? Notes { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalPrice { get; set; }

    // Reservation Business Logic Methods
    public void Approve(string? comment = null) 
    {
        Status = ReservationStatus.Approved;
    }

    public void Reject(string? comment = null)
    {
        Status = ReservationStatus.Rejected;
    }

    public void Cancel()
    {
        Status = ReservationStatus.Cancelled;
    }

    public void MarkAsNoShow()
    {
        Status = ReservationStatus.NoShow;
    }

    public void MarkAsCompleted()
    {
        Status = ReservationStatus.Completed;
    }
}
