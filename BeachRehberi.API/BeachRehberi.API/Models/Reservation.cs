using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.Models;

public class Reservation
{
    public int Id { get; private set; }

    [Required]
    public string ConfirmationCode { get; private set; } = string.Empty;

    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }

    public int? UserId { get; private set; }
    public BusinessUser? User { get; private set; }

    [Required]
    public string UserName { get; private set; } = string.Empty;

    [Required]
    public string UserPhone { get; private set; } = string.Empty;

    [Required]
    public string UserEmail { get; private set; } = string.Empty;

    public DateTime ReservationDate { get; private set; }
    public int PersonCount { get; private set; }
    public int SunbedCount { get; private set; }
    public string Notes { get; private set; } = string.Empty;

    public decimal TotalPrice { get; private set; }
    public ReservationStatus Status { get; private set; }

    public string? BusinessComment { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    // EF Core constructor
    private Reservation() { }

    public Reservation(int beachId, string userName, string userPhone, string userEmail, 
                       DateTime date, int personCount, int sunbedCount, string notes)
    {
        if (date < DateTime.UtcNow.Date) throw new DomainException("Rezervasyon tarihi geçmiş olamaz.");
        if (personCount <= 0) throw new DomainException("Kişi sayısı 1den az olamaz.");

        BeachId = beachId;
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        UserPhone = userPhone ?? throw new ArgumentNullException(nameof(userPhone));
        UserEmail = userEmail ?? throw new ArgumentNullException(nameof(userEmail));
        ReservationDate = date;
        PersonCount = personCount;
        SunbedCount = sunbedCount;
        Notes = notes;
        Status = ReservationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void SetConfirmationCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new DomainException("Onay kodu boş olamaz.");
        ConfirmationCode = code;
    }

    public void CalculatePrice(decimal entryFee, decimal sunbedPrice)
    {
        TotalPrice = (entryFee + sunbedPrice) * PersonCount;
    }

    public void AssignUser(int? userId)
    {
        UserId = userId;
    }

    public void Confirm() => Status = ReservationStatus.Confirmed;
    
    public void Cancel()
    {
        if (Status == ReservationStatus.Completed || Status == ReservationStatus.NoShow)
            throw new DomainException("Tamamlanmış veya gelinmemiş rezervasyonlar iptal edilemez.");
        
        Status = ReservationStatus.Cancelled;
    }

    public void Approve(string? comment = null)
    {
        Status = ReservationStatus.Approved;
        BusinessComment = comment;
    }

    public void Reject(string? comment)
    {
        Status = ReservationStatus.Rejected;
        BusinessComment = comment;
    }

    public void MarkAsCompleted() => Status = ReservationStatus.Completed;
    public void MarkAsNoShow() => Status = ReservationStatus.NoShow;
    public void SoftDelete() => IsDeleted = true;
}

public enum ReservationStatus
{
    Pending = 0,
    Confirmed = 1,
    Cancelled = 2,
    Approved = 3,
    Rejected = 4,
    NoShow = 5,
    Completed = 6
}

