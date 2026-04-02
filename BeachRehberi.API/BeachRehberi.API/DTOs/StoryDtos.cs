using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.DTOs;

public class CreateStoryDto
{
    [Required]
    public int BeachId { get; set; }

    [MaxLength(500)]
    public string? PhotoUrl { get; set; }

    [MaxLength(500)]
    public string? VideoUrl { get; set; }

    [MaxLength(300)]
    public string? Caption { get; set; }

    [MaxLength(50)]
    public string StoryType { get; set; } = "photo";

    public int ExpireHours { get; set; } = 24;
}

public class StoryResponseDto
{
    public int Id { get; set; }
    public int BeachId { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public string? BeachImageUrl { get; set; }
    public string? PhotoUrl { get; set; }
    public string? VideoUrl { get; set; }
    public string? Caption { get; set; }
    public string StoryType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime ExpireDate { get; set; }
}
