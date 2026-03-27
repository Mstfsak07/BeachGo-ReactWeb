using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.RateLimiting;

namespace BeachRehberi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("fixed")]
    public class ReservationsController : ControllerBase
    {
        private readonly BeachDbContext _context;
        private readonly IReservationService _reservationService;

        public ReservationsController(BeachDbContext context, IReservationService reservationService)
        {
            _context = context;
            _reservationService = reservationService;
        }

        [HttpGet("phone/{phone}")]
        public async Task<IActionResult> GetByPhone(string phone)
        {
            var res = await _reservationService.GetByPhoneAsync(phone);
            return Ok(ApiResponse<List<Reservation>>.SuccessResult(res));
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> GetByCode(string code)
        {
            var res = await _reservationService.GetByCodeAsync(code);
            if (res == null) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon bulunamadı."));
            return Ok(ApiResponse<Reservation>.SuccessResult(res));
        }

        [HttpDelete("{code}")]
        public async Task<IActionResult> Cancel(string code)
        {
            var success = await _reservationService.CancelAsync(code);
            if (!success) return NotFound(ApiResponse<string>.FailureResult("Rezervasyon bulunamadı."));
            return Ok(ApiResponse<string>.SuccessResult(null, "Rezervasyon iptal edildi."));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Reservation reservation)
        {
            try {
                var result = await _reservationService.CreateAsync(reservation);
                return Ok(ApiResponse<Reservation>.SuccessResult(result));
            } catch (Exception ex) {
                return BadRequest(ApiResponse<string>.FailureResult(ex.Message));
            }
        }
    }
}