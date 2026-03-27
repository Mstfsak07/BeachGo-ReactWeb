using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

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

    public async Task<Reservation> CreateAsync(Reservation reservation)
    {
        var beach = await _db.Beaches.FindAsync(reservation.BeachId);
        if (beach == null) throw new Exception("Plaj bulunamadı.");

        reservation.TotalPrice = (beach.EntryFee + beach.SunbedPrice) * reservation.PersonCount;

        string newCode;
        bool isUsed;
        do {
            newCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
            isUsed = await _db.Reservations.AnyAsync(r => r.ConfirmationCode == newCode);
        } while (isUsed);

        reservation.ConfirmationCode = newCode;
        reservation.Status = ReservationStatus.Confirmed;
        reservation.CreatedAt = DateTime.UtcNow;

        _db.Reservations.Add(reservation);
        await _db.SaveChangesAsync();

        await _notification.SendToBusinessAsync(
            reservation.BeachId,
            $"Yeni Rezervasyon: {reservation.UserName} ({reservation.PersonCount} Kişi) - Kod: {newCode}");

        return reservation;
    }

    public async Task<List<Reservation>> GetByPhoneAsync(string phone) =>
        await _db.Reservations
            .Include(r => r.Beach)
            .Where(r => r.UserPhone == phone)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();

    public async Task<Reservation?> GetByCodeAsync(string code) =>
        await _db.Reservations
            .Include(r => r.Beach)
            .FirstOrDefaultAsync(r => r.ConfirmationCode == code);

    public async Task<bool> CancelAsync(string code)
    {
        var res = await _db.Reservations.FirstOrDefaultAsync(r => r.ConfirmationCode == code);
        if (res == null) return false;
        res.Status = ReservationStatus.Cancelled;
        await _db.SaveChangesAsync();
        return true;
    }
}