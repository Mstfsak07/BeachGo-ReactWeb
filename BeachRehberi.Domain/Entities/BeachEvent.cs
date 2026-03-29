using BeachRehberi.Domain.Common;

namespace BeachRehberi.Domain.Entities;

public class BeachEvent : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime EventDate { get; private set; }
    public string ImageUrl { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private BeachEvent() { }

    public BeachEvent(int beachId, string title, string description, DateTime eventDate, string imageUrl = "")
    {
        BeachId = beachId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? string.Empty;
        EventDate = eventDate;
        ImageUrl = imageUrl ?? string.Empty;
    }

    public void Update(string title, string description, DateTime eventDate, string imageUrl)
    {
        Title = title ?? Title;
        Description = description ?? Description;
        EventDate = eventDate;
        ImageUrl = imageUrl ?? ImageUrl;
        SetUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdated();
    }
}
