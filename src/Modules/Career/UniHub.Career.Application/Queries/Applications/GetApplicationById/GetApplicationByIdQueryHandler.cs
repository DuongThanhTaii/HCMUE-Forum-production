using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.Applications.SubmitApplication;
using UniHub.Career.Domain.Applications;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Applications.GetApplicationById;

/// <summary>
/// Handler for GetApplicationByIdQuery.
/// </summary>
internal sealed class GetApplicationByIdQueryHandler
    : IQueryHandler<GetApplicationByIdQuery, ApplicationResponse>
{
    private readonly IApplicationRepository _applicationRepository;

    public GetApplicationByIdQueryHandler(IApplicationRepository applicationRepository)
    {
        _applicationRepository = applicationRepository;
    }

    public async Task<Result<ApplicationResponse>> Handle(
        GetApplicationByIdQuery query,
        CancellationToken cancellationToken)
    {
        var application = await _applicationRepository.GetByIdAsync(
            Domain.Applications.ApplicationId.Create(query.ApplicationId),
            cancellationToken);

        if (application == null)
            return Result.Failure<ApplicationResponse>(
                new Error("Application.NotFound", "Application not found."));

        var response = new ApplicationResponse(
            application.Id.Value,
            application.JobPostingId.Value,
            application.ApplicantId,
            application.Status.ToString(),
            new ResumeDto(
                application.Resume.FileName,
                application.Resume.FileUrl,
                application.Resume.FileSizeBytes,
                application.Resume.ContentType),
            application.CoverLetter?.Content,
            application.SubmittedAt,
            application.LastStatusChangedAt,
            application.LastStatusChangedBy,
            application.ReviewNotes);

        return Result.Success(response);
    }
}
