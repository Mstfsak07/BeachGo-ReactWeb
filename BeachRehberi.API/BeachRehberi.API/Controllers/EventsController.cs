using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;       

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;

    public EventsController(IEventService eventService) => _eventService = eventService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var data = await _eventService.GetAllAsync();
        return data.ToOkApiResponse();
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var data = await _eventService.GetTodayAsync();
        return data.ToOkApiResponse();
    }

    [HttpGet("beach/{beachId}")]
    public async Task<IActionResult> GetByBeach(int beachId)
    {
        var data = await _eventService.GetByBeachAsync(beachId);
        return data.ToOkApiResponse();
    }
}
