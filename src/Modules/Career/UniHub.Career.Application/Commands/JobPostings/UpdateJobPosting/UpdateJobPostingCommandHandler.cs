using UniHub.Career.Application.Abstractions;
using UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.JobPostings.UpdateJobPosting;

/// <summary>
/// Handler for updating an existing job posting.
/// </summary>
public sealed class UpdateJobPostingCommandHandler
    : ICommandHandler<UpdateJobPostingCommand, JobPostingResponse>
{
    private readonly IJobPostingRepository _jobPostingRepository;

    public UpdateJobPostingCommandHandler(IJobPostingRepository jobPostingRepository)
    {
        _jobPostingRepository = jobPostingRepository;
    }

    public async Task<Result<JobPostingResponse>> Handle(
        UpdateJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        // Retrieve job posting
        var jobPosting = await _jobPostingRepository.GetByIdAsync(
            JobPostingId.Create(command.JobPostingId), 
            cancellationToken);

        if (jobPosting == null)
            return Result.Failure<JobPostingResponse>(
                new Error("JobPosting.NotFound", $"Job posting with ID {command.JobPostingId} not found"));

        // Create WorkLocation value object
        var locationResult = WorkLocation.Create(
            command.City,
            command.District,
            command.Address,
            command.IsRemote);

        if (locationResult.IsFailure)
            return Result.Failure<JobPostingResponse>(locationResult.Error);

        // Create SalaryRange value object (if provided)
        SalaryRange? salary = null;
        if (command.MinSalary.HasValue && command.MaxSalary.HasValue)
        {
            if (string.IsNullOrWhiteSpace(command.SalaryCurrency) || 
                string.IsNullOrWhiteSpace(command.SalaryPeriod))
            {
                return Result.Failure<JobPostingResponse>(
                    new Error("Salary.CurrencyAndPeriodRequired", 
                            "Currency and period are required when salary is specified"));
            }

            var salaryResult = SalaryRange.Create(
                command.MinSalary.Value,
                command.MaxSalary.Value,
                command.SalaryCurrency,
                command.SalaryPeriod);

            if (salaryResult.IsFailure)
                return Result.Failure<JobPostingResponse>(salaryResult.Error);

            salary = salaryResult.Value;
        }

        // Update job posting
        var updateResult = jobPosting.Update(
            command.Title,
            command.Description,
            command.JobType,
            command.ExperienceLevel,
            locationResult.Value,
            salary,
            command.Deadline);

        if (updateResult.IsFailure)
            return Result.Failure<JobPostingResponse>(updateResult.Error);

        // Persist changes
        await _jobPostingRepository.UpdateAsync(jobPosting, cancellationToken);

        // Map to response DTO
        var response = MapToResponse(jobPosting);

        return Result.Success(response);
    }

    private static JobPostingResponse MapToResponse(JobPosting jobPosting)
    {
        return new JobPostingResponse(
            jobPosting.Id.Value,
            jobPosting.Title,
            jobPosting.Description,
            jobPosting.CompanyId,
            jobPosting.PostedBy,
            jobPosting.JobType.ToString(),
            jobPosting.ExperienceLevel.ToString(),
            jobPosting.Status.ToString(),
            jobPosting.Salary != null 
                ? new SalaryInfo(
                    jobPosting.Salary.MinAmount,
                    jobPosting.Salary.MaxAmount,
                    jobPosting.Salary.Currency,
                    jobPosting.Salary.Period)
                : null,
            new LocationInfo(
                jobPosting.Location.City,
                jobPosting.Location.District,
                jobPosting.Location.Address,
                jobPosting.Location.IsRemote),
            jobPosting.Deadline,
            jobPosting.CreatedAt,
            jobPosting.UpdatedAt,
            jobPosting.PublishedAt,
            jobPosting.ViewCount,
            jobPosting.ApplicationCount,
            jobPosting.Tags.ToList());
    }
}
