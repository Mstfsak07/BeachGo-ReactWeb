namespace BeachRehberi.API.Models;

public class Review
{
    public int Id { get; set; }
    public int BeachId { get; set; }
    public Beach Beach { get; set; } = null!;
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public int Rating { get; set; } // 1-5
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsApproved { get; set; } = true;
    public string Source { get; set; } = "app"; // app, google
}