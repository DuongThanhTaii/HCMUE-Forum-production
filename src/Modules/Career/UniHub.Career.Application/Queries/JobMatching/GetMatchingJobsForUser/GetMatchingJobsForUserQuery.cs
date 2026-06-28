using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.JobMatching.GetMatchingJobsForUser;

public sealed record GetMatchingJobsForUserQuery(
    Guid UserId,
    List<string> Skills,
    string? ExperienceLevel,
    List<string>? JobTypes,
    string? PreferredCity,
    bool PreferRemote,
    decimal? MinSalary,
    decimal? MaxSalary,
    int Page = 1,
    int PageSize = 20
) : IQuery<JobMatchingResponse>;

public sealed record JobMatchingResponse(
    List<JobMatchDto> Matches,
    int TotalMatches,
    int CurrentPage,
    int PageSize,
    int TotalPages,
    MatchingMetadata Metadata
);

public sealed record JobMatchDto(
    Guid JobPostingId,
    string Title,
    string CompanyName,
    Guid CompanyId,
    int MatchPercentage,
    MatchBreakdown Breakdown,
    string? JobType,
    string? ExperienceLevel,
    string? City,
    bool IsRemote,
    SalaryInfo? Salary,
    List<string> RequiredSkills,
    List<string> MatchingSkills,
    List<string> MissingSkills,
    DateTime PublishedAt,
    int ViewCount,
    int ApplicationCount
);

public sealed record MatchBreakdown(
    int SkillsScore,
    int ExperienceLevelScore,
    int LocationScore,
    int SalaryScore,
    int JobTypeScore,
    int RecencyScore
);

public sealed record SalaryInfo(
    decimal MinAmount,
    decimal MaxAmount,
    string Currency,
    string Period
);

public sealed record MatchingMetadata(
    int TotalJobsEvaluated,
    double AverageMatchPercentage,
    TimeSpan ProcessingTime,
    List<string> UserSkills,
    string? PreferredExperienceLevel
);
