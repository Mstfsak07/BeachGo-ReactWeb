using System;
using System.ComponentModel.DataAnnotations;

namespace BeachRehberi.API.DTOs;

public class CreateStoryDto
{
    [Required]
    public int BeachId { get; set; }

    [MaxLength(500)]
    public string? MediaUrl { get; set; }

    [MaxLength(300)]
    public string? Caption { get; set; }

    [MaxLength(50)]
    public string MediaType { get; set; } = "image";

    public int ExpireHours { get; set; } = 24;
}

public class StoryResponseDto
{
    public int Id { get; set; }
    public int BeachId { get; set; }
    public string BeachName { get; set; } = string.Empty;
    public string? BeachImageUrl { get; set; }
    public string MediaUrl { get; set; } = string.Empty;
    public string MediaType { get; set; } = string.Empty;
    public string? Caption { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}
