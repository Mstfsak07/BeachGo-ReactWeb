using BeachRehberi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Beach> Beaches => Set<Beach>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BeachPhoto> BeachPhotos => Set<BeachPhoto>();
    public DbSet<BeachEvent> BeachEvents => Set<BeachEvent>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Global Soft-Delete Filtreleri ────────────────────────
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Beach>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Reservation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BeachPhoto>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<BeachEvent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Subscription>().HasQueryFilter(e => !e.IsDeleted);

        // ─── User ─────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Phone).HasMaxLength(20);
            entity.Property(u => u.ProfileImageUrl).HasMaxLength(500);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.RefreshToken).HasMaxLength(500);
            entity.Property(u => u.Role).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(u => u.Tenant)
                  .WithMany(t => t.Users)
                  .HasForeignKey(u => u.TenantId)
                  .OnDelete(DeleteBehavior.SetNull)
                  .IsRequired(false);
        });

        // ─── Tenant ───────────────────────────────────────────────
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.HasIndex(t => t.Slug).IsUnique();
            entity.Property(t => t.Name).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            entity.Property(t => t.ContactEmail).HasMaxLength(256).IsRequired();
            entity.Property(t => t.ContactPhone).HasMaxLength(20);
            entity.Property(t => t.StripeCustomerId).HasMaxLength(100);
            entity.Property(t => t.StripeSubscriptionId).HasMaxLength(100);
            entity.Property(t => t.Plan).HasConversion<string>().HasMaxLength(30);
            entity.Property(t => t.SubscriptionStatus).HasConversion<string>().HasMaxLength(30);
        });

        // ─── Beach ────────────────────────────────────────────────
        modelBuilder.Entity<Beach>(entity =>
        {
            entity.HasKey(b => b.Id);
            entity.Property(b => b.Name).HasMaxLength(200).IsRequired();
            entity.Property(b => b.Description).HasMaxLength(2000).IsRequired();
            entity.Property(b => b.Location).HasMaxLength(500).IsRequired();
            entity.Property(b => b.City).HasMaxLength(100).IsRequired();
            entity.Property(b => b.District).HasMaxLength(100);
            entity.Property(b => b.Phone).HasMaxLength(20);
            entity.Property(b => b.Website).HasMaxLength(200);
            entity.Property(b => b.Instagram).HasMaxLength(100);
            entity.Property(b => b.OpenTime).HasMaxLength(10);
            entity.Property(b => b.CloseTime).HasMaxLength(10);
            entity.Property(b => b.CoverImageUrl).HasMaxLength(500);
            entity.Property(b => b.PricePerPerson).HasColumnType("decimal(18,2)");
            entity.Property(b => b.AverageRating).HasColumnType("decimal(4,2)");

            entity.HasOne(b => b.Tenant)
                  .WithMany(t => t.Beaches)
                  .HasForeignKey(b => b.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(b => b.Photos)
                  .WithOne(p => p.Beach)
                  .HasForeignKey(p => p.BeachId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(b => b.Reviews)
                  .WithOne(r => r.Beach)
                  .HasForeignKey(r => r.BeachId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(b => b.Reservations)
                  .WithOne(r => r.Beach)
                  .HasForeignKey(r => r.BeachId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(b => b.Events)
                  .WithOne(e => e.Beach)
                  .HasForeignKey(e => e.BeachId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Reservation ──────────────────────────────────────────
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(r => r.Notes).HasMaxLength(500);
            entity.Property(r => r.StatusNote).HasMaxLength(500);
            entity.Property(r => r.PaymentIntentId).HasMaxLength(200);
            entity.Property(r => r.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reservations)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Beach)
                  .WithMany(b => b.Reservations)
                  .HasForeignKey(r => r.BeachId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Tenant)
                  .WithMany()
                  .HasForeignKey(r => r.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ─── Review ───────────────────────────────────────────────
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.Id);
            entity.Property(r => r.Comment).HasMaxLength(1000).IsRequired();

            entity.HasOne(r => r.User)
                  .WithMany(u => u.Reviews)
                  .HasForeignKey(r => r.UserId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(r => r.Beach)
                  .WithMany(b => b.Reviews)
                  .HasForeignKey(r => r.BeachId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── BeachPhoto ───────────────────────────────────────────
        modelBuilder.Entity<BeachPhoto>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Url).HasMaxLength(500).IsRequired();
            entity.Property(p => p.Caption).HasMaxLength(200);

            entity.HasOne(p => p.Beach)
                  .WithMany(b => b.Photos)
                  .HasForeignKey(p => p.BeachId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── BeachEvent ───────────────────────────────────────────
        modelBuilder.Entity<BeachEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(1000).IsRequired();
            entity.Property(e => e.TicketPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(e => e.Beach)
                  .WithMany(b => b.Events)
                  .HasForeignKey(e => e.BeachId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ─── Subscription ─────────────────────────────────────────
        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Amount).HasColumnType("decimal(18,2)");
            entity.Property(s => s.Currency).HasMaxLength(3);
            entity.Property(s => s.StripePaymentIntentId).HasMaxLength(200);
            entity.Property(s => s.StripeInvoiceId).HasMaxLength(200);
            entity.Property(s => s.Plan).HasConversion<string>().HasMaxLength(30);
            entity.Property(s => s.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(s => s.Tenant)
                  .WithMany(t => t.Subscriptions)
                  .HasForeignKey(s => s.TenantId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
