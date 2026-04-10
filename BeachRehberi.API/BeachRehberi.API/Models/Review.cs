using System;
using System.ComponentModel.DataAnnotations;
using BeachRehberi.API.Exceptions;

namespace BeachRehberi.API.Models;

public class Review
{
    public int Id { get; private set; }

    public int BeachId { get; private set; }        
    public Beach? Beach { get; private set; }       

    public int? UserId { get; private set; }
    public BusinessUser? User { get; private set; }

    [Required]
    [MaxLength(100)]
    public string UserName { get; private set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string UserPhone { get; private set; } = string.Empty;

    public int Rating { get; private set; }

    [Required]
    [MaxLength(500)]
    public string Comment { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }
    public bool IsApproved { get; private set; }
    public string Source { get; private set; } = "App";

    public bool IsDeleted { get; private set; }

    // EF Core constructor
    private Review() { }

    public Review(int beachId, int? userId, string userName, string userPhone, int rating, string comment)
    {
        if (rating < 1 || rating > 5) throw new DomainException("Puan 1-5 arasında olmalıdır.");
        
        BeachId = beachId;
        UserId = userId;
        UserName = userName ?? throw new ArgumentNullException(nameof(userName));
        UserPhone = userPhone ?? throw new ArgumentNullException(nameof(userPhone));
        Rating = rating;
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        CreatedAt = DateTime.UtcNow;
        IsApproved = true; // Auto-approve for now
        Source = "App";
    }

    public void Approve() => IsApproved = true;
    public void Disapprove() => IsApproved = false;
    public void SoftDelete() => IsDeleted = true;
}

