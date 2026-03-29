using BeachRehberi.Domain.Entities;

namespace BeachRehberi.Domain.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<Beach> Beaches { get; }
    IRepository<User> Users { get; }
    IRepository<Reservation> Reservations { get; }
    IRepository<Review> Reviews { get; }
    IRepository<BeachPhoto> BeachPhotos { get; }
    IRepository<BeachEvent> BeachEvents { get; }
    IRepository<Tenant> Tenants { get; }
    IRepository<Subscription> Subscriptions { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}
