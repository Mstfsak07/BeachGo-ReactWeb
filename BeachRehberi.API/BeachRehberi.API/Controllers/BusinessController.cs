using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using BeachRehberi.API.Models;
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
