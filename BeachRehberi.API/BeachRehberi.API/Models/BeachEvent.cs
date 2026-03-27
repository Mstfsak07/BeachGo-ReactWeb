using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class BeachEvent
{
    public int Id { get; set; }

    public int BeachId { get; set; }
    public Beach? Beach { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;

    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public decimal TicketPrice { get; set; }
    public int Capacity { get; set; }
    public int AvailableSpots { get; set; }

    public bool IsAgeRestricted { get; set; }
    public int MinAge { get; set; }

    public string ImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool IsDeleted { get; set; } = false; // Soft Delete
}
