using ArtAuction.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ArtAuction.Infrastructure.Persistence;

/// <summary>
/// Generic EF Core repository implementation.
/// Wraps <see cref="ApplicationDbContext"/> and provides standard CRUD/query
/// operations for any domain entity via the <see cref="IRepository{T}"/> contract.
/// </summary>
/// <typeparam name="T">Domain entity type.</typeparam>
public sealed class Repository<T> : IRepository<T> where T : class
{
    private readonly ApplicationDbContext _context;
    private readonly DbSet<T> _set;

    public Repository(ApplicationDbContext context)
    {
        _context = context;
        _set = context.Set<T>();
    }

    // ── Query ──────────────────────────────────────────────────────────────────

    public async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _set.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _set.AsNoTracking().ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _set.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _set.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default)
        => await _set.AnyAsync(predicate, cancellationToken);

    public IQueryable<T> Query() => _set.AsQueryable();

    // ── Write ──────────────────────────────────────────────────────────────────

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _set.AddAsync(entity, cancellationToken);

    public void Update(T entity)
        => _set.Update(entity);

    public void Remove(T entity)
        => _set.Remove(entity);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
