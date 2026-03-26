using System;

namespace BeachRehberi.API.Models
{
    public enum ReservationStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled
    }

    public class Reservation
    {
        public int Id { get; set; }
        public int BeachId { get; set; }
        public Beach Beach { get; set; } = null!;
        public string CustomerName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public int Pax { get; set; } // KiĹąi sayÄąsÄą
        public string Code { get; set; } = string.Empty; // Sorgu kodu
        public DateTime ReservationDate { get; set; } // Hangi gĂźn iĂ§in?
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
        public string? BusinessComment { get; set; } // Ä°Ĺąletmenin notu (Red sebebi vb.)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
