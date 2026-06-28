using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.DocumentRating;

/// <summary>
/// Command to rate a document (1-5 stars).
/// Enforces one rating per user per document at application layer.
/// </summary>
public sealed record RateDocumentCommand(
    Guid DocumentId,
    Guid UserId,
    int Rating) : ICommand;
