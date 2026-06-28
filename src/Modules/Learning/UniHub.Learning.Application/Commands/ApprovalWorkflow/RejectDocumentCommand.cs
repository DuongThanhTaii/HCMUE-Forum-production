using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Command to reject a document
/// </summary>
/// <param name="DocumentId">ID of the document to reject</param>
/// <param name="RejectorId">ID of the moderator rejecting the document</param>
/// <param name="Reason">Reason for rejection</param>
public sealed record RejectDocumentCommand(
    Guid DocumentId,
    Guid RejectorId,
    string Reason) : ICommand;
