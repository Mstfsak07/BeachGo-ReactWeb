using BeachRehberi.Domain.Common;

namespace BeachRehberi.Domain.Entities;

public class BeachPhoto : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }
    public string Url { get; private set; } = string.Empty;
    public string Caption { get; private set; } = string.Empty;
    public bool IsCover { get; private set; }
    public int Order { get; private set; }

    // EF Core constructor
    private BeachPhoto() { }

    public BeachPhoto(int beachId, string url, string caption, bool isCover = false)
    {
        BeachId = beachId;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Caption = caption ?? string.Empty;
        IsCover = isCover;
    }

    public void SetCover(bool isCover)
    {
        IsCover = isCover;
        SetUpdated();
    }

    public void UpdateOrder(int order)
    {
        Order = order;
        SetUpdated();
    }

    public void UpdateCaption(string caption)
    {
        Caption = caption ?? string.Empty;
        SetUpdated();
    }
}
