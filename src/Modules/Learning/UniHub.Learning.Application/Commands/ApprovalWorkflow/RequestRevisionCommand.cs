using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Command to request revision on a document (returns to draft)
/// </summary>
/// <param name="DocumentId">ID of the document needing revision</param>
/// <param name="ReviewerId">ID of the moderator requesting revision</param>
/// <param name="RevisionNotes">Notes explaining what needs to be revised</param>
public sealed record RequestRevisionCommand(
    Guid DocumentId,
    Guid ReviewerId,
    string RevisionNotes) : ICommand;
