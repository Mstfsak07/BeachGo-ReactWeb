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
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<RevokedToken> RevokedTokens => Set<RevokedToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Global Query Filters (Soft Delete) ---
        modelBuilder.Entity<BusinessUser>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<Beach>().HasQueryFilter(b => !b.IsDeleted);
        modelBuilder.Entity<Reservation>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
        modelBuilder.Entity<BeachEvent>().HasQueryFilter(e => !e.IsDeleted);

        // --- BusinessUser Configuration ---
        modelBuilder.Entity<BusinessUser>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);

            entity.HasOne(d => d.Beach)
                .WithMany()
                .HasForeignKey(d => d.BeachId)
                .OnDelete(DeleteBehavior.SetNull)
                .IsRequired(false);
        });

        // --- Beach Configuration ---
        modelBuilder.Entity<Beach>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
        });

        // --- Reservation Configuration ---
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ConfirmationCode).IsUnique();
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");

            entity.HasOne(d => d.Beach)
                .WithMany(p => p.Reservations)
                .HasForeignKey(d => d.BeachId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Other Relationships ---
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasOne(d => d.Beach)
                .WithMany(p => p.Reviews)
                .HasForeignKey(d => d.BeachId);
        });

        modelBuilder.Entity<BeachEvent>(entity =>
        {
            entity.HasOne(d => d.Beach)
                .WithMany(p => p.Events)
                .HasForeignKey(d => d.BeachId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(e => e.TokenHash).IsUnique();
            entity.Property(x => x.CreatedByIp).HasMaxLength(100);
            entity.Property(x => x.CreatedByUserAgent).HasMaxLength(500);
        });

        modelBuilder.Entity<RevokedToken>(entity =>
        {
            entity.HasKey(x => x.Token);
        });
    }
}
