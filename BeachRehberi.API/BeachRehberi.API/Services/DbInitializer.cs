using BCrypt.Net;
using BeachRehberi.API.Data;
using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BeachRehberi.API.Services;

public interface IDatabaseInitializationState
{
    Task WaitForInitializationAsync(CancellationToken cancellationToken);
    void MarkSucceeded();
    void MarkFailed(Exception exception);
}

public sealed class DatabaseInitializationState : IDatabaseInitializationState
{
    private readonly TaskCompletionSource _ready = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task WaitForInitializationAsync(CancellationToken cancellationToken)
    {
        return _ready.Task.WaitAsync(cancellationToken);
    }

    public void MarkSucceeded()
    {
        _ready.TrySetResult();
    }

    public void MarkFailed(Exception exception)
    {
        _ready.TrySetException(exception);
    }
}

public sealed class DbInitializer
{
    private const long MigrationLockKey = 680170142399318081;

    private readonly IServiceProvider _services;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<DbInitializer> _logger;
    private readonly IDatabaseInitializationState _state;

    public DbInitializer(
        IServiceProvider services,
        IWebHostEnvironment environment,
        ILogger<DbInitializer> logger,
        IDatabaseInitializationState state)
    {
        _services = services;
        _environment = environment;
        _logger = logger;
        _state = state;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await using var scope = _services.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();

                await using var connection = (NpgsqlConnection)db.Database.GetDbConnection();
                await connection.OpenAsync(cancellationToken);

                await using var lockCommand = new NpgsqlCommand("SELECT pg_advisory_lock(@key);", connection);
                lockCommand.Parameters.AddWithValue("key", MigrationLockKey);
                await lockCommand.ExecuteNonQueryAsync(cancellationToken);

                try
                {
                    var pendingMigrations = (await db.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
                    if (pendingMigrations.Count > 0)
                    {
                        _logger.LogInformation(
                            "Applying {Count} pending EF Core migrations: {Migrations}",
                            pendingMigrations.Count,
                            string.Join(", ", pendingMigrations));

                        await db.Database.MigrateAsync(cancellationToken);
                    }
                    else
                    {
                        _logger.LogInformation("Database is up to date. No EF Core migrations to apply.");
                    }

                    if (_environment.IsDevelopment())
                    {
                        await SeedDevelopmentDataAsync(db, cancellationToken);
                    }

                    _state.MarkSucceeded();
                    return;
                }
                finally
                {
                    await using var unlockCommand = new NpgsqlCommand("SELECT pg_advisory_unlock(@key);", connection);
                    unlockCommand.Parameters.AddWithValue("key", MigrationLockKey);
                    await unlockCommand.ExecuteNonQueryAsync(CancellationToken.None);
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                var canceled = new OperationCanceledException("Database initialization was canceled.", cancellationToken);
                _state.MarkFailed(canceled);
                throw;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                var delay = TimeSpan.FromSeconds(Math.Min(attempt * 5, 20));
                _logger.LogWarning(
                    ex,
                    "Database initialization attempt {Attempt}/{MaxAttempts} failed. Retrying in {DelaySeconds}s.",
                    attempt,
                    maxAttempts,
                    delay.TotalSeconds);

                await Task.Delay(delay, cancellationToken);
            }
            catch (Exception ex)
            {
                _state.MarkFailed(ex);
                _logger.LogCritical(ex, "Database initialization failed after {MaxAttempts} attempts.", maxAttempts);
                throw;
            }
        }
    }

    private static async Task SeedDevelopmentDataAsync(BeachDbContext db, CancellationToken cancellationToken)
    {
        if (!await db.Beaches.AnyAsync(cancellationToken))
        {
            var beaches = new List<Beach>
            {
                new("Konyaalti Plaji", "Antalya'nin en gozde cakil plaji. Berrak mavi sulari ve uzun sahil seridiyle mukemmel bir tatil deneyimi sunar.", "Konyaalti, Antalya", 36.8784, 30.6657, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "08:00", CloseTime = "20:00",
                    HasSunbeds = true, HasShower = true, HasParking = true, HasWifi = true, HasBar = true, IsChildFriendly = true,
                    OccupancyPercent = 65, Capacity = 2000, IsOpen = true, SunbedPrice = 150
                },
                new("Lara Plaji", "Antalya'nin incisi Lara Plaji, ince kum ve turkuaz sulari ile ziyaretcilerini buyuluyor.", "Lara, Antalya", 36.8469, 30.7843, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "07:30", CloseTime = "21:00",
                    HasSunbeds = true, HasRestaurant = true, HasParking = true, HasWaterSports = true, IsChildFriendly = true,
                    OccupancyPercent = 80, Capacity = 3000, IsOpen = true, SunbedPrice = 200, HasEntryFee = true, EntryFee = 50
                },
                new("Mermerli Plaji", "Antalya Kaleici'nde tarihi dokularla cevrili, mermer kayaliklariyla benzersiz bir koy.", "Kaleici, Antalya", 36.8825, 30.7056, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1476673160081-cf065607f449?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "09:00", CloseTime = "19:00",
                    HasSunbeds = true, HasBar = true, HasShower = true,
                    OccupancyPercent = 45, Capacity = 500, IsOpen = true, SunbedPrice = 250, HasEntryFee = true, EntryFee = 100
                },
                new("Adrasan Plaji", "Kumluca'ya bagli sakin ve dogal guzelligini koruyan essiz bir koy.", "Adrasan, Antalya", 36.3451, 30.4712, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "08:00", CloseTime = "19:30",
                    HasShower = true, IsChildFriendly = true,
                    OccupancyPercent = 30, Capacity = 800, IsOpen = true
                },
                new("Phaselis Plaji", "Antik liman kalintilari arasinda tarihe dokunan, uc koylu muhtesem plaj.", "Kemer, Antalya", 36.5204, 30.5549, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1510414842594-a61c69b5ae57?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "08:30", CloseTime = "18:30",
                    HasParking = true, IsChildFriendly = true, HasShower = true,
                    OccupancyPercent = 55, Capacity = 1200, IsOpen = true, HasEntryFee = true, EntryFee = 120
                },
                new("Oludeniz Lagunu", "Turkiye'nin en fotograflanan noktasi, masmavi lagunu ve milli park statusuyle essiz guzellik.", "Oludeniz, Fethiye", 36.5500, 29.1167, 1)
                {
                    CoverImageUrl = "https://images.unsplash.com/photo-1520454974749-611b7248ffdb?auto=format&fit=crop&w=1200&q=80",
                    OpenTime = "07:00", CloseTime = "21:00",
                    HasSunbeds = true, HasBar = true, HasWaterSports = true, HasParking = true, IsChildFriendly = true, HasRestaurant = true, HasWifi = true,
                    OccupancyPercent = 90, Capacity = 5000, IsOpen = true, SunbedPrice = 300, HasEntryFee = true, EntryFee = 75
                }
            };

            var ratingsAndCounts = new[] { (4.6, 287), (4.8, 512), (4.3, 94), (4.9, 156), (4.5, 203), (4.7, 1024) };
            for (var i = 0; i < beaches.Count; i++)
            {
                beaches[i].UpdateRating(ratingsAndCounts[i].Item1, ratingsAndCounts[i].Item2);
            }

            db.Beaches.AddRange(beaches);
            await db.SaveChangesAsync(cancellationToken);
        }

        if (!await db.BusinessUsers.AnyAsync(x => x.Email == "admin@beachgo.com", cancellationToken))
        {
            var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");
            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            var adminUser = new BusinessUser(
                "admin@beachgo.com",
                BCrypt.Net.BCrypt.HashPassword(adminPassword),
                UserRoles.Admin);

            adminUser.UpdateProfile("Admin User", "BeachGo Admin");
            db.BusinessUsers.Add(adminUser);
            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
