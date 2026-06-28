using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Career.Application.Queries.JobPostings.SearchJobPostings;

public sealed record SearchJobPostingsQuery(
    string? Keywords = null,
    Guid? CompanyId = null,
    JobType? JobType = null,
    ExperienceLevel? ExperienceLevel = null,
    JobPostingStatus? Status = null,
    string? City = null,
    bool? IsRemote = null,
    decimal? MinSalary = null,
    decimal? MaxSalary = null,
    string? Currency = null,
    List<string>? RequiredSkills = null,
    List<string>? Tags = null,
    DateTime? PostedAfter = null,
    DateTime? PostedBefore = null,
    SearchSortBy SortBy = SearchSortBy.Relevance,
    bool SortDescending = true,
    int Page = 1,
    int PageSize = 20
) : IQuery<JobPostingSearchResponse>;

public sealed record JobPostingSearchResponse(
    List<JobPostingSearchResult> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    SearchMetadata Metadata
);

public sealed record JobPostingSearchResult(
    Guid JobPostingId,
    string Title,
    string Description,
    Guid CompanyId,
    string JobType,
    string ExperienceLevel,
    string Status,
    SalaryInfo? Salary,
    LocationInfo Location,
    List<string> Requirements,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    int ViewCount,
    int ApplicationCount,
    double RelevanceScore
);

public sealed record SalaryInfo(
    decimal MinAmount,
    decimal MaxAmount,
    string Currency,
    string Period
);

public sealed record LocationInfo(
    string City,
    string? District,
    string? Address,
    bool IsRemote
);

public sealed record SearchMetadata(
    string? SearchKeywords,
    int FiltersApplied,
    double AverageRelevanceScore,
    TimeSpan SearchDuration
);

public enum SearchSortBy
{
    Relevance,
    Date,
    Salary,
    ViewCount,
    ApplicationCount
}
