using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models
{
    public enum ReservationStatus
    {
        Pending,
        Approved,
        Confirmed,
        Rejected,
        Cancelled
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int BeachId { get; set; }
        public Beach? Beach { get; set; }

        [Required(ErrorMessage = "Kullanıcı adı zorunludur.")]
        [StringLength(100, MinimumLength = 2)]
        public required string UserName { get; set; }

        [Required(ErrorMessage = "Telefon numarası zorunludur.")]
        [RegularExpression(@"^\+?(\d{10,12})$", ErrorMessage = "Geçerli bir telefon numarası giriniz.")]
        public required string UserPhone { get; set; }

        [Range(1, 20)]
        public int PersonCount { get; set; }

        public string? ConfirmationCode { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime ReservationDate { get; set; }
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public string? BusinessComment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}