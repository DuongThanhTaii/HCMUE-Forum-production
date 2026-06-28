using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Applications.UpdateApplicationStatus;

/// <summary>
/// Command to update application status (reviewing, shortlisted, interviewed, offered).
/// Used by recruiters to move applications through the hiring pipeline.
/// </summary>
public sealed record UpdateApplicationStatusCommand(
    Guid ApplicationId,
    Guid ReviewerId,
    string TargetStatus,
    string? Notes = null) : ICommand<bool>;
