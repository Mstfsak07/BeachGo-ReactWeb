using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Interfaces;
using BeachRehberi.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace BeachRehberi.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    private IRepository<Beach>? _beaches;
    private IRepository<User>? _users;
    private IRepository<Reservation>? _reservations;
    private IRepository<Review>? _reviews;
    private IRepository<BeachPhoto>? _beachPhotos;
    private IRepository<BeachEvent>? _beachEvents;
    private IRepository<Tenant>? _tenants;
    private IRepository<Subscription>? _subscriptions;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Beach> Beaches => _beaches ??= new BaseRepository<Beach>(_context);
    public IRepository<User> Users => _users ??= new BaseRepository<User>(_context);
    public IRepository<Reservation> Reservations => _reservations ??= new BaseRepository<Reservation>(_context);
    public IRepository<Review> Reviews => _reviews ??= new BaseRepository<Review>(_context);
    public IRepository<BeachPhoto> BeachPhotos => _beachPhotos ??= new BaseRepository<BeachPhoto>(_context);
    public IRepository<BeachEvent> BeachEvents => _beachEvents ??= new BaseRepository<BeachEvent>(_context);
    public IRepository<Tenant> Tenants => _tenants ??= new BaseRepository<Tenant>(_context);
    public IRepository<Subscription> Subscriptions => _subscriptions ??= new BaseRepository<Subscription>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync()
        => _transaction = await _context.Database.BeginTransactionAsync();

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
            await _transaction.CommitAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
            await _transaction.RollbackAsync();
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
