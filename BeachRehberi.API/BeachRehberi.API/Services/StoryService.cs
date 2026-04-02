using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public interface IStoryService
{
    Task<List<StoryResponseDto>> GetActiveStoriesAsync();
    Task<List<StoryResponseDto>> GetStoriesByBeachAsync(int beachId);
    Task<ServiceResult<StoryResponseDto>> CreateAsync(CreateStoryDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}

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
            .Select(s => new StoryResponseDto
            {
                Id = s.Id,
                BeachId = s.BeachId,
                BeachName = s.Beach.Name,
                BeachImageUrl = s.Beach.CoverImageUrl,
                PhotoUrl = s.PhotoUrl,
                VideoUrl = s.VideoUrl,
                Caption = s.Caption,
                StoryType = s.StoryType,
                CreatedAt = s.CreatedAt,
                ExpireDate = s.ExpireDate
            })
            .ToListAsync();
    }

    public async Task<List<StoryResponseDto>> GetStoriesByBeachAsync(int beachId)
    {
        var now = DateTime.UtcNow;
        return await _db.BeachStories
            .Include(s => s.Beach)
            .Where(s => s.BeachId == beachId && s.IsActive && s.ExpireDate > now)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new StoryResponseDto
            {
                Id = s.Id,
                BeachId = s.BeachId,
                BeachName = s.Beach.Name,
                BeachImageUrl = s.Beach.CoverImageUrl,
                PhotoUrl = s.PhotoUrl,
                VideoUrl = s.VideoUrl,
                Caption = s.Caption,
                StoryType = s.StoryType,
                CreatedAt = s.CreatedAt,
                ExpireDate = s.ExpireDate
            })
            .ToListAsync();
    }

    public async Task<ServiceResult<StoryResponseDto>> CreateAsync(CreateStoryDto dto)
    {
        var beach = await _db.Beaches.FindAsync(dto.BeachId);
        if (beach == null)
            return ServiceResult<StoryResponseDto>.FailureResult("Plaj bulunamadı.");

        if (string.IsNullOrEmpty(dto.PhotoUrl) && string.IsNullOrEmpty(dto.VideoUrl))
            return ServiceResult<StoryResponseDto>.FailureResult("Foto veya video URL'si gereklidir.");

        var story = new BeachStory
        {
            BeachId = dto.BeachId,
            PhotoUrl = dto.PhotoUrl,
            VideoUrl = dto.VideoUrl,
            Caption = dto.Caption,
            StoryType = dto.StoryType,
            ExpireDate = DateTime.UtcNow.AddHours(Math.Clamp(dto.ExpireHours, 1, 168)),
        };

        _db.BeachStories.Add(story);
        await _db.SaveChangesAsync();

        return ServiceResult<StoryResponseDto>.SuccessResult(new StoryResponseDto
        {
            Id = story.Id,
            BeachId = story.BeachId,
            BeachName = beach.Name,
            BeachImageUrl = beach.CoverImageUrl,
            PhotoUrl = story.PhotoUrl,
            VideoUrl = story.VideoUrl,
            Caption = story.Caption,
            StoryType = story.StoryType,
            CreatedAt = story.CreatedAt,
            ExpireDate = story.ExpireDate
        }, "Story basariyla olusturuldu.");
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
}
