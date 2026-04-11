using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using BeachRehberi.API.Services;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Tests;

public class BeachServiceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("a")]
    [InlineData("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
    public async Task SearchAsync_rejects_invalid_query_lengths(string query)
    {
        await using var db = CreateDbContext();
        db.Beaches.Add(new Beach("Konyaalti", "desc", "Antalya", 36.0, 30.0, 1));
        await db.SaveChangesAsync();

        var service = new BeachService(db, new GeoCalculator());

        var result = await service.SearchAsync(query);

        Assert.Empty(result);
    }

    private static BeachDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BeachDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BeachDbContext(options);
    }
}
