using BeachRehberi.Domain.Entities;

namespace BeachRehberi.Domain.Entities;

/// <summary>
/// Review entity - yorum ve değerlendirme bilgilerini temsil eder
/// </summary>
public class Review : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach Beach { get; private set; } = null!;

    public int UserId { get; private set; }
    public BusinessUser User { get; private set; } = null!;

    public int Rating { get; private set; } // 1-5 arası
    public string Comment { get; private set; } = string.Empty;
    public DateTime ReviewDate { get; private set; }

    public bool IsVerified { get; private set; }

    // EF Core constructor
    private Review() : base()
    {
        ReviewDate = DateTime.UtcNow;
    }

    public Review(Guid tenantId, int beachId, int userId, int rating, string comment)
        : base(tenantId)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");

        BeachId = beachId;
        UserId = userId;
        Rating = rating;
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        ReviewDate = DateTime.UtcNow;
        IsVerified = false;
    }

    public void UpdateReview(int rating, string comment)
    {
        if (rating < 1 || rating > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");

        Rating = rating;
        Comment = comment ?? throw new ArgumentNullException(nameof(comment));
        MarkAsUpdated();
    }

    public void MarkAsVerified()
    {
        IsVerified = true;
        MarkAsUpdated();
    }

    public bool IsEditable()
    {
        // Yorumlar oluşturulduktan sonra 24 saat içinde düzenlenebilir
        return (DateTime.UtcNow - CreatedAt).TotalHours <= 24;
    }
}
