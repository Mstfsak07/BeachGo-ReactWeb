using BeachRehberi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Infrastructure.Persistence;

/// <summary>
/// Application database context with multi-tenant support
/// </summary>
public class BeachDbContext : DbContext
{
    private readonly Guid _currentTenantId;

    public BeachDbContext(DbContextOptions<BeachDbContext> options, ITenantService tenantService)
        : base(options)
    {
        _currentTenantId = tenantService.GetCurrentTenantId();
    }

    // DbSets
    public DbSet<Beach> Beaches => Set<Beach>();
    public DbSet<BusinessUser> BusinessUsers => Set<BusinessUser>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BeachEvent> BeachEvents => Set<BeachEvent>();
    public DbSet<BeachPhoto> BeachPhotos => Set<BeachPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply tenant isolation globally
        modelBuilder.Entity<Beach>().HasQueryFilter(e => e.TenantId == _currentTenantId && !e.IsDeleted);
        modelBuilder.Entity<BusinessUser>().HasQueryFilter(e => e.TenantId == _currentTenantId && !e.IsDeleted);
        modelBuilder.Entity<Reservation>().HasQueryFilter(e => e.TenantId == _currentTenantId && !e.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(e => e.TenantId == _currentTenantId && !e.IsDeleted);
        modelBuilder.Entity<BeachEvent>().HasQueryFilter(e => e.TenantId == _currentTenantId && !e.IsDeleted);
        modelBuilder.Entity<BeachPhoto>().HasQueryFilter(e => e.TenantId == _currentTenantId && !e.IsDeleted);

        // Configure entities
        ConfigureBeach(modelBuilder);
        ConfigureBusinessUser(modelBuilder);
        ConfigureReservation(modelBuilder);
        ConfigureReview(modelBuilder);
        ConfigureBeachEvent(modelBuilder);
        ConfigureBeachPhoto(modelBuilder);
    }

    private void ConfigureBeach(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Beach>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.Instagram).HasMaxLength(200);
            entity.Property(e => e.GooglePlaceId).HasMaxLength(200);
            entity.Property(e => e.CoverImageUrl).HasMaxLength(500);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.Name });
            entity.HasIndex(e => new { e.TenantId, e.Latitude, e.Longitude });
        });
    }

    private void ConfigureBusinessUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BusinessUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(254);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ContactName).HasMaxLength(100);
            entity.Property(e => e.BusinessName).HasMaxLength(200);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.Email }).IsUnique();
            entity.HasOne(e => e.Beach)
                .WithMany()
                .HasForeignKey(e => e.BeachId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }

    private void ConfigureReservation(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ReservationDate).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.BeachId, e.ReservationDate });
            entity.HasIndex(e => new { e.TenantId, e.UserId });

            entity.HasOne(e => e.Beach)
                .WithMany(b => b.Reservations)
                .HasForeignKey(e => e.BeachId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureReview(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Rating).IsRequired();
            entity.Property(e => e.Comment).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.ReviewDate).IsRequired();

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.BeachId });
            entity.HasIndex(e => new { e.TenantId, e.UserId });

            entity.HasOne(e => e.Beach)
                .WithMany(b => b.Reviews)
                .HasForeignKey(e => e.BeachId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureBeachEvent(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BeachEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(1000);
            entity.Property(e => e.EventDate).IsRequired();
            entity.Property(e => e.EventType).HasMaxLength(50);
            entity.Property(e => e.Organizer).HasMaxLength(100);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.BeachId });
            entity.HasIndex(e => new { e.TenantId, e.EventDate });

            entity.HasOne(e => e.Beach)
                .WithMany(b => b.Events)
                .HasForeignKey(e => e.BeachId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureBeachPhoto(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BeachPhoto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Caption).HasMaxLength(200);
            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.Property(e => e.UploadedBy).HasMaxLength(100);

            entity.HasIndex(e => e.TenantId);
            entity.HasIndex(e => new { e.TenantId, e.BeachId });
            entity.HasIndex(e => new { e.TenantId, e.BeachId, e.IsCoverPhoto });

            entity.HasOne(e => e.Beach)
                .WithMany(b => b.Photos)
                .HasForeignKey(e => e.BeachId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}