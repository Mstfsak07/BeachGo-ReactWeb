using System.Collections.Generic;
using System.Threading.Tasks;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Services;

public interface IStoryService
{
    Task<List<StoryResponseDto>> GetActiveStoriesAsync();
    Task<List<StoryResponseDto>> GetStoriesByBeachAsync(int beachId);
    Task<ServiceResult<StoryResponseDto>> CreateAsync(CreateStoryDto dto);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
