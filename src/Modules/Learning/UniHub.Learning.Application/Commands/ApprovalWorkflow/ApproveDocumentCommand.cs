using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Command to approve a document
/// </summary>
/// <param name="DocumentId">ID of the document to approve</param>
/// <param name="ApproverId">ID of the moderator approving the document</param>
/// <param name="ApprovalNotes">Optional notes from the approver</param>
public sealed record ApproveDocumentCommand(
    Guid DocumentId,
    Guid ApproverId,
    string? ApprovalNotes = null) : ICommand;
