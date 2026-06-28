using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Commands.Applications.AcceptApplication;

/// <summary>
/// Command for applicant to accept a job offer.
/// Only the applicant who submitted the application can accept it.
/// </summary>
public sealed record AcceptApplicationCommand(
    Guid ApplicationId,
    Guid ApplicantId) : ICommand<bool>;
