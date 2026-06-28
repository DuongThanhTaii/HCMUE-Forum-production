using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using DomainApplication = UniHub.Career.Domain.Applications.Application;
using DomainApplicationId = UniHub.Career.Domain.Applications.ApplicationId;

namespace UniHub.Career.Application.Abstractions;

/// <summary>
/// Repository interface for Application aggregate.
/// </summary>
public interface IApplicationRepository
{
    /// <summary>
    /// Adds a new application to the repository.
    /// </summary>
    Task AddAsync(DomainApplication application, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing application.
    /// </summary>
    Task UpdateAsync(DomainApplication application, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an application by its ID.
    /// </summary>
    Task<DomainApplication?> GetByIdAsync(DomainApplicationId id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an application by job posting and applicant.
    /// Used to check for duplicate applications.
    /// </summary>
    Task<DomainApplication?> GetByJobAndApplicantAsync(
        JobPostingId jobPostingId,
        Guid applicantId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all applications for a specific job posting with pagination and optional status filter.
    /// </summary>
    Task<(List<DomainApplication> Applications, int TotalCount)> GetByJobPostingAsync(
        JobPostingId jobPostingId,
        ApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all applications submitted by a specific applicant with pagination and optional status filter.
    /// </summary>
    Task<(List<DomainApplication> Applications, int TotalCount)> GetByApplicantAsync(
        Guid applicantId,
        ApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all applications for all job postings of a specific company.
    /// </summary>
    Task<(List<DomainApplication> Applications, int TotalCount)> GetByCompanyAsync(
        Guid companyId,
        ApplicationStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an application exists.
    /// </summary>
    Task<bool> ExistsAsync(DomainApplicationId id, CancellationToken cancellationToken = default);
}
