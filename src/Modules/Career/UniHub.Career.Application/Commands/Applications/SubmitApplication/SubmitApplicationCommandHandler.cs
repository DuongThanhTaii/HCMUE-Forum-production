using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.Applications.SubmitApplication;

/// <summary>
/// Handler for SubmitApplicationCommand.
/// Validates job posting exists and is accepting applications, then creates the application.
/// </summary>
internal sealed class SubmitApplicationCommandHandler
    : ICommandHandler<SubmitApplicationCommand, ApplicationResponse>
{
    private readonly IApplicationRepository _applicationRepository;
    private readonly IJobPostingRepository _jobPostingRepository;

    public SubmitApplicationCommandHandler(
        IApplicationRepository applicationRepository,
        IJobPostingRepository jobPostingRepository)
    {
        _applicationRepository = applicationRepository;
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<Result<ApplicationResponse>> Handle(
        SubmitApplicationCommand command,
        CancellationToken cancellationToken)
    {
        // Verify job posting exists and is accepting applications
        var jobPosting = await _jobPostingRepository.GetByIdAsync(
            JobPostingId.Create(command.JobPostingId),
            cancellationToken);

        if (jobPosting == null)
            return Result.Failure<ApplicationResponse>(
                new Error("JobPosting.NotFound", "Job posting not found."));

        if (!jobPosting.IsAcceptingApplications())
            return Result.Failure<ApplicationResponse>(
                new Error("JobPosting.NotAcceptingApplications",
                    "This job posting is not currently accepting applications."));

        // Check for duplicate application
        var existingApplication = await _applicationRepository.GetByJobAndApplicantAsync(
            JobPostingId.Create(command.JobPostingId),
            command.ApplicantId,
            cancellationToken);

        if (existingApplication != null)
            return Result.Failure<ApplicationResponse>(
                new Error("Application.AlreadyExists",
                    "You have already applied to this job posting."));

        // Create resume value object
        var resumeResult = Resume.Create(
            command.ResumeFileName,
            command.ResumeFileUrl,
            command.ResumeFileSizeBytes,
            command.ResumeContentType);

        if (resumeResult.IsFailure)
            return Result.Failure<ApplicationResponse>(resumeResult.Error);

        // Create cover letter if provided
        CoverLetter? coverLetter = null;
        if (!string.IsNullOrWhiteSpace(command.CoverLetterContent))
        {
            var coverLetterResult = CoverLetter.Create(command.CoverLetterContent);
            if (coverLetterResult.IsFailure)
                return Result.Failure<ApplicationResponse>(coverLetterResult.Error);
            coverLetter = coverLetterResult.Value;
        }

        // Submit application
        var applicationResult = Domain.Applications.Application.Submit(
            JobPostingId.Create(command.JobPostingId),
            command.ApplicantId,
            resumeResult.Value,
            coverLetter);

        if (applicationResult.IsFailure)
            return Result.Failure<ApplicationResponse>(applicationResult.Error);

        var application = applicationResult.Value;

        // Persist application
        await _applicationRepository.AddAsync(application, cancellationToken);

        // Increment job posting application count
        jobPosting.IncrementApplicationCount();
        await _jobPostingRepository.UpdateAsync(jobPosting, cancellationToken);

        // Map to response
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
