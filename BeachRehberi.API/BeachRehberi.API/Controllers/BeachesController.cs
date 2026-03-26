using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
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

    // GET api/beaches
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var beaches = await _beachService.GetAllAsync();
        return Ok(beaches);
    }

    // GET api/beaches/5
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var beach = await _beachService.GetByIdAsync(id);
        if (beach == null) return NotFound();
        return Ok(beach);
    }

    // GET api/beaches/search?q=kalypso
    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var results = await _beachService.SearchAsync(q);
        return Ok(results);
    }

    // POST api/beaches/filter
    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] BeachFilter filter)
    {
        var results = await _beachService.FilterAsync(filter);
        return Ok(results);
    }

    // GET api/beaches/5/weather
    [HttpGet("{id}/weather")]
    public async Task<IActionResult> GetWeather(int id)
    {
        var beach = await _beachService.GetByIdAsync(id);
        if (beach == null) return NotFound();

        var weather = await _weatherService.GetWeatherAsync(beach.Latitude, beach.Longitude);
        var sea = await _weatherService.GetSeaDataAsync(beach.Latitude, beach.Longitude);

        return Ok(new { weather, sea });
    }

    // GET api/beaches/weather/all - Ana sayfa için tek istekte hava durumu
    [HttpGet("weather/konyaalti")]
    public async Task<IActionResult> GetKonyaaltiWeather()
    {
        // Konyaaltı merkez koordinatları
        var weather = await _weatherService.GetWeatherAsync(36.8785, 30.6657);
        var sea = await _weatherService.GetSeaDataAsync(36.8785, 30.6657);
        return Ok(new { weather, sea });
    }
}