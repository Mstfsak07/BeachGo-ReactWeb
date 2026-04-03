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

    public bool IsGuest { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;

    public bool IsGuestReservation { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Detay ekranı için yeni eklenen alanlar
    public string BeachName { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
    
    // SMS / Verification
    public bool SmsSent { get; set; }
    public bool SmsVerified { get; set; }
    public DateTime? SmsLastSentTime { get; set; }
    
    // Timeline
    public DateTime? PaymentCreatedAt { get; set; }
    public DateTime? CancelledAt { get; set; }
}

