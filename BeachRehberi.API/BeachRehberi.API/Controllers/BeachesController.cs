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

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var beaches = await _beachService.GetAllAsync();
        return Ok(ApiResponse<List<Beach>>.SuccessResult(beaches));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var beach = await _beachService.GetByIdAsync(id);
        if (beach == null) 
            return NotFound(ApiResponse<Beach>.FailureResult("Plaj bulunamadı."));
            
        return Ok(ApiResponse<Beach>.SuccessResult(beach));
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string q)
    {
        var results = await _beachService.SearchAsync(q);
        return Ok(ApiResponse<List<Beach>>.SuccessResult(results));
    }

    [HttpPost("filter")]
    public async Task<IActionResult> Filter([FromBody] BeachFilter filter)
    {
        var results = await _beachService.FilterAsync(filter);
        return Ok(ApiResponse<List<Beach>>.SuccessResult(results));
    }

    [HttpGet("{id}/weather")]
    public async Task<IActionResult> GetWeather(int id)
    {
        var beach = await _beachService.GetByIdAsync(id);
        if (beach == null) 
            return NotFound(ApiResponse<object>.FailureResult("Plaj bulunamadı."));

        var weather = await _weatherService.GetWeatherAsync(beach.Latitude, beach.Longitude);
        var sea = await _weatherService.GetSeaDataAsync(beach.Latitude, beach.Longitude);

        return Ok(ApiResponse<object>.SuccessResult(new { weather, sea }));
    }

    [HttpGet("weather/konyaalti")]
    public async Task<IActionResult> GetKonyaaltiWeather()
    {
        var weather = await _weatherService.GetWeatherAsync(36.8785, 30.6657);
        var sea = await _weatherService.GetSeaDataAsync(36.8785, 30.6657);
        return Ok(ApiResponse<object>.SuccessResult(new { weather, sea }));
    }
}
