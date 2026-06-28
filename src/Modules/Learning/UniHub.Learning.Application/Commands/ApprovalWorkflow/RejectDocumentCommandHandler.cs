using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Documents;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Handler for RejectDocumentCommand
/// </summary>
public sealed class RejectDocumentCommandHandler : ICommandHandler<RejectDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IModeratorPermissionService _permissionService;

    public RejectDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IModeratorPermissionService permissionService)
    {
        _documentRepository = documentRepository;
        _permissionService = permissionService;
    }

    public async Task<Result> Handle(RejectDocumentCommand request, CancellationToken cancellationToken)
    {
        // Get document
        var document = await _documentRepository.GetByIdAsync(DocumentId.Create(request.DocumentId), cancellationToken);
        if (document is null)
        {
            return Result.Failure(new Error("Document.NotFound", "Document not found"));
        }

        // Check moderator permission
        var isModerator = await _permissionService.IsModeratorForDocumentAsync(
            request.RejectorId,
            request.DocumentId,
            cancellationToken);

        if (!isModerator)
        {
            return Result.Failure(new Error("Document.Unauthorized", "User is not a moderator for this document's course"));
        }

        // Reject document
        var result = document.Reject(request.RejectorId, request.Reason);
        if (result.IsFailure)
        {
            return result;
        }

        // Save document
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}
