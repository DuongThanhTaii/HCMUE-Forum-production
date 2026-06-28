using UniHub.SharedKernel.Domain;

namespace UniHub.SharedKernel.Persistence;

/// <summary>
/// Generic repository interface for aggregate roots.
/// </summary>
/// <typeparam name="TEntity">The aggregate root type</typeparam>
/// <typeparam name="TId">The entity ID type</typeparam>
public interface IRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : notnull
{
    /// <summary>
    /// Gets an entity by its ID.
    /// </summary>
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specification.
    /// </summary>
    Task<IReadOnlyList<TEntity>> FindAsync(
        Specification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the first entity matching the specification, or null.
    /// </summary>
    Task<TEntity?> FirstOrDefaultAsync(
        Specification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specification.
    /// </summary>
    Task<bool> AnyAsync(
        Specification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specification.
    /// </summary>
    Task<int> CountAsync(
        Specification<TEntity> specification,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task AddAsync(TEntity entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(TEntity entity);

    /// <summary>
    /// Updates multiple entities.
    /// </summary>
    void UpdateRange(IEnumerable<TEntity> entities);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    void Remove(TEntity entity);

    /// <summary>
    /// Removes multiple entities.
    /// </summary>
    void RemoveRange(IEnumerable<TEntity> entities);
}
