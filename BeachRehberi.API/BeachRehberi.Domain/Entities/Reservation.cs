using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Domain.Entities;

/// <summary>
/// Reservation entity - rezervasyon bilgilerini temsil eder
/// </summary>
public class Reservation : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach Beach { get; private set; } = null!;

    public int UserId { get; private set; }
    public BusinessUser User { get; private set; } = null!;

    public DateTime ReservationDate { get; private set; }
    public ReservationStatus Status { get; private set; }

    public int? GuestCount { get; private set; }
    public string? Notes { get; private set; }

    // EF Core constructor
    private Reservation() : base()
    {
        Status = ReservationStatus.Pending;
    }

    public Reservation(Guid tenantId, int beachId, int userId, DateTime reservationDate)
        : base(tenantId)
    {
        BeachId = beachId;
        UserId = userId;
        ReservationDate = reservationDate.Date; // Sadece tarih kısmı
        Status = ReservationStatus.Pending;
    }

    public void Confirm()
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Only pending reservations can be confirmed.");

        Status = ReservationStatus.Confirmed;
        MarkAsUpdated();
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Completed)
            throw new InvalidOperationException("Completed reservations cannot be cancelled.");

        Status = ReservationStatus.Cancelled;
        MarkAsUpdated();
    }

    public void Complete()
    {
        if (Status != ReservationStatus.Confirmed)
            throw new InvalidOperationException("Only confirmed reservations can be completed.");

        Status = ReservationStatus.Completed;
        MarkAsUpdated();
    }

    public void UpdateDetails(int? guestCount, string? notes)
    {
        GuestCount = guestCount;
        Notes = notes;
        MarkAsUpdated();
    }

    public bool IsActive()
    {
        return Status == ReservationStatus.Pending || Status == ReservationStatus.Confirmed;
    }

    public bool CanBeModified()
    {
        return Status == ReservationStatus.Pending && ReservationDate > DateTime.UtcNow;
    }
}
