using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Models.Enums;

namespace BeachRehberi.API.Services;

public class ReservationService : IReservationService
{
    private readonly BeachDbContext _context;

    public ReservationService(BeachDbContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<Reservation>> CreateAsync(CreateReservationDto dto, int userId)
    {
        if (dto.ReservationDate.Date < DateTime.UtcNow.Date)
            return ServiceResult<Reservation>.FailureResult("Geçmiş bir tarih için rezervasyon yapılamaz.");

        var beach = await _context.Beaches.FirstOrDefaultAsync(b => b.Id == dto.BeachId && !b.IsDeleted);
        if (beach == null)
            return ServiceResult<Reservation>.FailureResult("Plaj bulunamadı.");

        var exists = await _context.Reservations.AnyAsync(r => 
            r.UserId == userId && 
            r.BeachId == dto.BeachId && 
            r.ReservationDate.Date == dto.ReservationDate.Date &&
            !r.IsDeleted &&
            r.Status != ReservationStatus.Cancelled &&
            r.Status != ReservationStatus.Rejected);

        if (exists)
            return ServiceResult<Reservation>.FailureResult("Bu plaj için bu tarihte zaten aktif bir rezervasyonunuz bulunmaktadır.");

        var reservation = new Reservation
        {
            UserId = userId,
            BeachId = dto.BeachId,
            ReservationDate = dto.ReservationDate.Date,
            CreatedAt = DateTime.UtcNow,
            Status = ReservationStatus.Pending
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        return ServiceResult<Reservation>.SuccessResult(reservation, "Rezervasyon başarıyla oluşturuldu.");
    }

    public async Task<List<ReservationListItemDto>> GetByUserAsync(int userId)
    {
        return await _context.Reservations
            .Include(r => r.Beach)
            .Where(r => r.UserId == userId && !r.IsDeleted)
            .OrderByDescending(r => r.ReservationDate)
            .Select(r => new ReservationListItemDto
            {
                Id = r.Id,
                BeachName = r.Beach.Name,
                ReservationDate = r.ReservationDate
            })
            .ToListAsync();
    }

    public async Task<bool> CancelAsync(int id, int userId)
    {
        var reservation = await _context.Reservations.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        
        if (reservation == null || reservation.UserId != userId)
            return false;

        reservation.Cancel();
        await _context.SaveChangesAsync();

        return true;
    }
}
