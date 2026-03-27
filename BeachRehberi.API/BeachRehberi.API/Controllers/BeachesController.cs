using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BeachesController : ControllerBase
{
    private readonly IBeachService _beachService;
    private readonly IWeatherService _weatherService;

    public BeachesController(IBeachService beachService, IWeatherService weatherService)
    {
        _beachService = beachService;
        _weatherService = weatherService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => (await _beachService.GetAllAsync()).ToOkApiResponse();

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var beach = await _beachService.GetByIdAsync(id);
        return beach != null ? beach.ToOkApiResponse() : "Plaj bulunamadı.".ToNotFoundApiResponse();
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q) => (await _beachService.SearchAsync(q)).ToOkApiResponse();

    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] BeachFilter filter) => (await _beachService.FilterAsync(filter)).ToOkApiResponse();

    [HttpGet("{id}/weather")]
    public async Task<IActionResult> GetWeather(int id)
    {
        var beach = await _beachService.GetByIdAsync(id);
        if (beach == null) return "Plaj bulunamadı.".ToNotFoundApiResponse();

        var weather = await _weatherService.GetWeatherAsync(beach.Latitude, beach.Longitude);
        var sea = await _weatherService.GetSeaDataAsync(beach.Latitude, beach.Longitude);

        return new { weather, sea }.ToOkApiResponse();
    }

    [HttpGet("weather/konyaalti")]
    public async Task<IActionResult> GetKonyaaltiWeather()
    {
        var weather = await _weatherService.GetWeatherAsync(36.8785, 30.6657);
        var sea = await _weatherService.GetSeaDataAsync(36.8785, 30.6657);
        return new { weather, sea }.ToOkApiResponse();
    }
}
