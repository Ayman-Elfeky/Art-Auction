using System.Linq.Expressions;

namespace ArtAuction.Application.Common.Interfaces;

/// <summary>
/// Generic repository contract providing standard CRUD and query operations
/// over a domain entity. Keeps the Application layer free of EF Core dependencies.
/// </summary>
/// <typeparam name="T">Domain entity type.</typeparam>
public interface IRepository<T> where T : class
{
    // ── Query ──────────────────────────────────────────────────────────────────

    /// <summary>Returns the entity with the given primary key, or null.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities (unfiltered). Use with care on large tables.</summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>Returns entities that satisfy the given predicate.</summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Returns the first entity matching the predicate, or null.</summary>
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Returns true if at least one entity satisfies the predicate.</summary>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Returns a queryable that callers can further compose with LINQ.</summary>
    IQueryable<T> Query();

    // ── Write ──────────────────────────────────────────────────────────────────

    /// <summary>Adds a new entity. Call SaveChangesAsync to persist.</summary>
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>Updates an existing entity. Call SaveChangesAsync to persist.</summary>
    void Update(T entity);

    /// <summary>Removes an entity. Call SaveChangesAsync to persist.</summary>
    void Remove(T entity);

    /// <summary>Persists all pending changes to the database.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
