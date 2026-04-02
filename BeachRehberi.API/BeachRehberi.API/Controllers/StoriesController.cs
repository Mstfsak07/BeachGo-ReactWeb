using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StoriesController : ControllerBase
{
    private readonly IStoryService _storyService;

    public StoriesController(IStoryService storyService)
    {
        _storyService = storyService;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive()
    {
        var stories = await _storyService.GetActiveStoriesAsync();
        return Ok(new { success = true, data = stories });
    }

    [HttpGet("beach/{beachId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByBeach(int beachId)
    {
        var stories = await _storyService.GetStoriesByBeachAsync(beachId);
        return Ok(new { success = true, data = stories });
    }

    [HttpPost]
    [Authorize(Roles = "Business,Admin")]
    public async Task<IActionResult> Create([FromBody] CreateStoryDto dto)
    {
        var result = await _storyService.CreateAsync(dto);
        return result.ToActionResult();
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Business,Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _storyService.DeleteAsync(id);
        return result.ToActionResult();
    }
}
