using System.Linq.Expressions;
using BeachRehberi.Domain.Common;
using BeachRehberi.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BeachRehberi.Infrastructure.Persistence.Repositories;

public class BaseRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    /// <summary>
    /// ID ile entity getirir. Güncelleme / silme işlemlerinde tracking gerektiğinden
    /// FindAsync kullanılır (tracking açık).
    /// </summary>
    public async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(new object[] { id }, cancellationToken);

    /// <summary>
    /// Tüm kayıtları getirir. Soft-delete filtresi DbContext'te global query filter
    /// olarak tanımlandığından IsDeleted kontrolü burada tekrarlanmaz.
    /// </summary>
    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _dbSet
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .AsNoTracking()
            .Where(predicate)
            .ToListAsync(cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet
            .AsNoTracking()
            .FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> AnyAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);

    public async Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default)
        => predicate is null
            ? await _dbSet.CountAsync(cancellationToken)
            : await _dbSet.CountAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public async Task AddRangeAsync(
        IEnumerable<T> entities,
        CancellationToken cancellationToken = default)
        => await _dbSet.AddRangeAsync(entities, cancellationToken);

    /// <summary>
    /// Entity'yi günceller. Entity önce GetByIdAsync ile tracking modunda
    /// getirilmiş olmalıdır; aksi hâlde EF Core tüm alanları günceller.
    /// </summary>
    public void Update(T entity)
        => _dbSet.Update(entity);

    public void Remove(T entity)
        => _dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities)
        => _dbSet.RemoveRange(entities);

    /// <summary>
    /// Ham IQueryable döndürür. Özel sorgu ihtiyaçlarında kullanılır.
    /// Soft-delete global filtresi burada da aktiftir.
    /// </summary>
    public IQueryable<T> Query()
        => _dbSet.AsQueryable();
}
