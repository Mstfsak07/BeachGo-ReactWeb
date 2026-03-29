using BeachRehberi.Domain.Common;

namespace BeachRehberi.Domain.Entities;

public class Review : BaseEntity
{
    public int UserId { get; private set; }
    public User? User { get; private set; }

    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }

    public int Rating { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public bool IsApproved { get; private set; } = false;

    // EF Core constructor
    private Review() { }

    public Review(int userId, int beachId, int rating, string comment)
    {
        UserId = userId;
        BeachId = beachId;
        Rating = rating is >= 1 and <= 5
            ? rating
            : throw new ArgumentException("Puan 1 ile 5 arasında olmalıdır.");
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
    }

    public void Approve() { IsApproved = true; SetUpdated(); }
    public void Reject() { IsApproved = false; SetUpdated(); }

    public void Update(int rating, string comment)
    {
        Rating = rating is >= 1 and <= 5
            ? rating
            : throw new ArgumentException("Puan 1 ile 5 arasında olmalıdır.");
        Comment = comment ?? Comment;
        IsApproved = false; // Güncelleme sonrası tekrar onay beklesin
        SetUpdated();
    }
}
