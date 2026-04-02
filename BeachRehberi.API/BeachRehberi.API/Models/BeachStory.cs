using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeachRehberi.API.Models;

public class BeachStory
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int BeachId { get; set; }

    [ForeignKey("BeachId")]
    public virtual Beach Beach { get; set; } = null!;

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    [MaxLength(300)]
    public string? Caption { get; set; }

    [MaxLength(50)]
    public string StoryType { get; set; } = "photo"; // photo, video, announcement, event

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime ExpireDate { get; set; } = DateTime.UtcNow.AddHours(24);

    public bool IsActive { get; set; } = true;

    public bool IsArchived { get; set; } = false;

    public bool IsExpired => DateTime.UtcNow > ExpireDate;
}
