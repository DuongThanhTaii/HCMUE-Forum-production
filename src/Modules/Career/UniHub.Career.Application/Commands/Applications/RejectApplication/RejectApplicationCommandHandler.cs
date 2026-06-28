using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Applications.RejectApplication;

/// <summary>
/// Handler for RejectApplicationCommand.
/// Allows recruiters to reject applications.
/// </summary>
internal sealed class RejectApplicationCommandHandler
    : ICommandHandler<RejectApplicationCommand, bool>
{
    private readonly IApplicationRepository _applicationRepository;

    public RejectApplicationCommandHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<Result<bool>> Handle(
        RejectApplicationCommand command,
        CancellationToken cancellationToken)
    {
        // Retrieve application
        var application = await _applicationRepository.GetByIdAsync(
            Domain.Applications.ApplicationId.Create(command.ApplicationId),
            cancellationToken);

        if (application == null)
            return Result.Failure<bool>(
                new Error("Application.NotFound", "Application not found."));

        // Reject application
        var result = application.Reject(command.ReviewerId, command.Reason);

        if (result.IsFailure)
            return Result.Failure<bool>(result.Error);

        // Persist changes
        await _applicationRepository.UpdateAsync(application, cancellationToken);

        return Result.Success(true);
    }
}
