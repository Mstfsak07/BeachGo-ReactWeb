using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Models.Enums;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Tests;

public class BusinessServiceTests
{
    [Fact]
    public async Task GetAllReservationsAsync_applies_pagination_in_created_at_desc_order()
    {
        await using var db = CreateDbContext();

        var user = new BusinessUser("owner@example.com", "hash", UserRoles.Business);
        var beach = new Beach("Konyaalti", "desc", "Antalya", 36.0, 30.0, ownerId: 1);
        db.BusinessUsers.Add(user);
        db.Beaches.Add(beach);
        await db.SaveChangesAsync();

        db.Reservations.AddRange(
            CreateReservation(user.Id, beach.Id, DateTime.UtcNow.Date.AddDays(1), ReservationStatus.Pending, 100, createdAt: DateTime.UtcNow.AddMinutes(-30)),
            CreateReservation(user.Id, beach.Id, DateTime.UtcNow.Date.AddDays(2), ReservationStatus.Pending, 150, createdAt: DateTime.UtcNow.AddMinutes(-20)),
            CreateReservation(user.Id, beach.Id, DateTime.UtcNow.Date.AddDays(3), ReservationStatus.Pending, 200, createdAt: DateTime.UtcNow.AddMinutes(-10))
        );
        await db.SaveChangesAsync();

        var service = new BusinessService(db);

        var page1 = await service.GetAllReservationsAsync(beach.Id, page: 1, pageSize: 2);
        var page2 = await service.GetAllReservationsAsync(beach.Id, page: 2, pageSize: 2);

        Assert.Equal(2, page1.Count);
        Assert.Single(page2);
        Assert.True(page1[0].CreatedAt >= page1[1].CreatedAt);
        Assert.True(page1[1].CreatedAt >= page2[0].CreatedAt);
    }

    [Fact]
    public async Task GetStatsAsync_returns_expected_counts_without_querying_Date_on_database_side()
    {
        await using var db = CreateDbContext();

        var user = new BusinessUser("owner@example.com", "hash", UserRoles.Business);
        var beach = new Beach("Konyaalti", "desc", "Antalya", 36.0, 30.0, ownerId: 1);
        db.BusinessUsers.Add(user);
        db.Beaches.Add(beach);
        await db.SaveChangesAsync();

        var now = DateTime.UtcNow;
        var todayStart = new DateTime(now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc);
        var weekStart = todayStart.AddDays(-6);
        var monthStart = new DateTime(todayStart.Year, todayStart.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        db.Reservations.AddRange(
            CreateReservation(user.Id, beach.Id, todayStart.AddHours(9), ReservationStatus.Pending, totalPrice: 100),
            CreateReservation(user.Id, beach.Id, weekStart.AddHours(11), ReservationStatus.Approved, totalPrice: 150),
            CreateReservation(user.Id, beach.Id, monthStart.AddDays(-1).AddHours(12), ReservationStatus.Cancelled, totalPrice: 90),
            CreateReservation(user.Id, beach.Id, todayStart.AddHours(14), ReservationStatus.Rejected, totalPrice: 75, isDeleted: true)
        );
        await db.SaveChangesAsync();

        var service = new BusinessService(db);

        var result = await service.GetStatsAsync(beach.Id);

        Assert.Equal(3, result.TotalReservations);
        Assert.Equal(1, result.TodayCheckins);
        Assert.Equal(2, result.MonthlyReservations);
        Assert.Equal(1, result.ActiveCustomers);
        Assert.Equal(340, result.EstimatedEarnings);
        Assert.Equal(7, result.WeeklyData.Count);
        Assert.Equal(2, result.WeeklyData.Sum(x => x.Count));
    }

    private static Reservation CreateReservation(
        int userId,
        int beachId,
        DateTime reservationDate,
        ReservationStatus status,
        decimal totalPrice,
        bool isDeleted = false,
        DateTime? createdAt = null)
    {
        var reservation = new Reservation
        {
            UserId = userId,
            BeachId = beachId,
            ReservationDate = reservationDate,
            CreatedAt = createdAt ?? DateTime.UtcNow,
            Status = status,
            PersonCount = 2,
            SunbedCount = 1,
            TotalPrice = totalPrice
        };

        if (isDeleted)
        {
            typeof(Reservation)
                .GetProperty(nameof(Reservation.IsDeleted))!
                .SetValue(reservation, true);
        }

        return reservation;
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }
}
