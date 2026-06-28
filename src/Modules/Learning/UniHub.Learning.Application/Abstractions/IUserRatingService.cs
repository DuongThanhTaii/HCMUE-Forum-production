namespace UniHub.Learning.Application.Abstractions;

/// <summary>
/// Service for tracking user ratings on documents.
/// Ensures one rating per user per document constraint.
/// </summary>
public interface IUserRatingService
{
    /// <summary>
    /// Checks if a user has already rated a specific document.
    /// </summary>
    Task<bool> HasUserRatedDocumentAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records that a user has rated a document.
    /// This should be called after successfully adding the rating to the document.
    /// </summary>
    Task RecordUserRatingAsync(Guid userId, Guid documentId, int rating, CancellationToken cancellationToken = default);
}
