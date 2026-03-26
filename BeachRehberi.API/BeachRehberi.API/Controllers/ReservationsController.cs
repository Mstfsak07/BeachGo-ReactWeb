using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationsController : ControllerBase
    {
        private readonly BeachDbContext _context;

        public ReservationsController(BeachDbContext context)
        {
            _context = context;
        }

        [HttpGet("phone/{phone}")]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            var res = await _context.Reservations
                .Where(r => r.Phone == phone)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
            return Ok(ApiResponse<List<Reservation>>.SuccessResult(res));
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> Cancel(string code, [FromQuery] string phone)
        {
            var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Code == code && r.Phone == phone);
            
            if (res == null) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon veya telefon numarası hatalı."));
            
            if (res.Status == ReservationStatus.Approved)
                return BadRequest(ApiResponse<string>.FailureResult("Onaylanmış rezervasyonlar iptal edilemez."));

            _context.Reservations.Remove(res);
            await _context.SaveChangesAsync();
            
            return Ok(ApiResponse<string>.SuccessResult(null, "Rezervasyon iptal edildi."));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Reservation reservation)
        {
            // Secure 6-char upper code
            reservation.Code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.Status = ReservationStatus.Pending;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Reservation>.SuccessResult(reservation));
        }
    }
}
