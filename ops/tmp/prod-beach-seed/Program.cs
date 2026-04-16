using System.Text.Json;
using Npgsql;

var workspaceRoot = FindWorkspaceRoot(AppContext.BaseDirectory);
var importJsonPath = Path.Combine(workspaceRoot, "ops", "data", "beachs-import-20260416.json");

var beaches = JsonSerializer.Deserialize<List<BeachSeed>>(await File.ReadAllTextAsync(importJsonPath), new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true
}) ?? throw new InvalidOperationException("Import JSON could not be parsed.");

var storySeeds = BuildStories();
var extraFields = BuildExtraFields();
var connectionString = Environment.GetEnvironmentVariable("BEACHGO_PROXY_CONN")
    ?? throw new InvalidOperationException("BEACHGO_PROXY_CONN env var is required.");

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();
await using var transaction = await connection.BeginTransactionAsync();

var now = DateTime.UtcNow;
var expireAt = now.AddDays(365);
var beachIds = new Dictionary<string, int>(StringComparer.Ordinal);

foreach (var beach in beaches)
{
    if (!extraFields.TryGetValue(beach.Name, out var extra))
    {
        throw new InvalidOperationException($"Missing extra field config for {beach.Name}");
    }

    var existingId = await FindBeachIdAsync(connection, transaction, beach.Name);
    if (existingId is null)
    {
        beachIds[beach.Name] = await InsertBeachAsync(connection, transaction, beach, extra, now);
    }
    else
    {
        await UpdateBeachAsync(connection, transaction, existingId.Value, beach, extra, now);
        beachIds[beach.Name] = existingId.Value;
    }
}

foreach (var (beachName, stories) in storySeeds)
{
    if (!beachIds.TryGetValue(beachName, out var beachId))
    {
        throw new InvalidOperationException($"Beach id was not resolved for {beachName}");
    }

    await ArchiveStoriesAsync(connection, transaction, beachId);
    foreach (var story in stories)
    {
        await InsertStoryAsync(connection, transaction, beachId, story, now, expireAt);
    }
}

await transaction.CommitAsync();

Console.WriteLine("Upsert complete:");
foreach (var beach in beaches)
{
    Console.WriteLine($"{beachIds[beach.Name]}|{beach.Name}");
}

static string FindWorkspaceRoot(string startPath)
{
    var directory = new DirectoryInfo(startPath);
    while (directory is not null)
    {
        if (File.Exists(Path.Combine(directory.FullName, "AI_HANDOFF.md")))
        {
            return directory.FullName;
        }

        directory = directory.Parent;
    }

    throw new InvalidOperationException("Workspace root could not be found.");
}

static async Task<int?> FindBeachIdAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, string name)
{
    await using var command = new NpgsqlCommand("""
        select "Id"
        from "Beaches"
        where "Name" = @name
        limit 1
        """, connection, transaction);
    command.Parameters.AddWithValue("name", name);
    var result = await command.ExecuteScalarAsync();
    return result is null ? null : Convert.ToInt32(result);
}

static async Task<int> InsertBeachAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, BeachSeed beach, BeachExtra extra, DateTime now)
{
    await using var command = BuildBeachCommand("""
        insert into "Beaches" (
            "Name", "Description", "Address", "Phone", "Website", "Instagram", "InstagramUsername", "SocialContentSource",
            "OpenTime", "CloseTime", "HasEntryFee", "EntryFee", "SunbedPrice", "Latitude", "Longitude", "Rating", "ReviewCount",
            "GooglePlaceId", "CoverImageUrl", "HasSunbeds", "HasShower", "HasParking", "HasRestaurant", "HasBar", "HasAlcohol",
            "IsChildFriendly", "HasWaterSports", "HasWifi", "HasPool", "HasDJ", "HasAccessibility", "OccupancyPercent",
            "OccupancyLevel", "LastUpdated", "IsOpen", "TodaySpecial", "IsActive", "OwnerId", "Capacity", "IsDeleted"
        ) values (
            @Name, @Description, @Address, @Phone, @Website, @Instagram, @InstagramUsername, @SocialContentSource,
            @OpenTime, @CloseTime, @HasEntryFee, @EntryFee, @SunbedPrice, @Latitude, @Longitude, @Rating, @ReviewCount,
            @GooglePlaceId, @CoverImageUrl, @HasSunbeds, @HasShower, @HasParking, @HasRestaurant, @HasBar, @HasAlcohol,
            @IsChildFriendly, @HasWaterSports, @HasWifi, @HasPool, @HasDJ, @HasAccessibility, @OccupancyPercent,
            @OccupancyLevel, @LastUpdated, @IsOpen, @TodaySpecial, @IsActive, @OwnerId, @Capacity, @IsDeleted
        )
        returning "Id"
        """, connection, transaction, beach, extra, now);

    return Convert.ToInt32(await command.ExecuteScalarAsync());
}

static async Task UpdateBeachAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int beachId, BeachSeed beach, BeachExtra extra, DateTime now)
{
    await using var command = BuildBeachCommand("""
        update "Beaches"
        set
            "Description" = @Description,
            "Address" = @Address,
            "Phone" = @Phone,
            "Website" = @Website,
            "Instagram" = @Instagram,
            "InstagramUsername" = @InstagramUsername,
            "SocialContentSource" = @SocialContentSource,
            "OpenTime" = @OpenTime,
            "CloseTime" = @CloseTime,
            "HasEntryFee" = @HasEntryFee,
            "EntryFee" = @EntryFee,
            "SunbedPrice" = @SunbedPrice,
            "Latitude" = @Latitude,
            "Longitude" = @Longitude,
            "Rating" = @Rating,
            "ReviewCount" = @ReviewCount,
            "GooglePlaceId" = @GooglePlaceId,
            "CoverImageUrl" = @CoverImageUrl,
            "HasSunbeds" = @HasSunbeds,
            "HasShower" = @HasShower,
            "HasParking" = @HasParking,
            "HasRestaurant" = @HasRestaurant,
            "HasBar" = @HasBar,
            "HasAlcohol" = @HasAlcohol,
            "IsChildFriendly" = @IsChildFriendly,
            "HasWaterSports" = @HasWaterSports,
            "HasWifi" = @HasWifi,
            "HasPool" = @HasPool,
            "HasDJ" = @HasDJ,
            "HasAccessibility" = @HasAccessibility,
            "OccupancyPercent" = @OccupancyPercent,
            "OccupancyLevel" = @OccupancyLevel,
            "LastUpdated" = @LastUpdated,
            "IsOpen" = @IsOpen,
            "TodaySpecial" = @TodaySpecial,
            "IsActive" = @IsActive,
            "OwnerId" = @OwnerId,
            "Capacity" = @Capacity,
            "IsDeleted" = @IsDeleted
        where "Id" = @BeachId
        """, connection, transaction, beach, extra, now);
    command.Parameters.AddWithValue("BeachId", beachId);
    await command.ExecuteNonQueryAsync();
}

static NpgsqlCommand BuildBeachCommand(string sql, NpgsqlConnection connection, NpgsqlTransaction transaction, BeachSeed beach, BeachExtra extra, DateTime now)
{
    var command = new NpgsqlCommand(sql, connection, transaction);
    command.Parameters.AddWithValue("Name", beach.Name);
    command.Parameters.AddWithValue("Description", beach.Description);
    command.Parameters.AddWithValue("Address", beach.Address);
    command.Parameters.AddWithValue("Phone", beach.Phone);
    command.Parameters.AddWithValue("Website", beach.Website);
    command.Parameters.AddWithValue("Instagram", beach.Instagram);
    command.Parameters.AddWithValue("InstagramUsername", beach.InstagramUsername);
    command.Parameters.AddWithValue("SocialContentSource", beach.SocialContentSource);
    command.Parameters.AddWithValue("OpenTime", beach.OpenTime);
    command.Parameters.AddWithValue("CloseTime", beach.CloseTime);
    command.Parameters.AddWithValue("HasEntryFee", beach.HasEntryFee);
    command.Parameters.AddWithValue("EntryFee", beach.EntryFee);
    command.Parameters.AddWithValue("SunbedPrice", beach.SunbedPrice);
    command.Parameters.AddWithValue("Latitude", beach.Latitude);
    command.Parameters.AddWithValue("Longitude", beach.Longitude);
    command.Parameters.AddWithValue("Rating", extra.Rating);
    command.Parameters.AddWithValue("ReviewCount", extra.ReviewCount);
    command.Parameters.AddWithValue("GooglePlaceId", extra.GooglePlaceId);
    command.Parameters.AddWithValue("CoverImageUrl", extra.CoverImageUrl);
    command.Parameters.AddWithValue("HasSunbeds", beach.HasSunbeds);
    command.Parameters.AddWithValue("HasShower", beach.HasShower);
    command.Parameters.AddWithValue("HasParking", beach.HasParking);
    command.Parameters.AddWithValue("HasRestaurant", beach.HasRestaurant);
    command.Parameters.AddWithValue("HasBar", beach.HasBar);
    command.Parameters.AddWithValue("HasAlcohol", beach.HasAlcohol);
    command.Parameters.AddWithValue("IsChildFriendly", beach.IsChildFriendly);
    command.Parameters.AddWithValue("HasWaterSports", beach.HasWaterSports);
    command.Parameters.AddWithValue("HasWifi", beach.HasWifi);
    command.Parameters.AddWithValue("HasPool", beach.HasPool);
    command.Parameters.AddWithValue("HasDJ", beach.HasDJ);
    command.Parameters.AddWithValue("HasAccessibility", beach.HasAccessibility);
    command.Parameters.AddWithValue("OccupancyPercent", 0);
    command.Parameters.AddWithValue("OccupancyLevel", 1);
    command.Parameters.AddWithValue("LastUpdated", now);
    command.Parameters.AddWithValue("IsOpen", true);
    command.Parameters.AddWithValue("TodaySpecial", beach.TodaySpecial);
    command.Parameters.AddWithValue("IsActive", true);
    command.Parameters.AddWithValue("OwnerId", 0);
    command.Parameters.AddWithValue("Capacity", beach.Capacity);
    command.Parameters.AddWithValue("IsDeleted", false);
    return command;
}

static async Task ArchiveStoriesAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int beachId)
{
    await using var command = new NpgsqlCommand("""
        update "BeachStories"
        set "IsActive" = false, "IsArchived" = true
        where "BeachId" = @BeachId and "IsActive" = true and "IsArchived" = false
        """, connection, transaction);
    command.Parameters.AddWithValue("BeachId", beachId);
    await command.ExecuteNonQueryAsync();
}

static async Task InsertStoryAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, int beachId, StorySeed story, DateTime now, DateTime expireAt)
{
    await using var command = new NpgsqlCommand("""
        insert into "BeachStories" (
            "BeachId", "PhotoUrl", "VideoUrl", "Caption", "StoryType", "CreatedAt", "ExpireDate", "IsActive", "IsArchived"
        ) values (
            @BeachId, @PhotoUrl, @VideoUrl, @Caption, @StoryType, @CreatedAt, @ExpireDate, @IsActive, @IsArchived
        )
        """, connection, transaction);
    command.Parameters.AddWithValue("BeachId", beachId);
    command.Parameters.AddWithValue("PhotoUrl", story.PhotoUrl);
    command.Parameters.AddWithValue("VideoUrl", DBNull.Value);
    command.Parameters.AddWithValue("Caption", story.Caption);
    command.Parameters.AddWithValue("StoryType", "photo");
    command.Parameters.AddWithValue("CreatedAt", now);
    command.Parameters.AddWithValue("ExpireDate", expireAt);
    command.Parameters.AddWithValue("IsActive", true);
    command.Parameters.AddWithValue("IsArchived", false);
    await command.ExecuteNonQueryAsync();
}

static Dictionary<string, BeachExtra> BuildExtraFields() => new(StringComparer.Ordinal)
{
    ["Flamingo Lounge"] = new("ChIJt6EnCuGRwxQRzOJCJWUBnBU", "https://www.flamingoloungebeach.com/wp-content/uploads/2025/05/Flamingo_0006_Generative-Fill-8.png", 4.2, 1939),
    ["Roxy Beach Lounge Antalya"] = new("ChIJjxyCDpWRwxQRkaHDzpSqOsM", "https://www.roxybeachclubantalya.com/images/roxy-sld-2.jpg", 4.5, 2238),
    ["Sunshine Beach"] = new("ChIJo0ibX0GRwxQRmp3ywLZoEtA", "https://lh3.googleusercontent.com/place-photos/AJRVUZOCXTtAos68ZD1Li5y3ChKUM50iWeTXUtyR1dbWMPapYwWb-s6B2czZd1WXNCWZiFVPZEltLfYJUxDhZL6CjZmRNgvfYRPCcRVDSg10P336_9YU2PDVF-R9jV961tr3GQHQ9fYKG-8ZzbY1pQ=s4800-w1600", 4.6, 2820),
    ["Twenty Beach & Bistro"] = new("ChIJh2LjdWWRwxQRV_wjhwY1eJM", "https://lh3.googleusercontent.com/places/ANXAkqHw76kG0JJzxeGIsBZmhpmQ5f2pUDfJqeFMHg63PDfr92NuMcskr7BoJSpyG6dVyo3e4EuQbYQaBZD6_LUQbCmOwG1L9xXeajc=s4800-w1600", 4.6, 708),
    ["Dubai Beach Konyaalti"] = new("ChIJMw2QCEGTwxQRNt477QBDUmE", "https://lh3.googleusercontent.com/place-photos/AJRVUZMiHJbtfLB_VPL1cut7mY-s3uFhv2aHVuh1OXn28IsI70xo-SXV5WOCcVe-c43SOdEZ8NCV6KieiovY5gy35LcsGJae779tzi0v4YwdAWoN0mft_5tMj6dCmTrwXHBnAuT7OOI-kfSlAZZW3w=s4800-w1600", 4.3, 1775),
    ["La Bohem Beach"] = new("ChIJs3aOBpORwxQR4LSbVwagHI8", "https://labohembeach.com/_next/image?url=%2Fimages%2Fplaj-banner.jpeg&w=3840&q=75", 4.3, 547)
};

static Dictionary<string, List<StorySeed>> BuildStories() => new(StringComparer.Ordinal)
{
    ["Flamingo Lounge"] =
    [
        new("https://www.flamingoloungebeach.com/wp-content/uploads/2016/10/24-scaled.webp", "Flamingo sahilde gun boyu lounge ritmi"),
        new("https://www.flamingoloungebeach.com/wp-content/uploads/2016/10/20-scaled.webp", "Beach park cizgisinde ferah deniz gunu"),
        new("https://www.flamingoloungebeach.com/wp-content/uploads/2016/10/7-scaled.webp", "Flamingo gun batimi ve sahil atmosferi")
    ],
    ["Roxy Beach Lounge Antalya"] =
    [
        new("https://www.roxybeachclubantalya.com/images/roxy-sld-2.jpg", "Roxy beach club ve gun batimi enerjisi"),
        new("https://www.roxybeachclubantalya.com/images/sahil.png", "Sahil yasam parkinda beach lounge deneyimi"),
        new("https://www.roxybeachclubantalya.com/images/product-3.png", "Roxy restoran ve cocktail bar atmosferi")
    ],
    ["Sunshine Beach"] =
    [
        new("https://lh3.googleusercontent.com/place-photos/AJRVUZOCXTtAos68ZD1Li5y3ChKUM50iWeTXUtyR1dbWMPapYwWb-s6B2czZd1WXNCWZiFVPZEltLfYJUxDhZL6CjZmRNgvfYRPCcRVDSg10P336_9YU2PDVF-R9jV961tr3GQHQ9fYKG-8ZzbY1pQ=s4800-w1600", "Sunshine Beach sahil ve deniz cizgisi"),
        new("https://lh3.googleusercontent.com/place-photos/AJRVUZMGyzwOIgWR2FZPcmDUga5fl-I7l9nZs-mqA6FWtLu0gYj-vheGoZDWTPmNUQGSuW2VD9HhyT5518GuPXlpdPZchx69fupAg1DLKHL63xp0D9wWQqmlMtXclMIE7IsTvYMw7zuvx_niUhMW-w=s4800-w1600", "Sunshine Beach genis sahil alani"),
        new("https://lh3.googleusercontent.com/place-photos/AJRVUZMoH1FTlDWYV1-WxhQpBNnpcq4c4zBDJqM3YDM23gi7pnLuA3L0Z9eg8KY52QSpBNxjUq87yr9ZR4XacK2CwKJyYv3eRUfAy_civjeg36Vy8DJXbM9Z_DQdMGQhbSnhpJWkB8lNv0gQetpv0Gb0gB2-=s4800-w1600", "Sunshine Beach gunluk beach park akisi")
    ],
    ["Twenty Beach & Bistro"] =
    [
        new("https://lh3.googleusercontent.com/places/ANXAkqHw76kG0JJzxeGIsBZmhpmQ5f2pUDfJqeFMHg63PDfr92NuMcskr7BoJSpyG6dVyo3e4EuQbYQaBZD6_LUQbCmOwG1L9xXeajc=s4800-w1600", "Twenty Beach ana sahil gorunumu"),
        new("https://lh3.googleusercontent.com/places/ANXAkqGXwDIxbzywvpHJTobNDC1vIbYiWKNO03kq3gYfeHanE9dVjjDYcgiTNzA4JzscY6AdmrkzVH90N5FvdQzPRcEnaSa-_cqvvfI=s4800-w1600", "Twenty Beach social lounge alani"),
        new("https://lh3.googleusercontent.com/place-photos/AJRVUZOV3_8OQhHsfUnfSHLpU74K087i17loMXZeBAL67H-2WAh3A9BIWqn38H9SoOQsd8GUIkzumQbxExXZkhteOjlWU0TkMTUD6GzqkvTy6JPve2aH2isJBOHduIQLIVJxtBA0mP0_Ny90BAqWxb-sYgo7=s4800-w1600", "Twenty Beach deniz kenari beach bistro akisi")
    ],
    ["Dubai Beach Konyaalti"] =
    [
        new("https://lh3.googleusercontent.com/place-photos/AJRVUZMiHJbtfLB_VPL1cut7mY-s3uFhv2aHVuh1OXn28IsI70xo-SXV5WOCcVe-c43SOdEZ8NCV6KieiovY5gy35LcsGJae779tzi0v4YwdAWoN0mft_5tMj6dCmTrwXHBnAuT7OOI-kfSlAZZW3w=s4800-w1600", "Dubai Beach Konyaalti sahil gorunumu"),
        new("https://lh3.googleusercontent.com/place-photos/AJRVUZMWC48Vzp5yv9EwG_e7Ym6qGcaVBXBCnIIkayQ0OoQDVuAoMkCc7Ue6g4eXUgY4AiVJif5fTgXQRC448DiaWEi7nTt1fc-_UaxUfKood5kul-mZBYJ8Bq1YH-yxcot7GuVa2XsdRKB9e3DJDw=s4800-w1600", "Dubai Beach restoran ve sahil deneyimi"),
        new("https://lh3.googleusercontent.com/place-photos/AJRVUZPjsHF7q2gNk_jgIUS1e4J78g3ldOcZN6utRX1cnq5pRHO_AgqpZg6xNf__RVcRgiCpfrf3biz582whMt1SQZI7JSUNuwczJBJ6dLbRZefi8IdDMPhlQhN7TlH90I6t6sAzON4hSrKoCd5JXossDSciKA=s4800-w1600", "Dubai Beach geceye uzayan lounge enerjisi")
    ],
    ["La Bohem Beach"] =
    [
        new("https://labohembeach.com/_next/image?url=%2Fimages%2Fplaj.jpeg&w=3840&q=75", "La Bohem sahilde bohem gun deneyimi"),
        new("https://labohembeach.com/_next/image?url=%2Fimages%2Frestaruant.jpeg&w=3840&q=75", "La Bohem restoran ve sahil keyfi")
    ]
};

public sealed record BeachExtra(string GooglePlaceId, string CoverImageUrl, double Rating, int ReviewCount);
public sealed record StorySeed(string PhotoUrl, string Caption);

public sealed class BeachSeed
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Address { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Website { get; set; } = "";
    public string Instagram { get; set; } = "";
    public string InstagramUsername { get; set; } = "";
    public string SocialContentSource { get; set; } = "";
    public string OpenTime { get; set; } = "";
    public string CloseTime { get; set; } = "";
    public bool HasEntryFee { get; set; }
    public decimal EntryFee { get; set; }
    public decimal SunbedPrice { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public int Capacity { get; set; }
    public bool HasSunbeds { get; set; }
    public bool HasShower { get; set; }
    public bool HasParking { get; set; }
    public bool HasRestaurant { get; set; }
    public bool HasBar { get; set; }
    public bool HasAlcohol { get; set; }
    public bool IsChildFriendly { get; set; }
    public bool HasWaterSports { get; set; }
    public bool HasWifi { get; set; }
    public bool HasPool { get; set; }
    public bool HasDJ { get; set; }
    public bool HasAccessibility { get; set; }
    public string TodaySpecial { get; set; } = "";
}
