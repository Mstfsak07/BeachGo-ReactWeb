using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using System.Security.Claims;

namespace BeachRehberi.API.Controllers
{
    [Authorize(Roles = "BusinessOwner,Admin")]
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly BeachDbContext _context;

        public BusinessController(BeachDbContext context)
        {
            _context = context;
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return Unauthorized();

            var reservations = await _context.Reservations
                .Where(r => r.BeachId == beachId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse<List<Reservation>>.SuccessResult(reservations));
        }

        [HttpPut("reservations/{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            return await UpdateReservationStatus(id, ReservationStatus.Approved);
        }

        [HttpPut("reservations/{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? comment)
        {
            return await UpdateReservationStatus(id, ReservationStatus.Rejected, comment);
        }

        private async Task<IActionResult> UpdateReservationStatus(int id, ReservationStatus status, string? comment = null)
        {
            var beachId = GetUserBeachId();
            var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.BeachId == beachId);

            if (res == null) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon bulunamadı."));

            res.Status = status;
            if (comment != null) res.BusinessComment = comment;
            
            await _context.SaveChangesAsync();
            return Ok(ApiResponse<string>.SuccessResult(null, $"Rezervasyon {status} durumuna getirildi."));
        }

        private int GetUserBeachId()
        {
            var claim = User.FindFirst("BeachId")?.Value;
            return int.TryParse(claim, out int beachId) ? beachId : -1;
        }
    }
}
