using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Applications.AcceptApplication;

/// <summary>
/// Handler for AcceptApplicationCommand.
/// Allows applicants to accept job offers.
/// </summary>
internal sealed class AcceptApplicationCommandHandler
    : ICommandHandler<AcceptApplicationCommand, bool>
{
    private readonly IApplicationRepository _applicationRepository;

    public AcceptApplicationCommandHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<Result<bool>> Handle(
        AcceptApplicationCommand command,
        CancellationToken cancellationToken)
    {
        // Retrieve application
        var application = await _applicationRepository.GetByIdAsync(
            Domain.Applications.ApplicationId.Create(command.ApplicationId),
            cancellationToken);

        if (application == null)
            return Result.Failure<bool>(
                new Error("Application.NotFound", "Application not found."));

        // Accept application (domain validates applicant ID permission and Offered status)
        var result = application.Accept(command.ApplicantId);

        if (result.IsFailure)
            return Result.Failure<bool>(result.Error);

        // Persist changes
        await _applicationRepository.UpdateAsync(application, cancellationToken);

        return Result.Success(true);
    }
}
