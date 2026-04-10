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
            var revenue = await _context.Reservations
                .Where(r => r.PaymentStatus == "Paid" &&
                            r.Status != ReservationStatus.Cancelled &&
                            r.Status != ReservationStatus.Rejected)
                .SumAsync(r => r.TotalPrice);

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
                    b.Rating,
                    b.InstagramUsername,
                    b.SocialContentSource
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
                    BeachName = r.Beach != null ? r.Beach.Name : "Bilinmiyor",
                    CustomerName = r.IsGuest 
                        ? (r.GuestFirstName + " " + r.GuestLastName).Trim() 
                        : (r.User != null ? (r.User.ContactName ?? r.User.Email) : "Bilinmiyor"),
                    Phone = r.IsGuest ? r.GuestPhone : "Bilinmiyor",
                    r.ReservationDate,
                    ReservationTime = r.ReservationTime != null ? r.ReservationTime.ToString() : null,
                    r.PersonCount,
                    ReservationType = r.ReservationType ?? "Standart",
                    r.Status,
                    r.IsGuest,
                    r.ConfirmationCode,
                    r.CreatedAt
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

        [HttpGet("beaches/{id}")]
        public async Task<IActionResult> GetBeachById(int id)
        {
            var beach = await _context.Beaches.FindAsync(id);
            if (beach == null) return NotFound(new { success = false, message = "Plaj bulunamadı." });
            return Ok(new { success = true, data = beach });
        }

        [HttpPut("beaches/{id}")]
        public async Task<IActionResult> UpdateBeach(int id, [FromBody] DTOs.UpdateBeachDto dto)
        {
            var beach = await _context.Beaches.FindAsync(id);
            if (beach == null) return NotFound(new { success = false, message = "Plaj bulunamadı." });

            beach.UpdateDetails(dto.Name, dto.Description, dto.Address);
            beach.Phone = dto.Phone;
            beach.Website = dto.Website;
            beach.Instagram = dto.Instagram;
            beach.InstagramUsername = dto.InstagramUsername;
            beach.SocialContentSource = dto.SocialContentSource;
            beach.OpenTime = dto.OpenTime;
            beach.CloseTime = dto.CloseTime;
            beach.UpdateFees(dto.HasEntryFee, dto.EntryFee, dto.SunbedPrice);
            beach.Latitude = dto.Latitude;
            beach.Longitude = dto.Longitude;
            beach.Capacity = dto.Capacity;
            beach.HasSunbeds = dto.HasSunbeds;
            beach.HasShower = dto.HasShower;
            beach.HasParking = dto.HasParking;
            beach.HasRestaurant = dto.HasRestaurant;
            beach.HasBar = dto.HasBar;
            beach.HasAlcohol = dto.HasAlcohol;
            beach.IsChildFriendly = dto.IsChildFriendly;
            beach.HasWaterSports = dto.HasWaterSports;
            beach.HasWifi = dto.HasWifi;
            beach.HasPool = dto.HasPool;
            beach.HasDJ = dto.HasDJ;
            beach.HasAccessibility = dto.HasAccessibility;
            beach.SetTodaySpecial(dto.TodaySpecial);

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = "Plaj başarıyla güncellendi." });
        }

        [HttpPost("beaches/import")]
        public async Task<IActionResult> ImportBeaches([FromBody] List<DTOs.UpdateBeachDto> beaches)
        {
            if (beaches == null || beaches.Count == 0)
                return BadRequest(new { success = false, message = "Veri bulunamadı." });

            var created = 0;
            foreach (var dto in beaches)
            {
                var existing = await _context.Beaches
                    .FirstOrDefaultAsync(b => b.Name == dto.Name && b.Address == dto.Address);

                if (existing != null)
                {
                    existing.UpdateDetails(dto.Name, dto.Description, dto.Address);
                    existing.Phone = dto.Phone;
                    existing.Website = dto.Website;
                    existing.Instagram = dto.Instagram;
                    existing.InstagramUsername = dto.InstagramUsername;
                    existing.SocialContentSource = dto.SocialContentSource;
                    existing.OpenTime = dto.OpenTime;
                    existing.CloseTime = dto.CloseTime;
                    existing.UpdateFees(dto.HasEntryFee, dto.EntryFee, dto.SunbedPrice);
                    existing.Capacity = dto.Capacity;
                    existing.HasSunbeds = dto.HasSunbeds;
                    existing.HasShower = dto.HasShower;
                    existing.HasParking = dto.HasParking;
                    existing.HasRestaurant = dto.HasRestaurant;
                    existing.HasBar = dto.HasBar;
                    existing.IsChildFriendly = dto.IsChildFriendly;
                    existing.HasWaterSports = dto.HasWaterSports;
                    existing.HasWifi = dto.HasWifi;
                    existing.HasAccessibility = dto.HasAccessibility;
                }
                else
                {
                    var beach = new Beach(dto.Name, dto.Description, dto.Address, dto.Latitude, dto.Longitude, 0)
                    {
                        Phone = dto.Phone,
                        Website = dto.Website,
                        Instagram = dto.Instagram,
                        InstagramUsername = dto.InstagramUsername,
                        SocialContentSource = dto.SocialContentSource,
                        OpenTime = dto.OpenTime,
                        CloseTime = dto.CloseTime,
                        Capacity = dto.Capacity,
                        HasSunbeds = dto.HasSunbeds,
                        HasShower = dto.HasShower,
                        HasParking = dto.HasParking,
                        HasRestaurant = dto.HasRestaurant,
                        HasBar = dto.HasBar,
                        IsChildFriendly = dto.IsChildFriendly,
                        HasWaterSports = dto.HasWaterSports,
                        HasWifi = dto.HasWifi,
                        HasAccessibility = dto.HasAccessibility,
                        SunbedPrice = dto.SunbedPrice,
                    };
                    beach.UpdateFees(dto.HasEntryFee, dto.EntryFee, dto.SunbedPrice);
                    _context.Beaches.Add(beach);
                    created++;
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { success = true, message = $"{created} yeni plaj eklendi, {beaches.Count - created} plaj güncellendi." });
        }
    }
}
