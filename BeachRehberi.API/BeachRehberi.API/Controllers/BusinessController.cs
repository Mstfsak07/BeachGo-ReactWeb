using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Extensions;
using System.Security.Claims;

namespace BeachRehberi.API.Controllers
{
    // Requirement 4: Protected for Business/Admin roles
    [Authorize(Roles = UserRoles.Business + "," + UserRoles.Admin)]
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
            if (beachId == -1) 
                return "İşletme yetkiniz bulunamadı.".ToUnauthorizedApiResponse();

            var reservations = await _context.Reservations
                .Where(r => r.BeachId == beachId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return reservations.ToOkApiResponse();
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
            if (beachId == -1) return "Yetki hatası.".ToUnauthorizedApiResponse();

            var res = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && r.BeachId == beachId);

            if (res == null) 
                return "Rezervasyon bulunamadı.".ToNotFoundApiResponse();

            res.Status = status;
            if (comment != null) res.BusinessComment = comment;

            await _context.SaveChangesAsync();
            return ((object?)null).ToOkApiResponse($"Rezervasyon {status} durumuna getirildi.");
        }

        private int GetUserBeachId()
        {
            var claim = User.FindFirst("BeachId")?.Value;
            return int.TryParse(claim, out int beachId) ? beachId : -1;
        }
    }
}
