using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class VerificationCode
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [MaxLength(6)]
    public string Code { get; set; } = string.Empty;

    public DateTime ExpireDate { get; set; }

    public bool IsUsed { get; set; } = false;

    public int Attempts { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsExpired => DateTime.UtcNow > ExpireDate;
    public bool IsValid => !IsUsed && !IsExpired && Attempts < 5;
}
