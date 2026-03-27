using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
[Authorize] // Requirement 3: Minimum "User" role for basic operations
public class ReservationsController : ControllerBase 
{
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService) 
    {
        _reservationService = reservationService;
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyReservations() 
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) 
            return "Kullanıcı kimliği doğrulanamadı.".ToUnauthorizedApiResponse();

        var res = await _reservationService.GetByUserAsync(userId);
        return res.ToOkApiResponse();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Cancel(string code) 
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) 
            return "Kullanıcı kimliği doğrulanamadı.".ToUnauthorizedApiResponse();

        var success = await _reservationService.CancelAsync(code, userId);
        return success ? "Rezervasyon iptal edildi.".ToOkApiResponse() : "Rezervasyon iptal edilemedi veya yetkiniz yok.".ToBadRequestApiResponse();
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reservation reservation) 
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        reservation.UserId = int.TryParse(userIdStr, out int userId) ? userId : null;
        
        var result = await _reservationService.CreateAsync(reservation);
        return result.ToActionResult(); // Use mapper
    }
}
