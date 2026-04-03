using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
using BeachRehberi.API.DTOs;
using System.Security.Claims;

namespace BeachRehberi.API.Controllers
{
    [Authorize(Roles = UserRoles.Business + "," + UserRoles.Admin)]
    [EnableRateLimiting("fixed")]
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessController(IBusinessService businessService) => _businessService = businessService;

        [HttpGet("reservations")]
        public async Task<IActionResult> GetMyReservations()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

            var reservations = await _businessService.GetAllReservationsAsync(beachId);
            return reservations.ToOkApiResponse();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetMyStats()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

            var stats = await _businessService.GetStatsAsync(beachId);
            return stats.ToOkApiResponse();
        }

        [HttpGet("beach")]
        public async Task<IActionResult> GetMyBeach()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

            var beach = await _businessService.GetBeachByIdAsync(beachId);
            return beach.ToOkApiResponse();
        }

        [HttpPut("beach")]
        public async Task<IActionResult> UpdateMyBeach([FromBody] UpdateBeachDto beachUpdate)
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

            // Sadece belirli alanların güncellenmesine izin ver
            var result = await _businessService.UpdateBeachDetailsAsync(beachId, beachUpdate);
            return result.ToActionResult();
        }

        [HttpPut("reservations/{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Kendi işletme yetkiniz bulunamadı, işlem reddedildi.".ToForbiddenApiResponse();

            var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Approved);
            return result.ToActionResult();
        }

        [HttpPut("reservations/{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? comment)
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Kendi iÃ…Å¸letme yetkiniz bulunamadÃ„Â±, iÃ…Å¸lem reddedildi.".ToForbiddenApiResponse();

            var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Rejected, comment);
            return result.ToActionResult();
        }

        [HttpPut("reservations/{id}/cancel")]
        public async Task<IActionResult> Cancel(int id, [FromBody] string? comment)
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Kendi iÃ…Å¸letme yetkiniz bulunamadÃ„Â±, iÃ…Å¸lem reddedildi.".ToForbiddenApiResponse();

            var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Cancelled, comment);
            return result.ToActionResult();
        }

        private int GetUserBeachId()
        {
            var claim = User.FindFirst("BeachId")?.Value;
            return int.TryParse(claim, out int beachId) ? beachId : -1;
        }
    }
}
