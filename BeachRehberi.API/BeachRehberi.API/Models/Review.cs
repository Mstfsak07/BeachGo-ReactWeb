using System.ComponentModel.DataAnnotations;
namespace BeachRehberi.API.Models;

public class Review {
    public int Id { get; set; }
    public int BeachId { get; set; }
    public Beach? Beach { get; set; }
    public int UserId { get; set; } 
    
    // Projenin geri kalanının beklediği alanlar:
    public string UserName { get; set; } = string.Empty;
    public string UserPhone { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;

    [Range(1, 5)] 
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}