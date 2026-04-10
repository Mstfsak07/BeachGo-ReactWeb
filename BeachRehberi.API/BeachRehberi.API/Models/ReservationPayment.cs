using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.Models;

public class ReservationPayment
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int ReservationId { get; set; }

    [ForeignKey("ReservationId")]
    public virtual Reservation Reservation { get; set; } = null!;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

    [MaxLength(100)]
    public string? TransactionId { get; set; }

    [MaxLength(50)]
    public string PaymentMethod { get; set; } = "Stripe";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAt { get; set; }
}
