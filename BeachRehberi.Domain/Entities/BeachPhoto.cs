using BeachRehberi.Domain.Common;

namespace BeachRehberi.Domain.Entities;

public class BeachPhoto : BaseEntity
{
    public int BeachId { get; private set; }
    public Beach? Beach { get; private set; }

    public string Url { get; private set; } = string.Empty;
    public string? Caption { get; private set; }
    public bool IsCover { get; private set; } = false;
    public int Order { get; private set; } = 0;

    // EF Core constructor
    private BeachPhoto() { }

    public BeachPhoto(int beachId, string url, string? caption = null,
                      bool isCover = false, int order = 0)
    {
        BeachId = beachId;
        Url = url ?? throw new ArgumentNullException(nameof(url));
        Caption = caption;
        IsCover = isCover;
        Order = order;
    }

    public void MakeCover() { IsCover = true; SetUpdated(); }
    public void RemoveCover() { IsCover = false; SetUpdated(); }
    public void UpdateCaption(string? caption) { Caption = caption; SetUpdated(); }
    public void UpdateOrder(int order) { Order = order; SetUpdated(); }
}
