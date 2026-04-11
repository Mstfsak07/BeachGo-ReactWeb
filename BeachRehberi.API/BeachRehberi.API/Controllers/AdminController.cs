using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BeachRehberi.API.Models;
using BeachRehberi.API.Extensions;
using BeachRehberi.API.Services;

namespace BeachRehberi.API.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetGlobalStats()
        {
            return Ok(await _adminService.GetGlobalStatsAsync());
        }

        [HttpGet("beaches")]
        public async Task<IActionResult> GetAllBeaches()
        {
            return Ok(await _adminService.GetAllBeachesAsync());
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            return Ok(await _adminService.GetAllUsersAsync());
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetAllReservations()
        {
            return Ok(await _adminService.GetAllReservationsAsync());
        }

        [HttpPatch("beaches/{id}/toggle-status")]
        public async Task<IActionResult> ToggleBeachStatus(int id)
        {
            var beach = await _adminService.GetBeachByIdAsync(id);
            if (beach == null) return NotFound();

            await _adminService.ToggleBeachStatusAsync(id);

            return Ok(new { success = true, isActive = beach.IsActive });
        }

        [HttpGet("beaches/{id}")]
        public async Task<IActionResult> GetBeachById(int id)
        {
            var beach = await _adminService.GetBeachByIdAsync(id);
            if (beach == null) return NotFound(new { success = false, message = "Plaj bulunamadı." });
            return Ok(new { success = true, data = beach });
        }

        [HttpPut("beaches/{id}")]
        public async Task<IActionResult> UpdateBeach(int id, [FromBody] DTOs.UpdateBeachDto dto)
        {
            var updated = await _adminService.UpdateBeachAsync(id, dto);
            if (!updated) return NotFound(new { success = false, message = "Plaj bulunamadı." });

            return Ok(new { success = true, message = "Plaj başarıyla güncellendi." });
        }

        [HttpPost("beaches/import")]
        public async Task<IActionResult> ImportBeaches([FromBody] List<DTOs.UpdateBeachDto> beaches)
        {
            if (beaches == null || beaches.Count == 0)
                return BadRequest(new { success = false, message = "Veri bulunamadı." });

            var result = await _adminService.ImportBeachesAsync(beaches);
            return Ok(new { success = true, message = $"{result.CreatedCount} yeni plaj eklendi, {result.UpdatedCount} plaj güncellendi." });
        }
    }
}
