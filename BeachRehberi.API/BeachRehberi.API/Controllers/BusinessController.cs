using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using System.Security.Claims;

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

        // ─── GET DASHBOARD STATS ──────────────────────────────────
        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboardStats()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return Unauthorized(ApiResponse<string>.FailureResult("GeĂ§ersiz iĹąletme yetkisi."));

            var beach = await _context.Beaches
                .Include(b => b.Reservations)
                .Include(b => b.Events)
                .FirstOrDefaultAsync(b => b.Id == beachId);

            if (beach == null) return NotFound(ApiResponse<string>.FailureResult("Plaj bulunamadÄą."));

            return Ok(ApiResponse<object>.SuccessResult(new {
                beach.Name,
                beach.OccupancyRate,
                TotalReservations = beach.Reservations.Count,
                PendingReservations = beach.Reservations.Count(r => r.Status == ReservationStatus.Pending),
                ActiveEvents = beach.Events.Count
            }));
        }

        // ─── GET RESERVATIONS (SADECE KENDÄ° PLAJI) ───────────────
        [HttpGet("reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return Unauthorized(ApiResponse<string>.FailureResult("Yetkisiz iĹąletme ID."));

            var reservations = await _context.Reservations
                .Where(r => r.BeachId == beachId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Ok(ApiResponse<List<Reservation>>.SuccessResult(reservations));
        }

        // ─── UPDATE OCCUPANCY (SADECE KENDÄ° PLAJI) ────────────────
        [HttpPut("occupancy")]
        public async Task<IActionResult> UpdateOccupancy([FromBody] int percent)
        {
            var beachId = GetUserBeachId();
            var beach = await _context.Beaches.FindAsync(beachId);
            if (beach == null) return NotFound();

            beach.OccupancyRate = percent;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "Doluluk oranÄą gĂźncellendi."));
        }

        // ─── APPROVE/REJECT RESERVATION (GĂźvenli Kontrol) ────────
        [HttpPut("reservations/{id}/approve")]
        public async Task<IActionResult> ApproveReservation(int id)
        {
            var beachId = GetUserBeachId();
            var res = await _context.Reservations
                .FirstOrDefaultAsync(r => r.Id == id && r.BeachId == beachId); // Kendi plajÄą mÄą?

            if (res == null) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon size ait deÄąil veya bulunamadÄą."));

            res.Status = ReservationStatus.Approved;
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "OnaylandÄą."));
        }

        private int GetUserBeachId()
        {
            var claim = User.FindFirst("BeachId")?.Value;
            return int.TryParse(claim, out int beachId) ? beachId : -1;
        }
    }
}
