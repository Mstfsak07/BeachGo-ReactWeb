using BeachRehberi.Domain.Entities;
using BeachRehberi.Domain.Interfaces;
using BeachRehberi.Infrastructure.Repositories;

using BeachRehberi.Infrastructure.Persistence;

namespace BeachRehberi.Infrastructure.Repositories;

/// <summary>
/// Unit of Work implementation
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    private IRepository<Beach>? _beaches;
    private IRepository<Reservation>? _reservations;
    private IRepository<Review>? _reviews;
    private IRepository<BeachEvent>? _beachEvents;
    private IRepository<BeachPhoto>? _beachPhotos;
    private IRepository<BusinessUser>? _businessUsers;

    public UnitOfWork(BeachDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IRepository<Beach> Beaches => _beaches ??= new BaseRepository<Beach>(_context);
    public IRepository<Reservation> Reservations => _reservations ??= new BaseRepository<Reservation>(_context);
    public IRepository<Review> Reviews => _reviews ??= new BaseRepository<Review>(_context);
    public IRepository<BeachEvent> BeachEvents => _beachEvents ??= new BaseRepository<BeachEvent>(_context);
    public IRepository<BeachPhoto> BeachPhotos => _beachPhotos ??= new BaseRepository<BeachPhoto>(_context);
    public IRepository<BusinessUser> BusinessUsers => _businessUsers ??= new BaseRepository<BusinessUser>(_context);

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackTransactionAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}