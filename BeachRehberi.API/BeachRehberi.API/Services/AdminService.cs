using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class AdminService : IAdminService
{
    private readonly BeachDbContext _db;

    public AdminService(BeachDbContext db)
    {
        _db = db;
    }

    public async Task<AdminStatsDto> GetGlobalStatsAsync()
    {
        return new AdminStatsDto
        {
            TotalBeaches = await _db.Beaches.CountAsync(),
            TotalUsers = await _db.BusinessUsers.CountAsync(),
            TotalReservations = await _db.Reservations.CountAsync(),
            PendingBeaches = await _db.Beaches.CountAsync(b => !b.IsActive),
            Revenue = await _db.Reservations
                .Where(r => r.PaymentStatus == PaymentStatus.Paid &&
                            r.Status != ReservationStatus.Cancelled &&
                            r.Status != ReservationStatus.Rejected)
                .SumAsync(r => r.TotalPrice)
        };
    }

    public async Task<List<AdminBeachListItemDto>> GetAllBeachesAsync(int page, int pageSize)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        return await _db.Beaches
            .Select(b => new AdminBeachListItemDto
            {
                Id = b.Id,
                Name = b.Name,
                Location = b.Address,
                ImageUrl = b.CoverImageUrl,
                Capacity = b.Capacity,
                IsActive = b.IsActive,
                Rating = b.Rating,
                InstagramUsername = b.InstagramUsername,
                SocialContentSource = b.SocialContentSource
            })
            .OrderBy(b => b.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AdminUserListItemDto>> GetAllUsersAsync(int page, int pageSize)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        return await _db.BusinessUsers
            .Select(u => new AdminUserListItemDto
            {
                Id = u.Id,
                Email = u.Email,
                ContactName = u.ContactName,
                BusinessName = u.BusinessName,
                Role = u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt
            })
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AdminReservationListItemDto>> GetAllReservationsAsync(int page, int pageSize)
    {
        (page, pageSize) = NormalizePagination(page, pageSize);

        return await _db.Reservations
            .Include(r => r.User)
            .Include(r => r.Beach)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new AdminReservationListItemDto
            {
                Id = r.Id,
                BeachName = r.Beach != null ? r.Beach.Name : "Bilinmiyor",
                CustomerName = r.IsGuest
                    ? (r.GuestFirstName + " " + r.GuestLastName).Trim()
                    : (r.User != null ? (r.User.ContactName ?? r.User.Email) : "Bilinmiyor"),
                Phone = r.IsGuest ? (r.GuestPhone ?? "Bilinmiyor") : "Bilinmiyor",
                ReservationDate = r.ReservationDate,
                ReservationTime = r.ReservationTime,
                PersonCount = r.PersonCount,
                ReservationType = r.ReservationType ?? "Standart",
                Status = r.Status,
                IsGuest = r.IsGuest,
                ConfirmationCode = r.ConfirmationCode,
                CreatedAt = r.CreatedAt
            })
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Beach?> GetBeachByIdAsync(int id)
    {
        return await _db.Beaches.FindAsync(id);
    }

    public async Task<bool> ToggleBeachStatusAsync(int id)
    {
        var beach = await _db.Beaches.FindAsync(id);
        if (beach == null)
        {
            return false;
        }

        beach.IsActive = !beach.IsActive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateBeachAsync(int id, UpdateBeachDto dto)
    {
        var beach = await _db.Beaches.FindAsync(id);
        if (beach == null)
        {
            return false;
        }

        ApplyBeachUpdate(beach, dto);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<AdminBeachImportResultDto> ImportBeachesAsync(List<UpdateBeachDto> beaches)
    {
        var createdCount = 0;

        foreach (var dto in beaches)
        {
            var existing = await _db.Beaches
                .FirstOrDefaultAsync(b => b.Name == dto.Name && b.Address == dto.Address);

            if (existing != null)
            {
                ApplyBeachUpdate(existing, dto);
                continue;
            }

            var beach = new Beach(dto.Name, dto.Description, dto.Address, dto.Latitude, dto.Longitude, 0);
            ApplyBeachUpdate(beach, dto);
            _db.Beaches.Add(beach);
            createdCount++;
        }

        await _db.SaveChangesAsync();

        return new AdminBeachImportResultDto
        {
            CreatedCount = createdCount,
            UpdatedCount = beaches.Count - createdCount
        };
    }

    private static void ApplyBeachUpdate(Beach beach, UpdateBeachDto dto)
    {
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
    }

    private static (int page, int pageSize) NormalizePagination(int page, int pageSize)
    {
        var safePage = page < 1 ? 1 : page;
        var safePageSize = pageSize switch
        {
            < 1 => 50,
            > 200 => 200,
            _ => pageSize
        };

        return (safePage, safePageSize);
    }
}
