using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.JobMatching.GetMatchingCandidatesForJob;

public sealed record GetMatchingCandidatesForJobQuery(
    Guid JobPostingId,
    Guid CompanyId,
    int MinimumMatchPercentage = 30,
    int Page = 1,
    int PageSize = 20
) : IQuery<CandidateMatchingResponse>;

public sealed record CandidateMatchingResponse(
    List<CandidateMatchDto> Matches,
    int TotalMatches,
    int CurrentPage,
    int PageSize,
    int TotalPages,
    JobMatchingMetadata Metadata
);

public sealed record CandidateMatchDto(
    Guid ApplicationId,
    Guid ApplicantId,
    string ApplicantName,
    int MatchPercentage,
    CandidateMatchBreakdown Breakdown,
    List<string> MatchingSkills,
    List<string> AdditionalSkills,
    string ApplicationStatus,
    DateTime AppliedAt,
    bool HasCoverLetter,
    string ResumeUrl
);

public sealed record CandidateMatchBreakdown(
    int SkillsMatchScore,
    int ApplicationQualityScore,
    int TimingScore
);

public sealed record JobMatchingMetadata(
    Guid JobPostingId,
    string JobTitle,
    List<string> RequiredSkills,
    int TotalApplicationsEvaluated,
    double AverageMatchPercentage
);
