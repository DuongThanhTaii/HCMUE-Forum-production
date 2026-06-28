using UniHub.Learning.Application.Abstractions;
using UniHub.Learning.Domain.Documents;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Handler for ApproveDocumentCommand
/// </summary>
public sealed class ApproveDocumentCommandHandler : ICommandHandler<ApproveDocumentCommand>
{
    private readonly IDocumentRepository _documentRepository;
    private readonly IModeratorPermissionService _permissionService;

    public ApproveDocumentCommandHandler(
        IDocumentRepository documentRepository,
        IModeratorPermissionService permissionService)
    {
        _documentRepository = documentRepository;
        _permissionService = permissionService;
    }

    public async Task<Result> Handle(ApproveDocumentCommand request, CancellationToken cancellationToken)
    {
        // Get document
        var document = await _documentRepository.GetByIdAsync(DocumentId.Create(request.DocumentId), cancellationToken);
        if (document is null)
        {
            return Result.Failure(new Error("Document.NotFound", "Document not found"));
        }

        // Check moderator permission
        var isModerator = await _permissionService.IsModeratorForDocumentAsync(
            request.ApproverId,
            request.DocumentId,
            cancellationToken);

        if (!isModerator)
        {
            return Result.Failure(new Error("Document.Unauthorized", "User is not a moderator for this document's course"));
        }

        // Approve document
        var result = document.Approve(request.ApproverId, request.ApprovalNotes);
        if (result.IsFailure)
        {
            return result;
        }

        // Save document
        await _documentRepository.UpdateAsync(document, cancellationToken);

        return Result.Success();
    }
}
