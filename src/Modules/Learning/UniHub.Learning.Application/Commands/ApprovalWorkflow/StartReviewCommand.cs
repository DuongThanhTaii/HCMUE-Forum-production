using UniHub.SharedKernel.CQRS;

namespace UniHub.Learning.Application.Commands.ApprovalWorkflow;

/// <summary>
/// Command to start reviewing a document
/// </summary>
/// <param name="DocumentId">ID of the document to review</param>
/// <param name="ReviewerId">ID of the moderator starting the review</param>
public sealed record StartReviewCommand(
    Guid DocumentId,
    Guid ReviewerId) : ICommand;
