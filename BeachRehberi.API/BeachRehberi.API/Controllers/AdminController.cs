using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Extensions;

namespace BeachRehberi.API.Controllers
{
    [Authorize(Roles = UserRoles.Admin)]
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly BeachDbContext _context;

        public AdminController(BeachDbContext context)
        {
            _context = context;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetGlobalStats()
        {
            var totalBeaches = await _context.Beaches.CountAsync();
            var totalUsers = await _context.BusinessUsers.CountAsync();
            var totalReservations = await _context.Reservations.CountAsync();
            var pendingBeaches = await _context.Beaches.CountAsync(b => !b.IsActive);

            return Ok(new {
                TotalBeaches = totalBeaches,
                TotalUsers = totalUsers,
                TotalReservations = totalReservations,
                PendingBeaches = pendingBeaches,
                Revenue = 154200 // Mock revenue
            });
        }

        [HttpGet("beaches")]
        public async Task<IActionResult> GetAllBeaches()
        {
            var beaches = await _context.Beaches.ToListAsync();
            return Ok(beaches);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.BusinessUsers.ToListAsync();
            return Ok(users);
        }

        [HttpPatch("beaches/{id}/toggle-status")]
        public async Task<IActionResult> ToggleBeachStatus(int id)
        {
            var beach = await _context.Beaches.FindAsync(id);
            if (beach == null) return NotFound();

            beach.IsActive = !beach.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new { success = true, isActive = beach.IsActive });
        }
    }
}
