using Mapster;
using MapsterMapper;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.DTOs;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Services;

public class ReservationService : IReservationService
{
    private readonly BeachDbContext _db;
    private readonly INotificationService _notification;
    private readonly IConfirmationCodeGenerator _codeGenerator;
    private readonly MapsterMapper.IMapper _mapper;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(BeachDbContext db, INotificationService notification, IConfirmationCodeGenerator codeGenerator, MapsterMapper.IMapper mapper, ILogger<ReservationService> logger)
    {
        _db = db;
        _notification = notification;
        _codeGenerator = codeGenerator;
        _mapper = mapper;
        _logger = logger;
    }

    private static bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException is SqliteException sqliteEx && sqliteEx.SqliteErrorCode == 19;
    }

    public async Task<ServiceResult<Reservation>> CreateAsync(CreateReservationDto dto, int userId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var beach = await _db.Beaches.AsNoTracking().FirstOrDefaultAsync(x => x.Id == dto.BeachId);
            if (beach == null)
            {
                return ServiceResult<Reservation>.FailureResult("Plaj bulunamadı.");
            }

            var reservation = dto.Adapt<Reservation>();
            reservation.AssignUser(userId);
            reservation.CalculatePrice(beach.EntryFee, beach.SunbedPrice);
            reservation.Confirm();

            int maxRetries = 10;
            bool success = false;

            while (maxRetries > 0)
            {
                try
                {
                    reservation.SetConfirmationCode(_codeGenerator.Generate(8));
                    _db.Reservations.Add(reservation);
                    await _db.SaveChangesAsync();
                    success = true;
                    break;
                }
                catch (DbUpdateException ex) when (maxRetries-- > 0 && IsUniqueConstraintViolation(ex))
                {
                    _db.Entry(reservation).State = EntityState.Detached;
                    _logger.LogWarning("Confirmation code collision. Retrying... Code: {Code}", reservation.ConfirmationCode);
                }
            }

            if (!success)
            {
                await transaction.RollbackAsync();
                return ServiceResult<Reservation>.FailureResult("Rezervasyon kodu oluşturulamadı, lütfen tekrar deneyin.");
            }

            await transaction.CommitAsync();

            _ = _notification.SendToBusinessAsync(
                reservation.BeachId,
                $"Yeni Rezervasyon: {reservation.UserName} ({reservation.PersonCount} Kişi)");

            return ServiceResult<Reservation>.SuccessResult(reservation, "Rezervasyon başarıyla oluşturuldu.");
        }
        catch (DomainException ex)
        {
            await transaction.RollbackAsync();
            return ServiceResult<Reservation>.FailureResult(ex.Message);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error creating reservation");
            return ServiceResult<Reservation>.FailureResult("Rezervasyon sırasında bir hata oluştu.");
        }
    }

    public async Task<List<ReservationListItemDto>> GetByUserAsync(int userId) =>
        await _db.Reservations
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.ReservationDate)
            .ProjectToType<ReservationListItemDto>()
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

        try
        {
            res.Cancel();
            await _db.SaveChangesAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

