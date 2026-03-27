using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventsController : ControllerBase
{
    private readonly BeachDbContext _db;

    public EventsController(BeachDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() 
    {
        var data = await _db.Events
            .Include(e => e.Beach)
            .Where(e => e.IsActive && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
        return data.ToOkApiResponse();
    }

    [HttpGet("today")]
    public async Task<IActionResult> GetToday()
    {
        var today = DateTime.UtcNow.Date;
        var data = await _db.Events
            .Include(e => e.Beach)
            .Where(e => e.IsActive && e.StartDate.Date == today)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
        return data.ToOkApiResponse();
    }

    [HttpGet("beach/{beachId}")]
    public async Task<IActionResult> GetByBeach(int beachId)
    {
        var data = await _db.Events
            .Where(e => e.BeachId == beachId && e.IsActive && e.StartDate >= DateTime.UtcNow)
            .OrderBy(e => e.StartDate)
            .ToListAsync();
        return data.ToOkApiResponse();
    }
}
