using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Documents;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Handler for StartReviewCommand
/// </summary>
public sealed class StartReviewCommandHandler : ICommandHandler<StartReviewCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IModeratorPermissionService _permissionService;

    public StartReviewCommandHandler(
        IDocumentRepository documentRepository,
        IModeratorPermissionService permissionService)
    {
        _documentRepository = documentRepository;
        _permissionService = permissionService;
    }

    public async Task<Result> Handle(StartReviewCommand request, CancellationToken cancellationToken)
    {
        // Get document
        var document = await _documentRepository.GetByIdAsync(DocumentId.Create(request.DocumentId), cancellationToken);
        if (document is null)
        {
            return Result.Failure(new Error("Document.NotFound", "Document not found"));
        }

        // Check moderator permission
        var isModerator = await _permissionService.IsModeratorForDocumentAsync(
            request.ReviewerId,
            request.DocumentId,
            cancellationToken);

        if (!isModerator)
        {
            return Result.Failure(new Error("Document.Unauthorized", "User is not a moderator for this document's course"));
        }

        // Start review
        var result = document.StartReview(request.ReviewerId);
        if (result.IsFailure)
        {
            return result;
        }

        // Save document
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}
