using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Applications.WithdrawApplication;

/// <summary>
/// Handler for WithdrawApplicationCommand.
/// Allows applicants to withdraw their applications.
/// </summary>
internal sealed class WithdrawApplicationCommandHandler
    : ICommandHandler<WithdrawApplicationCommand, bool>
{
    private readonly IApplicationRepository _applicationRepository;

    public WithdrawApplicationCommandHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<Result<bool>> Handle(
        WithdrawApplicationCommand command,
        CancellationToken cancellationToken)
    {
        // Retrieve application
        var application = await _applicationRepository.GetByIdAsync(
            Domain.Applications.ApplicationId.Create(command.ApplicationId),
            cancellationToken);

        if (application == null)
            return Result.Failure<bool>(
                new Error("Application.NotFound", "Application not found."));

        // Withdraw application (domain validates applicant ID permission)
        var result = application.Withdraw(command.ApplicantId, command.Reason);

        if (result.IsFailure)
            return Result.Failure<bool>(result.Error);

        // Persist changes
        await _applicationRepository.UpdateAsync(application, cancellationToken);

        return Result.Success(true);
    }
}
