namespace BeachRehberi.API.DTOs;

public class CreateReservationDto
{
    public int BeachId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public string Notes { get; set; } = string.Empty;
}

public class ReservationListItemDto
{
    public int Id { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;
    public int BeachId { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public DateTime ReservationDate { get; set; }
    public int PersonCount { get; set; }
    public int SunbedCount { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

