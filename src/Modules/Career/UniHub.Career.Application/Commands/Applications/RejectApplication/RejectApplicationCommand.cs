using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Applications.RejectApplication;

/// <summary>
/// Command to reject a job application.
/// Used by recruiters to reject applications at any stage.
/// </summary>
public sealed record RejectApplicationCommand(
    Guid ApplicationId,
    Guid ReviewerId,
    string? Reason = null) : ICommand<bool>;
