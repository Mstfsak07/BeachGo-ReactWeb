using System;
using System.ComponentModel.DataAnnotations;
using BeachRehberi.API.Exceptions;

namespace BeachRehberi.API.Models;

public class BeachEvent
{
    public int Id { get; private set; }

    public int BeachId { get; private set; }        
    public Beach? Beach { get; private set; }       

    [Required]
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;

    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

    public decimal TicketPrice { get; private set; }
    public int Capacity { get; private set; }
    public int AvailableSpots { get; private set; }

    public bool IsAgeRestricted { get; private set; }
    public int MinAge { get; private set; }

    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    // EF Core constructor
    private BeachEvent() { }

    public BeachEvent(int beachId, string title, string description, DateTime start, DateTime end, int capacity)
    {
        if (start < DateTime.UtcNow) throw new DomainException("Etkinlik başlangıç tarihi geçmiş olamaz.");
        if (end <= start) throw new DomainException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");
        if (capacity <= 0) throw new DomainException("Kapasite 0dan büyük olmalıdır.");

        BeachId = beachId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description;
        StartDate = start;
        EndDate = end;
        Capacity = capacity;
        AvailableSpots = capacity;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
    }

    public void UpdateSpots(int bookedCount)
    {
        var newSpots = AvailableSpots - bookedCount;
        if (newSpots < 0) throw new DomainException("Yetersiz kapasite.");
        AvailableSpots = newSpots;
    }

    public void Cancel() => IsActive = false;
    public void SoftDelete() => IsDeleted = true;
}

