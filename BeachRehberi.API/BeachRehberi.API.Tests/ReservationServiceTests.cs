using BeachRehberi.API.Data;
using BeachRehberi.API.DTOs.Reservation;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace BeachRehberi.API.Tests;

public class ReservationServiceTests
{
    [Fact]
    public async Task CreateAsync_recomputes_total_price_on_server()
    {
        await using var db = CreateDbContext();
        db.Beaches.Add(new Beach("Konyaalti", "desc", "Antalya", 36.0, 30.0, 1)
        {
            HasEntryFee = true,
            EntryFee = 100,
            SunbedPrice = 75
        });
        await db.SaveChangesAsync();

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Features:UseRealPayment"] = "true"
            })
            .Build();

        var service = new ReservationService(db, config);

        var result = await service.CreateAsync(new CreateReservationDto
        {
            BeachId = db.Beaches.Select(x => x.Id).Single(),
            ReservationDate = DateTime.UtcNow.Date.AddDays(1),
            PersonCount = 2,
            SunbedCount = 1,
            TotalPrice = 1
        }, userId: 42);

        Assert.True(result.Success);
        Assert.Equal(275, result.Data?.Id > 0 ? db.Reservations.Single().TotalPrice : 0);
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }
}
