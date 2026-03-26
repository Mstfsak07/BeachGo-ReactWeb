namespace BeachRehberi.API.Models;

public class BeachPhoto
{
    public int Id { get; set; }
    public int BeachId { get; set; }
    public Beach Beach { get; set; } = null!;
    public string Url { get; set; } = string.Empty;
    public string Caption { get; set; } = string.Empty;
    public bool IsCover { get; set; }
    public int Order { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}