namespace BeachRehberi.API.Models;

public class BeachPhoto
{
    public int Id { get; private set; }
    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string Caption { get; private set; } = string.Empty;        
    public bool IsCover { get; private set; }
    public int Order { get; private set; }
    public DateTime UploadedAt { get; private set; }

    // EF Core constructor
    private BeachPhoto() { }

    public BeachPhoto(int beachId, string url, string caption, bool isCover = false)
    {
        BeachId = beachId;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Caption = caption;
        IsCover = isCover;
        UploadedAt = DateTime.UtcNow;
    }

    public void SetCover(bool isCover) => IsCover = isCover;
    public void UpdateOrder(int order) => Order = order;
}

