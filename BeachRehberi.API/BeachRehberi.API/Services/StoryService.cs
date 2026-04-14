using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class StoryService : IStoryService
{
    private readonly BeachDbContext _db;

    public StoryService(BeachDbContext db)
    {
        _db = db;
    }

    public async Task<List<StoryResponseDto>> GetActiveStoriesAsync()
    {
        var now = DateTime.UtcNow;
        return await _db.BeachStories
            .Include(s => s.Beach)
            .Where(s => s.IsActive && !s.IsArchived && s.ExpireDate > now)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapStory(s))
            .ToListAsync();
    }

    public async Task<List<StoryResponseDto>> GetStoriesByBeachAsync(int beachId)
    {
        var now = DateTime.UtcNow;
        return await _db.BeachStories
            .Include(s => s.Beach)
            .Where(s => s.BeachId == beachId && s.IsActive && !s.IsArchived && s.ExpireDate > now)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => MapStory(s))
            .ToListAsync();
    }

    public async Task<ServiceResult<StoryResponseDto>> CreateAsync(CreateStoryDto dto)
    {
        var beach = await _db.Beaches.FindAsync(dto.BeachId);
        if (beach == null)
            return ServiceResult<StoryResponseDto>.FailureResult("Plaj bulunamadı.");

        if (string.IsNullOrWhiteSpace(dto.MediaUrl))
            return ServiceResult<StoryResponseDto>.FailureResult("Media URL gereklidir.");

        var mediaType = NormalizeMediaType(dto.MediaType, dto.MediaUrl);

        var story = new BeachStory
        {
            BeachId = dto.BeachId,
            PhotoUrl = mediaType == "video" ? null : dto.MediaUrl,
            VideoUrl = mediaType == "video" ? dto.MediaUrl : null,
            Caption = dto.Caption,
            StoryType = mediaType,
            ExpireDate = DateTime.UtcNow.AddHours(Math.Clamp(dto.ExpireHours, 1, 168)),
        };

        _db.BeachStories.Add(story);
        await _db.SaveChangesAsync();

        story.Beach = beach;

        return ServiceResult<StoryResponseDto>.SuccessResult(
            MapStory(story),
            "Story basariyla olusturuldu.");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var story = await _db.BeachStories.FindAsync(id);
        if (story == null)
            return ServiceResult<bool>.FailureResult("Story bulunamadı.");

        story.IsActive = false;
        story.IsArchived = true;
        await _db.SaveChangesAsync();
        return ServiceResult<bool>.SuccessResult(true, "Story silindi.");
    }

    private static StoryResponseDto MapStory(BeachStory story)
    {
        return new StoryResponseDto
        {
            Id = story.Id,
            BeachId = story.BeachId,
            BeachName = story.Beach.Name,
            BeachImageUrl = story.Beach.CoverImageUrl,
            MediaUrl = story.VideoUrl ?? story.PhotoUrl ?? string.Empty,
            MediaType = NormalizeMediaType(story.StoryType, story.VideoUrl ?? story.PhotoUrl),
            Caption = story.Caption,
            CreatedAt = story.CreatedAt,
            ExpiresAt = story.ExpireDate
        };
    }

    private static string NormalizeMediaType(string? rawType, string? mediaUrl)
    {
        var normalized = rawType?.Trim().ToLowerInvariant();
        if (normalized == "video")
        {
            return "video";
        }

        if (normalized == "image" || normalized == "photo")
        {
            return "image";
        }

        var url = mediaUrl?.Trim().ToLowerInvariant() ?? string.Empty;
        if (url.EndsWith(".mp4") || url.EndsWith(".mov") || url.EndsWith(".webm") || url.EndsWith(".m3u8"))
        {
            return "video";
        }

        return "image";
    }
}
