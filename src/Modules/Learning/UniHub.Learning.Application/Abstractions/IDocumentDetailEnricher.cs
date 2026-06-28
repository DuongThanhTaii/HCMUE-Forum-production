namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Resolves human-readable names for document detail (users, course).
/// </summary>
public interface IDocumentDetailEnricher
{
    Task<(string? UploaderDisplayName, string? CourseName, string? ReviewerDisplayName)> EnrichAsync(
        Guid uploaderId,
        Guid? courseId,
        Guid? reviewerId,
        CancellationToken cancellationToken = default);
}
