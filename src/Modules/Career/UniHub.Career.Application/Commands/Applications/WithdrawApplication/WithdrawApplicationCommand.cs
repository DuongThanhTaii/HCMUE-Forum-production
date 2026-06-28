using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Applications.WithdrawApplication;

/// <summary>
/// Command to withdraw a job application.
/// Only the applicant who submitted the application can withdraw it.
/// </summary>
public sealed record WithdrawApplicationCommand(
    Guid ApplicationId,
    Guid ApplicantId,
    string? Reason = null) : ICommand<bool>;
