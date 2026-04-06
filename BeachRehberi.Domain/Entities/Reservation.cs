using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Enums;

namespace BeachRehberi.Domain.Entities;

public class Reservation : BaseEntity
{
    public int UserId { get; private set; }
    public User? User { get; private set; }

    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }

    public int TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public DateTime ReservationDate { get; private set; }
    public int GuestCount { get; private set; } = 1;
    public string? Notes { get; private set; }

    public ReservationStatus Status { get; private set; } = ReservationStatus.Pending;
    public string? StatusNote { get; private set; }

    public decimal TotalPrice { get; private set; }
    public bool IsPaid { get; private set; } = false;
    public string? PaymentIntentId { get; private set; }

    // EF Core constructor
    private Reservation() { }

    public Reservation(int userId, int beachId, int tenantId,
                       DateTime reservationDate, int guestCount, decimal totalPrice, string? notes = null)
    {
        UserId = userId;
        BeachId = beachId;
        TenantId = tenantId;
        ReservationDate = reservationDate;
        GuestCount = guestCount > 0 ? guestCount : throw new ArgumentException("Misafir sayısı 0'dan büyük olmalı.");
        TotalPrice = totalPrice;
        Notes = notes;
    }

    public void Approve(string? comment = null)
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen rezervasyonlar onaylanabilir.");

        Status = ReservationStatus.Approved;
        StatusNote = comment;
        SetUpdated();
    }

    public void Reject(string? comment = null)
    {
        if (Status != ReservationStatus.Pending)
            throw new InvalidOperationException("Sadece bekleyen rezervasyonlar reddedilebilir.");

        Status = ReservationStatus.Rejected;
        StatusNote = comment;
        SetUpdated();
    }

    public void Cancel(string? reason = null)
    {
        if (Status == ReservationStatus.Completed || Status == ReservationStatus.NoShow)
            throw new InvalidOperationException("Tamamlanmış veya gelmemiş rezervasyonlar iptal edilemez.");

        Status = ReservationStatus.Cancelled;
        StatusNote = reason;
        SetUpdated();
    }

    public void MarkAsNoShow()
    {
        if (Status != ReservationStatus.Approved)
            throw new InvalidOperationException("Sadece onaylanmış rezervasyonlar 'gelmedi' olarak işaretlenebilir.");

        Status = ReservationStatus.NoShow;
        SetUpdated();
    }

    public void MarkAsCompleted()
    {
        if (Status != ReservationStatus.Approved)
            throw new InvalidOperationException("Sadece onaylanmış rezervasyonlar tamamlanabilir.");

        Status = ReservationStatus.Completed;
        SetUpdated();
    }

    public void MarkAsPaid(string paymentIntentId)
    {
        IsPaid = true;
        PaymentIntentId = paymentIntentId;
        SetUpdated();
    }
}
