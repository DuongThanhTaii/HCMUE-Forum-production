using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Commands.JobPostings.CreateJobPosting;

/// <summary>
/// Handler for creating a new job posting.
/// </summary>
public sealed class CreateJobPostingCommandHandler
    : ICommandHandler<CreateJobPostingCommand, JobPostingResponse>
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly ICompanyRepository _companyRepository;

    public CreateJobPostingCommandHandler(
        IJobPostingRepository jobPostingRepository,
        ICompanyRepository companyRepository)
    {
        _jobPostingRepository = jobPostingRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<JobPostingResponse>> Handle(
        CreateJobPostingCommand command,
        CancellationToken cancellationToken)
    {
        var company = await _companyRepository.GetByIdAsync(
            CompanyId.Create(command.CompanyId),
            cancellationToken);

        if (company is null)
        {
            return Result.Failure<JobPostingResponse>(
                new Error("Company.NotFound", $"Company with ID {command.CompanyId} was not found."));
        }

        if (!company.CanPostJobs())
        {
            return Result.Failure<JobPostingResponse>(
                new Error("Company.NotVerified", "Company is pending approval by Admin and cannot post jobs yet."));
        }

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

        // Create JobPosting aggregate
        var jobPostingResult = JobPosting.Create(
            command.Title,
            command.Description,
            command.CompanyId,
            command.PostedBy,
            command.JobType,
            command.ExperienceLevel,
            locationResult.Value,
            salary,
            command.Deadline);

        if (jobPostingResult.IsFailure)
            return Result.Failure<JobPostingResponse>(jobPostingResult.Error);

        var jobPosting = jobPostingResult.Value;

        // Persist to repository
        await _jobPostingRepository.AddAsync(jobPosting, cancellationToken);

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
