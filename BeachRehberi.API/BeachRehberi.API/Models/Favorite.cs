using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeachRehberi.API.Models;

public class Favorite
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public BusinessUser User { get; set; } = null!;

    [Required]
    public int BeachId { get; set; }

    [ForeignKey(nameof(BeachId))]
    public Beach Beach { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
