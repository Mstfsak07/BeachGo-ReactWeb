using BeachRehberi.Domain.Common;

namespace BeachRehberi.Domain.Entities;

public class BeachEvent : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }

    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public decimal? TicketPrice { get; private set; }
    public int? MaxAttendees { get; private set; }
    public bool IsActive { get; private set; } = true;

    // EF Core constructor
    private BeachEvent() { }

    public BeachEvent(int beachId, string title, string description,
                      DateTime startDate, DateTime endDate,
                      decimal? ticketPrice = null, int? maxAttendees = null)
    {
        BeachId = beachId;
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Description = description ?? throw new ArgumentNullException(nameof(description));

        if (endDate <= startDate)
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        StartDate = startDate;
        EndDate = endDate;
        TicketPrice = ticketPrice;
        MaxAttendees = maxAttendees;
    }

    public void Update(string title, string description, DateTime startDate,
                       DateTime endDate, decimal? ticketPrice, int? maxAttendees)
    {
        Title = title ?? Title;
        Description = description ?? Description;

        if (endDate <= startDate)
            throw new ArgumentException("Bitiş tarihi başlangıç tarihinden sonra olmalıdır.");

        StartDate = startDate;
        EndDate = endDate;
        TicketPrice = ticketPrice;
        MaxAttendees = maxAttendees;
        SetUpdated();
    }

    public void Activate() { IsActive = true; SetUpdated(); }
    public void Deactivate() { IsActive = false; SetUpdated(); }
}
