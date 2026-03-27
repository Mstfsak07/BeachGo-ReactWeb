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
[Authorize]
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
            return Unauthorized(ApiResponse<object>.FailureResult("Kullanıcı kimliği doğrulanamadı."));

        var res = await _reservationService.GetByUserAsync(userId);
        return Ok(ApiResponse<List<Reservation>>.SuccessResult(res));
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Cancel(string code) 
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdStr, out int userId)) 
            return Unauthorized(ApiResponse<object>.FailureResult("Kullanıcı kimliği doğrulanamadı."));

        var success = await _reservationService.CancelAsync(code, userId);
        if (!success) 
            return BadRequest(ApiResponse<object>.FailureResult("Rezervasyon iptal edilemedi veya yetkiniz yok."));

        return Ok(ApiResponse<string>.SuccessResult(null, "Rezervasyon iptal edildi."));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Reservation reservation) 
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        reservation.UserId = int.TryParse(userIdStr, out int userId) ? userId : null;
        
        var result = await _reservationService.CreateAsync(reservation);
        return Ok(ApiResponse<Reservation>.SuccessResult(result, "Rezervasyon başarıyla oluşturuldu."));
    }
}
