using BeachRehberi.Domain.Entities;

namespace BeachRehberi.Domain.Entities;

/// <summary>
/// Beach Event entity - plaj etkinliklerini temsil eder
/// </summary>
public class BeachEvent : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach Beach { get; private set; } = null!;

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime EventDate { get; private set; }
    public DateTime? EndDate { get; private set; }

    public string? EventType { get; private set; }
    public string? Organizer { get; private set; }
    public int? MaxParticipants { get; private set; }
    public int CurrentParticipants { get; private set; }

    public bool IsActive { get; private set; }

    // EF Core constructor
    private BeachEvent() : base()
    {
        CurrentParticipants = 0;
        IsActive = true;
    }

    public BeachEvent(Guid tenantId, int beachId, string title, string description, DateTime eventDate)
        : base(tenantId)
    {
        BeachId = beachId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        EventDate = eventDate;
        CurrentParticipants = 0;
        IsActive = true;
    }

    public void UpdateEvent(string title, string description, DateTime eventDate, DateTime? endDate)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));
        EventDate = eventDate;
        EndDate = endDate;
        MarkAsUpdated();
    }

    public void SetDetails(string? eventType, string? organizer, int? maxParticipants)
    {
        EventType = eventType;
        Organizer = organizer;
        MaxParticipants = maxParticipants;
        MarkAsUpdated();
    }

    public void IncrementParticipants()
    {
        if (MaxParticipants.HasValue && CurrentParticipants >= MaxParticipants.Value)
            throw new InvalidOperationException("Event is at maximum capacity.");

        CurrentParticipants++;
        MarkAsUpdated();
    }

    public void DecrementParticipants()
    {
        if (CurrentParticipants > 0)
        {
            CurrentParticipants--;
            MarkAsUpdated();
        }
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
    }

    public bool HasCapacity()
    {
        return !MaxParticipants.HasValue || CurrentParticipants < MaxParticipants.Value;
    }

    public bool IsUpcoming()
    {
        return EventDate > DateTime.UtcNow && IsActive;
    }
}
