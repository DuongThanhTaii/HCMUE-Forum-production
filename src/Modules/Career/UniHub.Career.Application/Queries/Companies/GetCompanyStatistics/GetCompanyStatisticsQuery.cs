using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.Companies.GetCompanyStatistics;

/// <summary>
/// Query to get comprehensive statistics for a company dashboard.
/// </summary>
public sealed record GetCompanyStatisticsQuery(
    Guid CompanyId
) : IRequest<Result<CompanyStatisticsResponse>>;

/// <summary>
/// Response containing company dashboard statistics.
/// </summary>
public sealed record CompanyStatisticsResponse(
    Guid CompanyId,
    string CompanyName,
    CompanyOverviewStats Overview,
    JobPostingStats JobPostings,
    ApplicationStats Applications,
    List<TopJobPosting> TopPerformingJobs
);

/// <summary>
/// Overall company statistics.
/// </summary>
public sealed record CompanyOverviewStats(
    int TotalJobPostings,
    int ActiveJobPostings,
    int TotalApplications,
    int TotalViews,
    DateTime? LastJobPostedAt
);

/// <summary>
/// Job posting breakdown statistics.
/// </summary>
public sealed record JobPostingStats(
    int Draft,
    int Published,
    int Paused,
    int Closed,
    int Expired
);

/// <summary>
/// Application breakdown statistics.
/// </summary>
public sealed record ApplicationStats(
    int Pending,
    int Reviewing,
    int Shortlisted,
    int Interviewed,
    int Offered,
    int Accepted,
    int Rejected,
    int Withdrawn,
    double AcceptanceRate,
    double RejectionRate
);

/// <summary>
/// Top performing job posting summary.
/// </summary>
public sealed record TopJobPosting(
    Guid JobPostingId,
    string Title,
    int ApplicationCount,
    int ViewCount,
    DateTime PublishedAt
);
