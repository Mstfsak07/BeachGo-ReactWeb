using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
namespace BeachRehberi.API.Models;

public class Review {
    public int Id { get; set; }
    public int BeachId { get; set; }
    [JsonIgnore]
    public Beach? Beach { get; set; }
    [JsonIgnore]
    public int UserId { get; set; } 
    
    // Projenin geri kalanının beklediği alanlar:
    public string UserName { get; set; } = string.Empty;
    [JsonIgnore]
    public string UserPhone { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = true;

    [Range(1, 5)] 
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}