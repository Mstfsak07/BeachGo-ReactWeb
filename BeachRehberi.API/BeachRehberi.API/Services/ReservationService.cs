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
        // 1. Ücret Hesaplama (Security: İstemciye güvenme)
        var beach = await _db.Beaches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == reservation.BeachId)
                    ?? throw new KeyNotFoundException("Plaj bulunamadı.");

        reservation.TotalPrice = (beach.EntryFee + beach.SunbedPrice) * reservation.PersonCount;
        reservation.Status = ReservationStatus.Confirmed;
        reservation.CreatedAt = DateTime.UtcNow;

        // 2. Retry Pattern for Unique Confirmation Code
        int maxRetries = 3;
        while (maxRetries > 0)
        {
            try
            {
                reservation.ConfirmationCode = Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
                _db.Reservations.Add(reservation);
                await _db.SaveChangesAsync();
                break;
            }
            catch (DbUpdateException) when (maxRetries-- > 0)
            {
                _db.Entry(reservation).State = EntityState.Detached; // Reset tracking for retry
                if (maxRetries == 0) throw new Exception("Şu an sistemsel bir çakışma yaşanıyor, lütfen tekrar deneyin.");
            }
        }

        // 3. Bildirim Gönder (Aynı kaldı)
        await _notification.SendToBusinessAsync(
            reservation.BeachId,
            $"Yeni Rezervasyon: {reservation.UserName} ({reservation.PersonCount} Kişi)");

        return reservation;
    }

    public async Task<List<Reservation>> GetByPhoneAsync(string phone) =>
        await _db.Reservations
            .AsNoTracking()
            .Include(r => r.Beach)
            .Where(r => r.UserPhone == phone)
            .OrderByDescending(r => r.ReservationDate)
            .ToListAsync();

    public async Task<Reservation?> GetByCodeAsync(string code) =>
        await _db.Reservations
            .AsNoTracking()
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