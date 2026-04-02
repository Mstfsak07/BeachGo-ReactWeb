using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.DTOs.Reservation;

public class CreateGuestReservationDto
{
    [Required]
    public int BeachId { get; set; }

    [Required]
    public DateTime ReservationDate { get; set; }

    [Required]
    public string ReservationTime { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string ReservationType { get; set; } = string.Empty;

    [Required]
    [Range(1, 100)]
    public int PersonCount { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? Email { get; set; }

    [Required]
    public string VerificationId { get; set; } = string.Empty;
}

public class GuestReservationResponseDto
{
    public int ReservationId { get; set; }
    public string ConfirmationCode { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalPrice { get; set; }
}

public class SendOtpDto
{
    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;
}

public class SendOtpResponseDto
{
    public string VerificationId { get; set; } = string.Empty;
}

public class VerifyOtpDto
{
    [Required]
    public string VerificationId { get; set; } = string.Empty;

    [Required]
    [MaxLength(6)]
    public string Code { get; set; } = string.Empty;
}

public class VerifyOtpResponseDto
{
    public bool Verified { get; set; }
}
