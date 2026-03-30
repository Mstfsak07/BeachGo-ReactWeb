using BeachRehberi.Application.Common.Interfaces;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly ICurrentUserService? _currentUserService;

    // ICurrentUserService migration atarken (design-time) null gelebileceği için opsiyonel yapıldı
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Beach> Beaches => Set<Beach>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<BeachPhoto> BeachPhotos => Set<BeachPhoto>();
    public DbSet<BeachEvent> BeachEvents => Set<BeachEvent>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUserIdStr = _currentUserService?.UserId?.ToString();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            // ─── GÜVENLİK PENCERESİ: MUTASYON VE MANİPÜLASYON KONTROLLERİ ────────────
            if (entry.State == EntityState.Modified || entry.State == EntityState.Deleted)
            {
                // İşletme Sahipleri Koruması
                if (_currentUserService?.TenantId != null)
                {
                    // Farklı kayıtlardaki işlem sınırlarını aşmasını engelle
                    if (entry.Entity.TenantId.HasValue && entry.Entity.TenantId != _currentUserService.TenantId)
                    {
                        throw new UnauthorizedAccessException("Erişim Reddedildi: Başka bir işletmeye ait kayıtlar değiştirilemez veya silinemez.");
                    }
                    
                    if (entry.State == EntityState.Modified)
                    {
                        var tenantProp = entry.Metadata.FindProperty("TenantId");
                        if (tenantProp != null && entry.Property("TenantId").IsModified)
                            throw new UnauthorizedAccessException("Erişim Reddedildi: Kaydın TenantId bilgisini değiştiremezsiniz (Hak ihlali).");
                    }
                }
                // Standart Müşteri Kullanıcı Sahiplik Koruması
                else if (_currentUserService?.TenantId == null && currentUserIdStr != null)
                {
                    // Rezarvasyon veya Review için UserId kontrolü
                    var userIdProp = entry.Metadata.FindProperty("UserId");
                    if (userIdProp != null)
                    {
                        var originalOwnerId = entry.Property("UserId").OriginalValue?.ToString();
                        
                        if (originalOwnerId != null && originalOwnerId != currentUserIdStr)
                            throw new UnauthorizedAccessException("Erişim Reddedildi: Yalnızca kendi verilerinizi değiştirebilir ve silebilirsiniz.");

                        // Kötü Niyet Koruması: Kaydını başkasının üstüne yamamayı engelle
                        if (entry.State == EntityState.Modified && entry.Property("UserId").IsModified)
                            throw new UnauthorizedAccessException("Sahiplik (UserId) bilgisi sonradan manipüle edilemez.");
                    }
                    // Profili düzenlerken Kendi Profilinde mi? (User Entity Kontrolü)
                    else if (entry.Entity is User userEntity)
                    {
                        if (userEntity.Id.ToString() != currentUserIdStr)
                            throw new UnauthorizedAccessException("Erişim Reddedildi: Sadece kendi kullanıcı profili ayarlarınızı güncelleyebilirsiniz.");
                        
                        // YETKİ (Role) YÜKSELTME KORUMASI: Normal üyelerin PUT isteğinde kendilerini Admin'e çekmesi bloklanır.
                        if (entry.State == EntityState.Modified && entry.Property("Role").IsModified)
                            throw new UnauthorizedAccessException("Erişim Reddedildi: Kullanıcı rolleri ve yetkileri doğrudan manipüle edilemez (Yetki Yükseltme).");
                    }
                }
            }

            // ─── STANDART DURUM GEÇİŞLERİ ────────────────────────────────────────────
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    
                    // İşletme sahibi eklerken sahte bir TenantId enjeksiyonunu siler ve zorla kendi TenantId'sini atar
                    if (_currentUserService?.TenantId != null)
                    {
                        entry.Entity.TenantId = _currentUserService.TenantId;
                    }
                    else if (_currentUserService?.TenantId == null && currentUserIdStr != null)
                    {
                        // Müşteri sisteme yorum veya rezervasyon ekliyorsa:
                        // Dışarıdan gelen Fake UserId enjeksiyonunu sileriz
                        var addUserIdProp = entry.Metadata.FindProperty("UserId");
                        if (addUserIdProp != null)
                        {
                            entry.Property("UserId").CurrentValue = _currentUserService.UserId;
                        }
                    }
                    break;

                case EntityState.Modified:
                    // Silinmiş olarak işaretlenmiyorsa (SoftDelete fonksiyonu bunu yapar) normal Updated ataması
                    if (!entry.Entity.IsDeleted)
                    {
                        entry.Entity.SetUpdated();
                    }
                    break;

                case EntityState.Deleted:
                    // EF Core Hard-Delete yakalandığında işlemi iptal edip Soft-Delete olarak değiştiriyoruz
                    entry.State = EntityState.Modified;
                    entry.Entity.SoftDelete();
                    break;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Global Soft-Delete ve Multi-Tenant/Owner Filtreleri ────────────────────────
        
        // Tenant Entity'sinin kendisi merkeze ait ana yönetim tablosudur, sadece Soft-Delete filtresi uygulandı.
        modelBuilder.Entity<Tenant>().HasQueryFilter(e => !e.IsDeleted);

        // Ortak ve Dışarıya Açık / Listelenebilir Varlıklar (Tüm Müşteriler Hepsini Görebilir)
        modelBuilder.Entity<Beach>().HasQueryFilter(e => !e.IsDeleted && (_currentUserService == null || _currentUserService.TenantId == null || e.TenantId == _currentUserService.TenantId));
        modelBuilder.Entity<Review>().HasQueryFilter(e => !e.IsDeleted && (_currentUserService == null || _currentUserService.TenantId == null || e.TenantId == _currentUserService.TenantId));
        modelBuilder.Entity<BeachPhoto>().HasQueryFilter(e => !e.IsDeleted && (_currentUserService == null || _currentUserService.TenantId == null || e.TenantId == _currentUserService.TenantId));
        modelBuilder.Entity<BeachEvent>().HasQueryFilter(e => !e.IsDeleted && (_currentUserService == null || _currentUserService.TenantId == null || e.TenantId == _currentUserService.TenantId));

        // ─── GİZLİLİK ONANMIŞ VARLIKLAR (USER VE SUBSCRIPTION RİSKİ GİDERİLDİ) ────────
        // User (Kullanıcı): Standart bir müşteri SADECE kendini görebilir! Başkasının User datasını çekemez.
        modelBuilder.Entity<User>().HasQueryFilter(e => !e.IsDeleted && 
            (_currentUserService == null || 
             (_currentUserService.TenantId != null && e.TenantId == _currentUserService.TenantId) ||
             (_currentUserService.TenantId == null && _currentUserService.UserId != null && e.Id == _currentUserService.UserId)));

        // Subscription (Abonelik): Standart bir müşteri hiçbir finansal kaydı göremez! Sadece Tenant okuyabilir.
        modelBuilder.Entity<Subscription>().HasQueryFilter(e => !e.IsDeleted && 
            (_currentUserService == null || 
             (_currentUserService.TenantId != null && e.TenantId == _currentUserService.TenantId)));

        // ─── OWNER VALIDATION: REZERVASYON (GİZLİ VERİ) ──────────────────────────────────
        // Eğer Tenant ise kendi işletmesinin rezervasyonlarını (TenantId eşleşmesi)
        // Eğer Müşteri ise SADECE KENDİ rezervasyonlarını (UserId eşleşmesi) görebilir!
        modelBuilder.Entity<Reservation>().HasQueryFilter(e => !e.IsDeleted && 
            (_currentUserService == null || 
             (_currentUserService.TenantId != null && e.TenantId == _currentUserService.TenantId) ||
             (_currentUserService.TenantId == null && _currentUserService.UserId != null && e.UserId == _currentUserService.UserId)));

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
