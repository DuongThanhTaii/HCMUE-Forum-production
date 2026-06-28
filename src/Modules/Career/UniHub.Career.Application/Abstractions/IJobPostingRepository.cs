using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Application.Abstractions;

/// <summary>
/// Repository interface for JobPosting aggregate.
/// </summary>
public interface IJobPostingRepository
{
    /// <summary>
    /// Adds a new job posting to the repository.
    /// </summary>
    Task AddAsync(JobPosting jobPosting, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing job posting.
    /// </summary>
    Task UpdateAsync(JobPosting jobPosting, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a job posting by its ID.
    /// </summary>
    Task<JobPosting?> GetByIdAsync(JobPostingId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of job postings with optional filters.
    /// Returns the list of job postings and the total count.
    /// </summary>
    Task<(List<JobPosting> JobPostings, int TotalCount)> GetAllAsync(
        int page,
        int pageSize,
        Guid? companyId = null,
        JobType? jobType = null,
        ExperienceLevel? experienceLevel = null,
        JobPostingStatus? status = null,
        string? city = null,
        bool? isRemote = null,
        string? searchTerm = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all job postings for a specific company.
    /// </summary>
    Task<List<JobPosting>> GetByCompanyAsync(Guid companyId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all published job postings that are accepting applications.
    /// </summary>
    Task<List<JobPosting>> GetPublishedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a job posting (soft delete recommended).
    /// </summary>
    Task DeleteAsync(JobPostingId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a job posting exists.
    /// </summary>
    Task<bool> ExistsAsync(JobPostingId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs advanced search on job postings with multiple filters and returns results without pagination applied.
    /// The handler will apply scoring, sorting, and pagination logic.
    /// </summary>
    Task<(List<JobPosting> JobPostings, int TotalCount)> SearchAsync(
        string? keywords = null,
        Guid? companyId = null,
        JobType? jobType = null,
        ExperienceLevel? experienceLevel = null,
        JobPostingStatus? status = null,
        string? city = null,
        bool? isRemote = null,
        decimal? minSalary = null,
        decimal? maxSalary = null,
        string? currency = null,
        List<string>? requiredSkills = null,
        List<string>? tags = null,
        DateTime? postedAfter = null,
        DateTime? postedBefore = null,
        CancellationToken cancellationToken = default);
}
