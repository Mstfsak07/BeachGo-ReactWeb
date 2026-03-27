using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("fixed")]
public class ReservationsController : ControllerBase {
    private readonly IReservationService _reservationService;
    private readonly BeachDbContext _context;

    public ReservationsController(IReservationService reservationService, BeachDbContext context) {
        _reservationService = reservationService;
        _context = context;
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
        var res = await _context.Reservations.FirstOrDefaultAsync(r => r.ConfirmationCode == code);
        if (res == null) return NotFound();
        
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdStr, out int userId) && res.UserId != userId && !User.IsInRole("Admin"))
            return Forbid();

        await _reservationService.CancelAsync(code);
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