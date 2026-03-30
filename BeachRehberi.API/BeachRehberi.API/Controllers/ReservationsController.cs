using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReservationsController : ControllerBase
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService)
    {
        _reservationService = reservationService;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReservationDto dto)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();
        
        var userId = int.Parse(userIdStr);
        var result = await _reservationService.CreateAsync(dto, userId);
        return result.ToActionResult();
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations()
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = int.Parse(userIdStr);
        var reservations = await _reservationService.GetByUserAsync(userId);
        return Ok(new { success = true, data = reservations });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userIdStr == null) return Unauthorized();

        var userId = int.Parse(userIdStr);
        
        // Owner verification is now strictly handled and communicated via ServiceResult
        var cancelResult = await _reservationService.CancelAsync(id, userId);
        return cancelResult.ToActionResult();
    }
}
