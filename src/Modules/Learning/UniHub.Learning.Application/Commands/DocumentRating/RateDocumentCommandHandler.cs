using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Documents;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.DocumentRating;

public sealed class RateDocumentCommandHandler : ICommandHandler<RateDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IUserRatingService _userRatingService;

    public RateDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IUserRatingService userRatingService)
    {
        _documentRepository = documentRepository;
        _userRatingService = userRatingService;
    }

    public async Task<Result> Handle(RateDocumentCommand request, CancellationToken cancellationToken)
    {
        // Check if user has already rated this document
        var hasRated = await _userRatingService.HasUserRatedDocumentAsync(
            request.UserId, request.DocumentId, cancellationToken);

        if (hasRated)
        {
            return Result.Failure(new Error(
                "Document.AlreadyRated",
                "User has already rated this document"));
        }

        // Get document
        var document = await _documentRepository.GetByIdAsync(
            DocumentId.Create(request.DocumentId), cancellationToken);

        if (document is null)
        {
            return Result.Failure(new Error(
                "Document.NotFound",
                $"Document with ID '{request.DocumentId}' was not found"));
        }

        // Add rating to document (validates approved status and rating range)
        var result = document.AddRating(request.Rating);
        if (result.IsFailure)
        {
            return result;
        }

        // Save document with updated rating
        await _documentRepository.UpdateAsync(document, cancellationToken);

        // Record user rating
        await _userRatingService.RecordUserRatingAsync(
            request.UserId, request.DocumentId, request.Rating, cancellationToken);

        return Result.Success();
    }
}
