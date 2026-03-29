using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using BeachRehberi.API.Services;
using BeachRehberi.API.Extensions;
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
            if (beachId == -1) return "İşletme yetkiniz bulunamadı.".ToUnauthorizedApiResponse();

            var reservations = await _businessService.GetAllReservationsAsync(beachId);
            return reservations.ToOkApiResponse();
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetMyStats()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "İşletme yetkiniz bulunamadı.".ToUnauthorizedApiResponse();

            // Gerçek projede bu veriler servisten gelmeli
            var stats = new {
                TotalReservations = 124,
                TodayCheckins = 18,
                OccupancyRate = 65,
                EstimatedEarnings = 12400,
                RecentActivity = new[] {
                    new { Name = "Murat", Action = "Yeni Rezervasyon", Time = "10 dk önce" },
                    new { Name = "Ayşe", Action = "İptal Talebi", Time = "45 dk önce" }
                }
            };
            return stats.ToOkApiResponse();
        }

        [HttpGet("beach")]
        public async Task<IActionResult> GetMyBeach()
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "İşletme yetkiniz bulunamadı.".ToUnauthorizedApiResponse();

            var beach = await _businessService.GetBeachByIdAsync(beachId);
            return beach.ToOkApiResponse();
        }

        [HttpPut("beach")]
        public async Task<IActionResult> UpdateMyBeach([FromBody] Beach beachUpdate)
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "İşletme yetkiniz bulunamadı.".ToUnauthorizedApiResponse();

            // Sadece belirli alanların güncellenmesine izin ver
            var result = await _businessService.UpdateBeachDetailsAsync(beachId, beachUpdate);
            return result.ToActionResult();
        }

        [HttpPut("reservations/{id}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Yetki hatası.".ToUnauthorizedApiResponse();

            var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Approved);
            return result.ToActionResult();
        }

        [HttpPut("reservations/{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] string? comment)
        {
            var beachId = GetUserBeachId();
            if (beachId == -1) return "Yetki hatası.".ToUnauthorizedApiResponse();

            var result = await _businessService.UpdateReservationStatusAsync(id, beachId, ReservationStatus.Rejected, comment);
            return result.ToActionResult();
        }

        private int GetUserBeachId()
        {
            var claim = User.FindFirst("BeachId")?.Value;
            return int.TryParse(claim, out int beachId) ? beachId : -1;
        }
    }
}
