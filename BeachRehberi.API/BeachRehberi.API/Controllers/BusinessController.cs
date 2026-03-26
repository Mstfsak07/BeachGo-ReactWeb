using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly IBeachService _beachService;
    private readonly IBusinessService _businessService;

    public BusinessController(IBeachService beachService, IBusinessService businessService)
    {
        _beachService = beachService;
        _businessService = businessService;
    }

    private int GetBeachId() =>
        int.Parse(User.FindFirst("BeachId")?.Value ?? "0");

    // GET api/business/dashboard
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
    {
        var beachId = GetBeachId();
        var beach = await _beachService.GetByIdAsync(beachId);
        if (beach == null) return NotFound();

        var today = DateTime.UtcNow.Date;
        var reservations = await _businessService.GetReservationsAsync(beachId, today);
        var events = await _businessService.GetEventsAsync(beachId);

        return Ok(new
        {
            Beach = beach,
            TodayReservations = reservations,
            UpcomingEvents = events,
            Stats = new
            {
                TodayReservationCount = reservations.Count,
                TodayPersonCount = reservations.Sum(r => r.PersonCount),
                OccupancyPercent = beach.OccupancyPercent,
                OccupancyLevel = beach.OccupancyLevel.ToString()
            }
        });
    }

    // PUT api/business/occupancy
    [HttpPut("occupancy")]
    public async Task<IActionResult> UpdateOccupancy([FromBody] OccupancyDto dto)
    {
        var beachId = GetBeachId();
        var level = dto.Percent switch
        {
            <= 20 => OccupancyLevel.Empty,
            <= 40 => OccupancyLevel.Low,
            <= 60 => OccupancyLevel.Medium,
            <= 80 => OccupancyLevel.High,
            _ => OccupancyLevel.Full
        };

        await _beachService.UpdateOccupancyAsync(beachId, dto.Percent, level);
        return Ok(new { Message = "Doluluk güncellendi!", Percent = dto.Percent, Level = level.ToString() });
    }

    // PUT api/business/special
    [HttpPut("special")]
    public async Task<IActionResult> UpdateSpecial([FromBody] SpecialDto dto)
    {
        var beachId = GetBeachId();
        await _beachService.UpdateTodaySpecialAsync(beachId, dto.Message);
        return Ok(new { Message = "Günlük özel güncellendi!" });
    }

    // POST api/business/events
    [HttpPost("events")]
    public async Task<IActionResult> AddEvent([FromBody] AddEventDto dto)
    {
        var beachId = GetBeachId();
        var ev = new BeachEvent
        {
            BeachId = beachId,
            Title = dto.Title,
            Description = dto.Description,
            Category = dto.Category,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            TicketPrice = dto.TicketPrice,
            Capacity = dto.Capacity,
            AvailableSpots = dto.Capacity,
            IsAgeRestricted = dto.IsAgeRestricted,
            MinAge = dto.MinAge
        };

        var result = await _businessService.AddEventAsync(ev);
        return Ok(result);
    }

    // DELETE api/business/events/5
    [HttpDelete("events/{eventId}")]
    public async Task<IActionResult> DeleteEvent(int eventId)
    {
        var beachId = GetBeachId();
        var result = await _businessService.DeleteEventAsync(eventId, beachId);
        if (!result) return NotFound();
        return Ok(new { Message = "Etkinlik silindi." });
    }

    // GET api/business/reservations?date=2026-03-19
    [HttpGet("reservations")]
    public async Task<IActionResult> GetReservations([FromQuery] DateTime? date)
    {
        var beachId = GetBeachId();
        var targetDate = date ?? DateTime.UtcNow;
        var reservations = await _businessService.GetReservationsAsync(beachId, targetDate);
        return Ok(reservations);
    }
}

public class OccupancyDto { public int Percent { get; set; } }
public class SpecialDto { public string Message { get; set; } = string.Empty; }
public class AddEventDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TicketPrice { get; set; }
    public int Capacity { get; set; }
    public bool IsAgeRestricted { get; set; }
    public int MinAge { get; set; }
}