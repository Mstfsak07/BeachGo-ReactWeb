using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class Review
{
    public int Id { get; set; }

    public int BeachId { get; set; }
    public Beach? Beach { get; set; }

    public int? UserId { get; set; }
    public BusinessUser? User { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string UserPhone { get; set; } = string.Empty;

    public int Rating { get; set; }

    [Required]
    [MaxLength(500)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; }
    public string Source { get; set; } = "App";
    
    public bool IsDeleted { get; set; } = false; // Soft Delete
}
