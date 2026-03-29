using BeachRehberi.Domain.Entities;

namespace BeachRehberi.Domain.Entities;

/// <summary>
/// Beach Photo entity - plaj fotoğraflarını temsil eder
/// </summary>
public class BeachPhoto : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach Beach { get; private set; }

    public string ImageUrl { get; private set; }
    public string? Caption { get; private set; }
    public string? AltText { get; private set; }

    public int DisplayOrder { get; private set; }
    public bool IsCoverPhoto { get; private set; }

    public string? UploadedBy { get; private set; }
    public DateTime UploadedAt { get; private set; }

    // EF Core constructor
    private BeachPhoto() : base()
    {
        Beach = null!;
        ImageUrl = string.Empty;
        UploadedAt = DateTime.UtcNow;
        DisplayOrder = 0;
    }

    public BeachPhoto(Guid tenantId, int beachId, string imageUrl, string? caption = null, string? altText = null)
        : base(tenantId)
    {
        BeachId = beachId;
        ImageUrl = imageUrl ?? throw new ArgumentNullException(nameof(imageUrl));
        Caption = caption;
        AltText = altText ?? caption; // Alt text caption'dan türetilebilir
        UploadedAt = DateTime.UtcNow;
        DisplayOrder = 0;
        IsCoverPhoto = false;
    }

    public void UpdateCaption(string? caption, string? altText = null)
    {
        Caption = caption;
        AltText = altText ?? caption;
        MarkAsUpdated();
    }

    public void SetAsCoverPhoto()
    {
        IsCoverPhoto = true;
        MarkAsUpdated();
    }

    public void RemoveAsCoverPhoto()
    {
        IsCoverPhoto = false;
        MarkAsUpdated();
    }

    public void UpdateDisplayOrder(int displayOrder)
    {
        DisplayOrder = displayOrder;
        MarkAsUpdated();
    }

    public void SetUploadedBy(string uploadedBy)
    {
        UploadedBy = uploadedBy;
        MarkAsUpdated();
    }
}