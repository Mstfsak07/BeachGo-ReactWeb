using BeachRehberi.API.Models;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.API.Data;

public class BeachDbContext : DbContext
{
    public BeachDbContext(DbContextOptions<BeachDbContext> options) : base(options) { }

    public DbSet<Beach> Beaches => Set<Beach>();
    public DbSet<BeachEvent> Events => Set<BeachEvent>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BeachPhoto> Photos => Set<BeachPhoto>();
    public DbSet<BusinessUser> BusinessUsers => Set<BusinessUser>();


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.ConfigureWarnings(w =>
            w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Beach
        modelBuilder.Entity<Beach>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Name).IsRequired().HasMaxLength(200);
            e.Property(x => x.EntryFee).HasColumnType("decimal(10,2)");
            e.Property(x => x.SunbedPrice).HasColumnType("decimal(10,2)");
            e.Property(x => x.Rating).HasColumnType("double precision");
        });

        // BeachEvent
        modelBuilder.Entity<BeachEvent>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TicketPrice).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Beach)
             .WithMany(x => x.Events)
             .HasForeignKey(x => x.BeachId)
             .OnDelete(DeleteBehavior.Cascade);
            e.Ignore(x => x.IsFree);
        });

        // Reservation
        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.TotalPrice).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Beach)
             .WithMany(x => x.Reservations)
             .HasForeignKey(x => x.BeachId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Review
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Beach)
             .WithMany(x => x.Reviews)
             .HasForeignKey(x => x.BeachId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // BeachPhoto
        modelBuilder.Entity<BeachPhoto>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Beach)
             .WithMany(x => x.Photos)
             .HasForeignKey(x => x.BeachId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // BusinessUser
        modelBuilder.Entity<BusinessUser>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Email).IsRequired().HasMaxLength(200);
            e.HasOne(x => x.Beach)
             .WithMany()
             .HasForeignKey(x => x.BeachId)
             .OnDelete(DeleteBehavior.Cascade);
        });
       

        // Seed Data - Konyaaltı Plajları
        modelBuilder.Entity<Beach>().HasData(
            new Beach
            {
                Id = 4,
                Name = "TEST BEACH API",
                Description = "API testi.",
                Address = "Test",
                OpenTime = "08:00",
                CloseTime = "20:00",
                HasEntryFee = false,
                EntryFee = 0,
                SunbedPrice = 0,
                Latitude = 36.87,
                Longitude = 30.66,
                Rating = 5.0,
                ReviewCount = 1,
                CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800",
                IsOpen = true,
                OccupancyLevel = OccupancyLevel.Low,
                OccupancyPercent = 10
            },
            new Beach
            {
                Id = 1,
                Name = "Kalypso Beach Club",
                Description = "Konyaaltı'nın en popüler beach club'larından.",
                Address = "Meltem, Beach Park No:10, Muratpaşa/Antalya",
                Phone = "+90 530 783 71 20",
                Instagram = "@kalypsobeach",
                OpenTime = "08:30",
                CloseTime = "01:00",
                HasEntryFee = false,
                EntryFee = 0,
                SunbedPrice = 400,
                Latitude = 36.8785811,
                Longitude = 30.6656502,
                Rating = 4.6,
                ReviewCount = 2741,
                GooglePlaceId = "ChIJeWMyN6yRwxQRGQAjkCC3kCs",
                HasSunbeds = true,
                HasShower = true,
                HasParking = true,
                HasRestaurant = true,
                HasBar = true,
                HasAlcohol = true,
                IsChildFriendly = false,
                HasWaterSports = true,
                HasWifi = true,
                HasPool = false,
                HasDJ = true,
                OccupancyPercent = 60,
                OccupancyLevel = OccupancyLevel.Medium,
                IsOpen = true,
                CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800"
            },


            new Beach
            {
                Id = 2,
                Name = "Sunshine Beach",
                Description = "Gece gündüz açık, canlı müzikli plaj.",
                Address = "Kuşkavağı, Akdeniz Blv. No:17, Konyaaltı/Antalya",
                Phone = "+90 530 345 92 85",
                Instagram = "@sunshinebeachantalya",
                OpenTime = "00:00",
                CloseTime = "23:59",
                HasEntryFee = false,
                EntryFee = 0,
                SunbedPrice = 300,
                Latitude = 36.8688414,
                Longitude = 30.6497255,
                Rating = 4.6,
                ReviewCount = 2814,
                GooglePlaceId = "ChIJo0ibX0GRwxQRmp3ywLZoEtA",
                HasSunbeds = true,
                HasShower = true,
                HasParking = true,
                HasRestaurant = true,
                HasBar = true,
                HasAlcohol = true,
                IsChildFriendly = true,
                HasWaterSports = false,
                HasWifi = true,
                HasPool = false,
                HasDJ = true,
                OccupancyPercent = 45,
                OccupancyLevel = OccupancyLevel.Low,
                IsOpen = true,
                CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800"
            },
            new Beach
            {
                Id = 3,
                Name = "Roxy Beach Lounge",
                Description = "Şık tasarım, premium hizmet.",
                Address = "Meltem, Akdeniz Blv. No:5, Muratpaşa/Antalya",
                Phone = "+90 532 489 65 05",
                Instagram = "@roxybeachantalya",
                OpenTime = "08:00",
                CloseTime = "01:00",
                HasEntryFee = false,
                EntryFee = 0,
                SunbedPrice = 400,
                Latitude = 36.8816754,
                Longitude = 30.6724529,
                Rating = 4.5,
                ReviewCount = 2179,
                GooglePlaceId = "ChIJjxyCDpWRwxQRkaHDzpSqOsM",
                HasSunbeds = true,
                HasShower = true,
                HasParking = true,
                HasRestaurant = true,
                HasBar = true,
                HasAlcohol = true,
                IsChildFriendly = false,
                HasWaterSports = true,
                HasWifi = true,
                HasPool = false,
                HasDJ = true,
                OccupancyPercent = 80,
                OccupancyLevel = OccupancyLevel.High,
                IsOpen = true,
                CoverImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800"
            },
            new Beach
            {
                Id = 5,
                Name = "Flamingo Lounge",
                Description = "Canlı müzik ve kokteyllerle sahil keyfi.",
                Address = "Sahil Yaşam Parkı No:7, Konyaaltı/Antalya",
                Phone = "+90 555 053 13 36",
                OpenTime = "09:00",
                CloseTime = "02:00",
                HasEntryFee = false,
                EntryFee = 0,
                SunbedPrice = 200,
                Latitude = 36.8805096,
                Longitude = 30.6699175,
                Rating = 4.2,
                ReviewCount = 850,
                HasSunbeds = true,
                HasShower = true,
                HasParking = true,
                HasRestaurant = true,
                HasBar = true,
                HasAlcohol = true,
                IsChildFriendly = false,
                HasWaterSports = false,
                HasWifi = true,
                HasPool = false,
                HasDJ = true,
                OccupancyPercent = 50,
                OccupancyLevel = OccupancyLevel.Medium,
                IsOpen = true,
                CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800"
            },
new Beach
{
    Id = 6,
    Name = "Aydın Beach Club",
    Description = "1999'dan beri Konyaaltı'nın köklü plajlarından, aile dostu.",
    Address = "Konyaaltı Beachpark, Konyaaltı/Antalya",
    OpenTime = "08:00",
    CloseTime = "20:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 150,
    Latitude = 36.8750,
    Longitude = 30.6580,
    Rating = 4.5,
    ReviewCount = 1200,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = false,
    IsChildFriendly = true,
    HasWaterSports = true,
    HasWifi = false,
    HasPool = false,
    HasDJ = false,
    OccupancyPercent = 40,
    OccupancyLevel = OccupancyLevel.Low,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800"
},
new Beach
{
    Id = 7,
    Name = "Lucky 13 Beach Restaurant",
    Description = "Burger, pizza ve frozen içeceklerle sahil keyfi.",
    Address = "Arapsuyu, Akdeniz Bulv. No:64, Konyaaltı/Antalya",
    Phone = "0542 408 07 87",
    OpenTime = "08:00",
    CloseTime = "19:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 100,
    Latitude = 36.8681,
    Longitude = 30.6498,
    Rating = 4.0,
    ReviewCount = 430,
    HasSunbeds = true,
    HasShower = false,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = false,
    IsChildFriendly = true,
    HasWaterSports = false,
    HasWifi = false,
    HasPool = false,
    HasDJ = false,
    OccupancyPercent = 30,
    OccupancyLevel = OccupancyLevel.Low,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800"
},
new Beach
{
    Id = 8,
    Name = "Ferma Beach",
    Description = "Canlı müzik ve deniz ürünleriyle gece hayatı.",
    Address = "Akdeniz Bulvarı No:15, Konyaaltı/Antalya",
    OpenTime = "12:00",
    CloseTime = "01:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 250,
    Latitude = 36.8800,
    Longitude = 30.6650,
    Rating = 4.1,
    ReviewCount = 620,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = false,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 55,
    OccupancyLevel = OccupancyLevel.Medium,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800"
},
new Beach
{
    Id = 9,
    Name = "Twenty Beach & Bistro",
    Description = "Sunset manzarasıyla bistro yemekleri ve şezlong keyfi.",
    Address = "Kuşkavağı Mah., Akdeniz Bulv. No:25/1, Konyaaltı/Antalya",
    OpenTime = "11:00",
    CloseTime = "02:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 200,
    Latitude = 36.8660223,
    Longitude = 30.6452678,
    Rating = 4.5,
    ReviewCount = 980,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = true,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 45,
    OccupancyLevel = OccupancyLevel.Low,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800"
},
new Beach
{
    Id = 10,
    Name = "Alabama Beach & Restaurant",
    Description = "Kahvaltıdan geceye kadar sahil keyfi, aile dostu.",
    Address = "Kuşkavağı Mah., Akdeniz Bulv. No:27/21, Konyaaltı/Antalya",
    OpenTime = "09:00",
    CloseTime = "01:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 200,
    Latitude = 36.8670,
    Longitude = 30.6460,
    Rating = 4.5,
    ReviewCount = 750,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = true,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 50,
    OccupancyLevel = OccupancyLevel.Medium,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800"
},
new Beach
{
    Id = 11,
    Name = "La Bohem Beach Restaurant",
    Description = "Bohemian atmosfer, steakhouse ve DJ performance.",
    Address = "Akdeniz Blv. No:22, Kuşkavağı, Konyaaltı/Antalya",
    Phone = "+90 540 156 07 56",
    OpenTime = "12:00",
    CloseTime = "02:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 500,
    Latitude = 36.8670,
    Longitude = 30.6480,
    Rating = 4.6,
    ReviewCount = 1100,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = false,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 70,
    OccupancyLevel = OccupancyLevel.High,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800"
},
new Beach
{
    Id = 12,
    Name = "Dubai Beach Konyaaltı",
    Description = "Premium şezlong ve daybed, steakhouse ve gece müziği.",
    Address = "Akdeniz Blv. No:33, Konyaaltı/Antalya",
    Phone = "+90 537 652 25 94",
    OpenTime = "09:00",
    CloseTime = "02:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 400,
    Latitude = 36.8624728,
    Longitude = 30.6397343,
    Rating = 4.6,
    ReviewCount = 1850,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = false,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 65,
    OccupancyLevel = OccupancyLevel.Medium,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800"
},
new Beach
{
    Id = 13,
    Name = "Vento Mare Coffee & Beach",
    Description = "Kahve ve sahil keyfinin buluştuğu sakin atmosfer.",
    Address = "Akdeniz Bulvarı, Konyaaltı/Antalya",
    OpenTime = "09:00",
    CloseTime = "23:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 150,
    Latitude = 36.8790,
    Longitude = 30.6620,
    Rating = 4.3,
    ReviewCount = 320,
    HasSunbeds = true,
    HasShower = false,
    HasParking = false,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = false,
    IsChildFriendly = true,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = false,
    OccupancyPercent = 30,
    OccupancyLevel = OccupancyLevel.Low,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800"
},
new Beach
{
    Id = 14,
    Name = "Cafe Belle Bistro & Beach",
    Description = "Akdeniz mutfağı, kokteyl ve her gece DJ performance.",
    Address = "Gürsu Mah. Akdeniz Blv. No:45/1, Konyaaltı/Antalya",
    OpenTime = "09:00",
    CloseTime = "02:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 300,
    Latitude = 36.8795,
    Longitude = 30.6635,
    Rating = 4.5,
    ReviewCount = 680,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = false,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 55,
    OccupancyLevel = OccupancyLevel.Medium,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800"
},
new Beach
{
    Id = 15,
    Name = "Kuki Box Beach Lounge",
    Description = "Pizza, burger ve daybed ile bohem sahil atmosferi.",
    Address = "Gürsu Mah. Akdeniz Blv., Konyaaltı/Antalya",
    Phone = "+90 242 248 47 07",
    OpenTime = "10:00",
    CloseTime = "01:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 400,
    Latitude = 36.8800,
    Longitude = 30.6640,
    Rating = 4.4,
    ReviewCount = 540,
    HasSunbeds = true,
    HasShower = true,
    HasParking = false,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = false,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 60,
    OccupancyLevel = OccupancyLevel.Medium,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800"
},
new Beach
{
    Id = 16,
    Name = "Frida Beach 33",
    Description = "Aile dostu, oyun parkı ve kahvaltıyla sahil keyfi.",
    Address = "Gürsu Mah. Akdeniz Bulvarı, Konyaaltı/Antalya",
    Phone = "+90 530 668 21 07",
    OpenTime = "08:00",
    CloseTime = "23:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 150,
    Latitude = 36.8792,
    Longitude = 30.6628,
    Rating = 4.0,
    ReviewCount = 290,
    HasSunbeds = true,
    HasShower = true,
    HasParking = true,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = true,
    HasWaterSports = false,
    HasWifi = false,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 35,
    OccupancyLevel = OccupancyLevel.Low,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1519046904884-53103b34b206?w=800"
},
new Beach
{
    Id = 17,
    Name = "Shakespeare Beach",
    Description = "Pet friendly, bistro yemekleri ve aile dostu sahil.",
    Address = "Liman Mah. Akdeniz Blv. No:207, Konyaaltı/Antalya",
    Phone = "+90 507 072 06 00",
    OpenTime = "08:00",
    CloseTime = "23:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 200,
    Latitude = 36.8820,
    Longitude = 30.6730,
    Rating = 4.2,
    ReviewCount = 410,
    HasSunbeds = true,
    HasShower = true,
    HasParking = false,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = false,
    IsChildFriendly = true,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = false,
    OccupancyPercent = 40,
    OccupancyLevel = OccupancyLevel.Low,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1507525428034-b723cf961d3e?w=800"
},
new Beach
{
    Id = 18,
    Name = "Riviera Beach Lounge",
    Description = "Sakin atmosfer, deniz manzarası ve özenli servis.",
    Address = "Akdeniz Blv. No:211, Liman, Konyaaltı/Antalya",
    OpenTime = "09:00",
    CloseTime = "23:00",
    HasEntryFee = false,
    EntryFee = 0,
    SunbedPrice = 300,
    Latitude = 36.8822,
    Longitude = 30.6735,
    Rating = 4.8,
    ReviewCount = 95,
    HasSunbeds = true,
    HasShower = true,
    HasParking = false,
    HasRestaurant = true,
    HasBar = true,
    HasAlcohol = true,
    IsChildFriendly = true,
    HasWaterSports = false,
    HasWifi = true,
    HasPool = false,
    HasDJ = true,
    OccupancyPercent = 30,
    OccupancyLevel = OccupancyLevel.Low,
    IsOpen = true,
    CoverImageUrl = "https://images.unsplash.com/photo-1473116763249-2faaef81ccda?w=800"
}

        );
        modelBuilder.Entity<BusinessUser>().HasData(
   new BusinessUser
   {
       Id = 1,
       BeachId = 1,
       Email = "kalypso@beach.com",
       PasswordHash = "UOm0YNf5xsCBeKQJM3Yxt8d8zNcBESfLxj3H1gKKQbE=",
       ContactName = "Kalypso Yönetici",
       IsActive = true,
       CreatedAt = new DateTime(2026, 1, 1)
   }
);
    }
}