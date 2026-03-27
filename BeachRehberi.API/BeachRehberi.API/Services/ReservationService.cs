using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace BeachRehberi.API.Services;

public class ReservationService : IReservationService
{
    private readonly BeachDbContext _db;
    private readonly INotificationService _notification;

    public ReservationService(BeachDbContext db, INotificationService notification)
    {
        _db = db;
        _notification = notification;
    }

    private static readonly char[] ConfirmationChars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private string GenerateAlphanumericCode(int length = 8)
    {
        var codeChars = new char[length];
        for (int i = 0; i < length; i++)
        {
            codeChars[i] = ConfirmationChars[RandomNumberGenerator.GetInt32(ConfirmationChars.Length)];
        }
        return new string(codeChars);
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19;
    }

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        var beach = await _db.Beaches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == reservation.BeachId)      
                    ?? throw new KeyNotFoundException("Plaj bulunamadı.");

        reservation.TotalPrice = (beach.EntryFee + beach.SunbedPrice) * reservation.PersonCount;
        reservation.Status = ReservationStatus.Confirmed;
        reservation.CreatedAt = DateTime.UtcNow;

        int maxRetries = 10;
        while (maxRetries > 0)
        {
            try
            {
                reservation.ConfirmationCode = GenerateAlphanumericCode(8);
                _db.Reservations.Add(reservation);
                await _db.SaveChangesAsync();
                break;
            }
            catch (DbUpdateException ex) when (maxRetries-- > 0 && IsUniqueConstraintViolation(ex))
            {
                _db.Entry(reservation).State = EntityState.Detached;
                if (maxRetries == 0) throw new Exception("Şu an sistemsel bir çakışma yaşanıyor, lütfen tekrar deneyin.");
            }
        }

        await _notification.SendToBusinessAsync(
            reservation.BeachId,
            $"Yeni Rezervasyon: {reservation.UserName} ({reservation.PersonCount} Kişi)");

        return reservation;
    }

    public async Task<List<Reservation>> GetByUserAsync(int userId) =>
        await _db.Reservations
            .AsNoTracking()
            .Include(r => r.Beach)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();

    public async Task<Reservation?> GetByCodeAsync(string code) =>
        await _db.Reservations
            .AsNoTracking()
            .Include(r => r.Beach)
            .FirstOrDefaultAsync(r => r.ConfirmationCode == code);

    public async Task<bool> CancelAsync(string code, int userId)
    {
        var res = await _db.Reservations.FirstOrDefaultAsync(r => r.ConfirmationCode == code);
        if (res == null || res.UserId != userId) return false;

        res.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();
        return true;
    }
}
