using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace UniHub.Infrastructure.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically sets audit fields (CreatedAt, UpdatedAt, CreatedBy, UpdatedBy)
/// for entities that implement the IAuditableEntity interface.
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    /// <summary>
    /// Called before SaveChanges to set audit fields on tracked entities.
    /// </summary>
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            UpdateAuditableEntities(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    /// <summary>
    /// Updates audit fields for all tracked entities that implement IAuditableEntity.
    /// </summary>
    /// <param name="context">The database context.</param>
    private static void UpdateAuditableEntities(DbContext context)
    {
        var utcNow = DateTime.UtcNow;
        var entries = context.ChangeTracker.Entries<IAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                SetCreatedProperties(entry, utcNow);
            }

            if (entry.State == EntityState.Modified)
            {
                SetModifiedProperties(entry, utcNow);
            }
        }
    }

    /// <summary>
    /// Sets the CreatedAt and CreatedBy properties for a newly added entity.
    /// </summary>
    private static void SetCreatedProperties(EntityEntry<IAuditableEntity> entry, DateTime utcNow)
    {
        entry.Entity.CreatedAt = utcNow;
        // TODO: Get current user ID from ICurrentUserService when available
        // entry.Entity.CreatedBy = currentUserId;
    }

    /// <summary>
    /// Sets the UpdatedAt and UpdatedBy properties for a modified entity.
    /// </summary>
    private static void SetModifiedProperties(EntityEntry<IAuditableEntity> entry, DateTime utcNow)
    {
        entry.Entity.UpdatedAt = utcNow;
        // TODO: Get current user ID from ICurrentUserService when available
        // entry.Entity.UpdatedBy = currentUserId;
    }
}

/// <summary>
/// Interface for entities that track creation and modification metadata.
/// </summary>
public interface IAuditableEntity
{
    /// <summary>
    /// Gets or sets the date and time when the entity was created (UTC).
    /// </summary>
    DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who created the entity.
    /// </summary>
    string? CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the entity was last updated (UTC).
    /// </summary>
    DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the ID of the user who last updated the entity.
    /// </summary>
    string? UpdatedBy { get; set; }
}
