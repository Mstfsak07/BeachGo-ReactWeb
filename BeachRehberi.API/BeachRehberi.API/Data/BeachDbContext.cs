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

        modelBuilder.Entity<Reservation>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.ConfirmationCode).IsUnique(); 
            e.Property(x => x.TotalPrice).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Beach).WithMany(x => x.Reservations).HasForeignKey(x => x.BeachId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Review>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => new { x.BeachId, x.UserId }).IsUnique(); 
            e.HasOne(x => x.Beach).WithMany(x => x.Reviews).HasForeignKey(x => x.BeachId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<RefreshToken>(e => {
            e.HasIndex(x => x.Token).IsUnique();
        });

        modelBuilder.Entity<RevokedToken>(e => {
            e.HasKey(x => x.Token);
        });
    }
}
