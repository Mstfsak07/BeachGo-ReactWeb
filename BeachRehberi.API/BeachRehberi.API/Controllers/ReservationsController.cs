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

        // ─── REZERVASYON Ä°PTALÄ° (GĂśvenli DoÄąrulama) ────────────
        [HttpDelete("{code}")]
        public async Task<IActionResult> CancelReservation(string code)
        {
            // Sadece 'Pending' (Beklemede) olanlar iptal edilebilir (GĂźvenlik kuralÄą)
            var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Code == code);
            
            if (res == null) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon kodu hatalÄą."));

            if (res.Status == ReservationStatus.Approved)
                return BadRequest(ApiResponse<string>.FailureResult("OnaylanmÄąĹą rezervasyonlar sadece iĹąletme tarafÄąndan iptal edilebilir."));

            _context.Reservations.Remove(res);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResult(null, "Rezervasyon baĹąarÄąyla iptal edildi."));
        }

        // ─── YENÄ° REZERVASYON (GĂśvenli Kod Ăretimi) ─────────────
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Reservation reservation)
        {
            // Benzersiz 6 Haneli Sorgu Kodu Ăret
            reservation.Code = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            reservation.CreatedAt = DateTime.UtcNow;
            reservation.Status = ReservationStatus.Pending;

            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<Reservation>.SuccessResult(reservation, "Rezervasyon talebiniz alÄąndÄą."));
        }
    }
}
