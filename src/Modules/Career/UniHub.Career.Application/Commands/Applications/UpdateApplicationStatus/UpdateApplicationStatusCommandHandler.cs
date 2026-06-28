using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Applications.UpdateApplicationStatus;

/// <summary>
/// Handler for UpdateApplicationStatusCommand.
/// Handles status transitions: Reviewing, Shortlisted, Interviewed, Offered.
/// </summary>
internal sealed class UpdateApplicationStatusCommandHandler
    : ICommandHandler<UpdateApplicationStatusCommand, bool>
{
    private readonly IApplicationRepository _applicationRepository;

    public UpdateApplicationStatusCommandHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<Result<bool>> Handle(
        UpdateApplicationStatusCommand command,
        CancellationToken cancellationToken)
    {
        // Retrieve application
        var application = await _applicationRepository.GetByIdAsync(
            Domain.Applications.ApplicationId.Create(command.ApplicationId),
            cancellationToken);

        if (application == null)
            return Result.Failure<bool>(
                new Error("Application.NotFound", "Application not found."));

        // Parse target status
        if (!Enum.TryParse<ApplicationStatus>(command.TargetStatus, true, out var targetStatus))
            return Result.Failure<bool>(
                new Error("Application.InvalidStatus",
                    $"Invalid status: {command.TargetStatus}"));

        // Apply appropriate state transition based on target status
        Result result = targetStatus switch
        {
            ApplicationStatus.Reviewing => application.MoveToReviewing(command.ReviewerId),
            ApplicationStatus.Shortlisted => application.Shortlist(command.ReviewerId, command.Notes),
            ApplicationStatus.Interviewed => application.MarkAsInterviewed(command.ReviewerId, command.Notes),
            ApplicationStatus.Offered => application.Offer(command.ReviewerId, command.Notes),
            _ => Result.Failure(new Error("Application.InvalidStatusTransition",
                $"Cannot transition to status: {targetStatus}. Use specific commands for Reject, Accept, or Withdraw."))
        };

        if (result.IsFailure)
            return Result.Failure<bool>(result.Error);

        // Persist changes
        await _applicationRepository.UpdateAsync(application, cancellationToken);

        return Result.Success(true);
    }
}
