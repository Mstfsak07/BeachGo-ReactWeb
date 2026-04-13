using BeachRehberi.API.Data;
using BeachRehberi.API.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Testcontainers.PostgreSql;

namespace BeachRehberi.API.Tests;

public sealed class DbInitializerIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer? _postgres;
    private bool _containerAvailable = true;

    public async Task InitializeAsync()
    {
        try
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("postgres")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _postgres.StartAsync();
        }
        catch
        {
            _containerAvailable = false;
        }
    }

    public async Task DisposeAsync()
    {
        if (_postgres is not null)
        {
            await _postgres.DisposeAsync();
        }
    }

    [Fact]
    public async Task InitializeAsync_AppliesMigrations_OnEmptyDatabase()
    {
        if (!_containerAvailable)
            return;

        var databaseName = $"dbinit_{Guid.NewGuid():N}";
        await CreateDatabaseAsync(databaseName);
        var connectionString = BuildDatabaseConnectionString(databaseName);

        await using var provider = BuildServiceProvider(connectionString);
        var initializer = provider.GetRequiredService<DbInitializer>();

        await initializer.InitializeAsync(CancellationToken.None);

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();

        var appliedMigrations = await db.Database.GetAppliedMigrationsAsync();
        Assert.NotEmpty(appliedMigrations);

        var revokedTokenTableExists = await db.Database.ExecuteSqlRawAsync(
            """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1
                    FROM information_schema.tables
                    WHERE table_schema = 'public'
                      AND table_name = 'RevokedTokens'
                ) THEN
                    RAISE EXCEPTION 'RevokedTokens table is missing';
                END IF;
            END $$;
            """);

        Assert.Equal(0, revokedTokenTableExists);
    }

    [Fact]
    public async Task InitializeAsync_IsIdempotent_WhenDatabaseIsAlreadyMigrated()
    {
        if (!_containerAvailable)
            return;

        var databaseName = $"dbinit_{Guid.NewGuid():N}";
        await CreateDatabaseAsync(databaseName);
        var connectionString = BuildDatabaseConnectionString(databaseName);

        await using var provider = BuildServiceProvider(connectionString);
        var initializer = provider.GetRequiredService<DbInitializer>();

        await initializer.InitializeAsync(CancellationToken.None);
        await initializer.InitializeAsync(CancellationToken.None);

        await using var scope = provider.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<BeachDbContext>();
        var pendingMigrations = await db.Database.GetPendingMigrationsAsync();

        Assert.Empty(pendingMigrations);
    }

    private async Task CreateDatabaseAsync(string databaseName)
    {
        var postgres = AssertContainer();
        await using var connection = new NpgsqlConnection(postgres.GetConnectionString());
        await connection.OpenAsync();

        await using var command = connection.CreateCommand();
        command.CommandText = $"CREATE DATABASE \"{databaseName}\";";
        await command.ExecuteNonQueryAsync();
    }

    private string BuildDatabaseConnectionString(string databaseName)
    {
        var postgres = AssertContainer();
        var builder = new NpgsqlConnectionStringBuilder(postgres.GetConnectionString())
        {
            Database = databaseName
        };

        return builder.ConnectionString;
    }

    private PostgreSqlContainer AssertContainer()
    {
        return Assert.IsType<PostgreSqlContainer>(_postgres);
    }

    private static ServiceProvider BuildServiceProvider(string connectionString)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<BeachDbContext>(options => options.UseNpgsql(connectionString));
        services.AddSingleton<IWebHostEnvironment>(new TestWebHostEnvironment());
        services.AddSingleton<IDatabaseInitializationState, DatabaseInitializationState>();
        services.AddSingleton<DbInitializer>();

        return services.BuildServiceProvider();
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "BeachRehberi.API.Tests";
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string WebRootPath { get; set; } = string.Empty;
        public string EnvironmentName { get; set; } = "Production";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
