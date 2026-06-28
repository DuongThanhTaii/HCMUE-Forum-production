using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Abstractions;

/// <summary>
/// Computes moderation scope for a user and resolves the effective category of a report target.
/// </summary>
public interface IModerationScopeService
{
    /// <summary>
    /// Returns the category scope for a moderator:
    /// <list type="bullet">
    ///   <item><term>null</term><description>Admin — no filter, sees everything.</description></item>
    ///   <item><term>empty list</term><description>Moderator with no category assignments — sees nothing.</description></item>
    ///   <item><term>non-empty list</term><description>Moderator's assigned category IDs.</description></item>
    /// </list>
    /// </summary>
    Task<IReadOnlyList<Guid>?> GetCategoryScopeAsync(
        Guid userId,
        bool isAdmin,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves the effective category ID for the reported item (Post or Comment → Post).
    /// Returns <c>null</c> if the target entity no longer exists.
    /// </summary>
    Task<Guid?> GetEffectiveCategoryIdAsync(
        Report report,
        CancellationToken cancellationToken = default);
}
