using BeachRehberi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Beach> Beaches => Set<Beach>();
    public DbSet<BeachPhoto> BeachPhotos => Set<BeachPhoto>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BeachEvent> BeachEvents => Set<BeachEvent>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User konfigürasyonu
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(256);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.FirstName).HasMaxLength(100);
            e.Property(u => u.LastName).HasMaxLength(100);
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.Role).HasConversion<string>();
            e.HasQueryFilter(u => !u.IsDeleted);
        });

        // Tenant konfigürasyonu
        modelBuilder.Entity<Tenant>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Name).IsRequired().HasMaxLength(200);
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Plan).HasConversion<string>();
            e.Property(t => t.SubscriptionStatus).HasConversion<string>();
            e.HasQueryFilter(t => !t.IsDeleted);
        });

        // Beach konfigürasyonu
        modelBuilder.Entity<Beach>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.Name).IsRequired().HasMaxLength(200);
            e.Property(b => b.EntryFee).HasPrecision(18, 2);
            e.Property(b => b.SunbedPrice).HasPrecision(18, 2);
            e.Property(b => b.OccupancyLevel).HasConversion<string>();

            e.HasOne(b => b.Tenant)
             .WithMany(t => t.Beaches)
             .HasForeignKey(b => b.TenantId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasMany(b => b.Photos)
             .WithOne(p => p.Beach)
             .HasForeignKey(p => p.BeachId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(b => b.Reviews)
             .WithOne(r => r.Beach)
             .HasForeignKey(r => r.BeachId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(b => b.Reservations)
             .WithOne(r => r.Beach)
             .HasForeignKey(r => r.BeachId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(b => !b.IsDeleted);
        });

        // Reservation konfigürasyonu
        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.TotalPrice).HasPrecision(18, 2);
            e.Property(r => r.Status).HasConversion<string>();

            e.HasOne(r => r.User)
             .WithMany(u => u.Reservations)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Restrict);

            e.HasQueryFilter(r => !r.IsDeleted);
        });

        // Review konfigürasyonu
        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Comment).HasMaxLength(1000);
            e.HasQueryFilter(r => !r.IsDeleted);
        });

        // Subscription konfigürasyonu
        modelBuilder.Entity<Subscription>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Amount).HasPrecision(18, 2);
            e.Property(s => s.Plan).HasConversion<string>();
            e.Property(s => s.Status).HasConversion<string>();
        });
    }
}
