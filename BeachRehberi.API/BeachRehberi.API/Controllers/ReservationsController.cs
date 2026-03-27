using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.RateLimiting; 
using System.Security.Claims;

namespace BeachRehberi.API.Controllers;  

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class ReservationsController : ControllerBase {
    private readonly IReservationService _reservationService;

    public ReservationsController(IReservationService reservationService) {
        _reservationService = reservationService;
    }

    [Authorize]
    [HttpGet("phone/{phone}")]
    public async Task<IActionResult> GetByPhone(string phone) {
        var res = await _reservationService.GetByPhoneAsync(phone);
        return Ok(ApiResponse<List<Reservation>>.SuccessResult(res));
    }

    [Authorize]
    [HttpDelete("{code}")]
    public async Task<IActionResult> Cancel(string code) {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) return Unauthorized();

        var success = await _reservationService.CancelAsync(code, userId);
        if (!success) return Forbid();

        return Ok(ApiResponse<string>.SuccessResult(null, "İptal edildi."));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reservation reservation) {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId)) reservation.UserId = userId;
        var result = await _reservationService.CreateAsync(reservation);
        return Ok(ApiResponse<Reservation>.SuccessResult(result));
    }
}
