using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Extensions;
using BeachRehberi.API.Models.Enums;

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
            
            // Ortalama sepet tutarı 500 TRY varsayımı üzerinden genel ciro hesabı 
            // (gerçek senaryoda veritabanındaki rezervasyon ödemelerinden çekilebilir)
            var revenue = totalReservations * 500m; 

            return Ok(new {
                totalBeaches,
                totalUsers,
                totalReservations,
                pendingBeaches,
                revenue
            });
        }

        [HttpGet("beaches")]
        public async Task<IActionResult> GetAllBeaches()
        {
            var beaches = await _context.Beaches
                .Select(b => new {
                    b.Id,
                    b.Name,
                    location = b.Address,
                    imageUrl = b.CoverImageUrl,
                    b.Capacity,
                    b.IsActive,
                    b.Rating
                })
                .ToListAsync();
            return Ok(beaches);
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.BusinessUsers
                .Select(u => new {
                    u.Id,
                    u.Email,
                    u.ContactName,
                    u.BusinessName,
                    u.Role,
                    u.IsActive,
                    u.CreatedAt,
                    u.LastLoginAt
                })
                .ToListAsync();
            return Ok(users);
        }

        [HttpGet("reservations")]
        public async Task<IActionResult> GetAllReservations()
        {
            var reservations = await _context.Reservations
                .Include(r => r.User)
                .Include(r => r.Beach)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new {
                    r.Id,
                    BeachName = r.Beach.Name,
                    UserEmail = r.User != null ? r.User.Email : "Bilinmiyor",
                    r.ReservationDate,
                    r.CreatedAt,
                    r.Status
                })
                .ToListAsync();

            return Ok(reservations);
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
