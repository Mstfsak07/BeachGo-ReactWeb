using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Controllers
{
    [Authorize(Roles = "BusinessOwner,Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly BeachDbContext _context;

        public BusinessController(BeachDbContext context)
        {
            _context = context;
        }

        // Ĺletmenin kendi plajÄąndaki rezervasyonlarÄą listele
        [HttpGet("reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            // Token'dan BeachId'yi al (Claim bazlÄą gĂźvenli okuma)
            var beachIdClaim = User.FindFirst("BeachId")?.Value;
            if (string.IsNullOrEmpty(beachIdClaim)) return BadRequest(ApiResponse<string>.FailureResult("Yetkisiz giriĹą."));

            var beachId = int.Parse(beachIdClaim);
            var reservations = await _context.Reservations
                .Where(r => r.BeachId == beachId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse<List<Reservation>>.SuccessResult(reservations));
        }

        // Rezervasyon Onayla
        [HttpPut("reservations/{id}/approve")]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon bulunamadÄą."));

            res.Status = ReservationStatus.Approved;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "Rezervasyon onaylandÄą."));
        }

        // Rezervasyon Reddet
        [HttpPut("reservations/{id}/reject")]
        public async Task<IActionResult> RejectReservation(int id, [FromBody] string comment)
        {
            var res = await _context.Reservations.FindAsync(id);
            if (res == null) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon bulunamadÄą."));

            res.Status = ReservationStatus.Rejected;
            res.BusinessComment = comment;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "Rezervasyon reddedildi."));
        }
    }
}
