using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;
using DomainApplication = UniHub.Career.Domain.Applications.Application;
using ApplicationStatus = UniHub.Career.Domain.Applications.ApplicationStatus;

namespace UniHub.Career.Application.Queries.JobMatching.GetMatchingCandidatesForJob;

internal sealed class GetMatchingCandidatesForJobQueryHandler
    : IQueryHandler<GetMatchingCandidatesForJobQuery, CandidateMatchingResponse>
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly IApplicationRepository _applicationRepository;
    private readonly ICompanyRepository _companyRepository;
    
    private const int SkillsMatchWeight = 70;
    private const int ApplicationQualityWeight = 20;
    private const int TimingWeight = 10;

    public GetMatchingCandidatesForJobQueryHandler(
        IJobPostingRepository jobPostingRepository,
        IApplicationRepository applicationRepository,
        ICompanyRepository companyRepository)
    {
        _jobPostingRepository = jobPostingRepository;
        _applicationRepository = applicationRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<CandidateMatchingResponse>> Handle(
        GetMatchingCandidatesForJobQuery request,
        CancellationToken cancellationToken)
    {
        // Verify company owns the job posting
        var company = await _companyRepository.GetByIdAsync(
            CompanyId.Create(request.CompanyId),
            cancellationToken);
        
        if (company == null)
        {
            return Result.Failure<CandidateMatchingResponse>(
                new Error("Company.NotFound", "Company not found"));
        }
        
        // Get the job posting
        var jobPostingId = JobPostingId.Create(request.JobPostingId);
        var jobPosting = await _jobPostingRepository.GetByIdAsync(jobPostingId, cancellationToken);
        
        if (jobPosting == null)
        {
            return Result.Failure<CandidateMatchingResponse>(
                new Error("JobPosting.NotFound", "Job posting not found"));
        }
        
        // Verify company owns the job
        if (jobPosting.CompanyId != request.CompanyId)
        {
            return Result.Failure<CandidateMatchingResponse>(
                new Error("JobPosting.Unauthorized", "Company does not own this job posting"));
        }
        
        // Get all applications for this job
        var (applications, _) = await _applicationRepository.GetByJobPostingAsync(
            jobPostingId,
            status: null,
            page: 1,
            pageSize: int.MaxValue, // Get all applications for matching
            cancellationToken);
        
        if (!applications.Any())
        {
            return Result<CandidateMatchingResponse>.Success(new CandidateMatchingResponse(
                Matches: new List<CandidateMatchDto>(),
                TotalMatches: 0,
                CurrentPage: request.Page,
                PageSize: request.PageSize,
                TotalPages: 0,
                Metadata: new JobMatchingMetadata(
                    JobPostingId: request.JobPostingId,
                    JobTitle: jobPosting.Title,
                    RequiredSkills: jobPosting.Requirements.Select(r => r.Skill).ToList(),
                    TotalApplicationsEvaluated: 0,
                    AverageMatchPercentage: 0
                )
            ));
        }
        
        // Calculate match scores for each candidate
        // For this implementation, we'll use a simplified scoring based on application data
        // In a real system, you'd fetch candidate profiles with their skills
        var matchedCandidates = new List<(DomainApplication Application, int MatchPercentage, CandidateMatchBreakdown Breakdown)>();
        
        var requiredSkills = jobPosting.Requirements.Select(r => r.Skill.ToLower()).ToList();
        var firstApplicationDate = applications.Min(a => a.SubmittedAt);
        
        foreach (var application in applications)
        {
            var (matchPercentage, breakdown) = CalculateCandidateMatchScore(
                application,
                requiredSkills,
                firstApplicationDate);
            
            if (matchPercentage >= request.MinimumMatchPercentage)
            {
                matchedCandidates.Add((application, matchPercentage, breakdown));
            }
        }
        
        // Sort by match percentage descending
        var sortedMatches = matchedCandidates.OrderByDescending(m => m.MatchPercentage).ToList();
        
        // Apply pagination
        var totalMatches = sortedMatches.Count;
        var totalPages = (int)Math.Ceiling(totalMatches / (double)request.PageSize);
        
        var paginatedMatches = sortedMatches
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        // Build response DTOs
        var matchDtos = paginatedMatches.Select(match => new CandidateMatchDto(
            ApplicationId: match.Application.Id.Value,
            ApplicantId: match.Application.ApplicantId,
            ApplicantName: $"Applicant {match.Application.ApplicantId.ToString().Substring(0, 8)}", // Placeholder
            MatchPercentage: match.MatchPercentage,
            Breakdown: match.Breakdown,
            MatchingSkills: new List<string>(), // Simplified - would need candidate profile
            AdditionalSkills: new List<string>(),
            ApplicationStatus: match.Application.Status.ToString(),
            AppliedAt: match.Application.SubmittedAt,
            HasCoverLetter: match.Application.CoverLetter != null,
            ResumeUrl: match.Application.Resume.FileUrl
        )).ToList();
        
        var averageMatch = matchedCandidates.Any()
            ? matchedCandidates.Average(m => m.MatchPercentage)
            : 0;
        
        var metadata = new JobMatchingMetadata(
            JobPostingId: request.JobPostingId,
            JobTitle: jobPosting.Title,
            RequiredSkills: requiredSkills,
            TotalApplicationsEvaluated: applications.Count,
            AverageMatchPercentage: Math.Round(averageMatch, 2)
        );
        
        var response = new CandidateMatchingResponse(
            Matches: matchDtos,
            TotalMatches: totalMatches,
            CurrentPage: request.Page,
            PageSize: request.PageSize,
            TotalPages: totalPages,
            Metadata: metadata
        );
        
        return Result<CandidateMatchingResponse>.Success(response);
    }

    private (int MatchPercentage, CandidateMatchBreakdown Breakdown) CalculateCandidateMatchScore(
        DomainApplication application,
        List<string> requiredSkills,
        DateTime firstApplicationDate)
    {
        // 1. Skills match score (70%)
        // In a real implementation, this would compare candidate skills with job requirements
        // For now, we'll use a placeholder score based on application status
        var skillsScore = application.Status switch
        {
            ApplicationStatus.Accepted => SkillsMatchWeight,
            ApplicationStatus.Offered => (int)(SkillsMatchWeight * 0.9),
            ApplicationStatus.Interviewed => (int)(SkillsMatchWeight * 0.8),
            ApplicationStatus.Shortlisted => (int)(SkillsMatchWeight * 0.7),
            ApplicationStatus.Reviewing => (int)(SkillsMatchWeight * 0.6),
            ApplicationStatus.Pending => (int)(SkillsMatchWeight * 0.5),
            _ => 0
        };
        
        // 2. Application quality score (20%)
        var qualityScore = 0;
        
        // Has cover letter: +10 points
        if (application.CoverLetter != null)
        {
            qualityScore += ApplicationQualityWeight / 2;
        }
        
        // Has resume (always true, required): +10 points
        if (application.Resume != null)
        {
            qualityScore += ApplicationQualityWeight / 2;
        }
        
        // 3. Timing score (10%)
        // Early applicants get bonus points
        var daysSinceFirst = (application.SubmittedAt - firstApplicationDate).TotalDays;
        var timingScore = daysSinceFirst switch
        {
            <= 1 => TimingWeight,
            <= 7 => (int)(TimingWeight * 0.7),
            <= 14 => (int)(TimingWeight * 0.5),
            _ => (int)(TimingWeight * 0.3)
        };
        
        var totalScore = skillsScore + qualityScore + timingScore;
        
        var breakdown = new CandidateMatchBreakdown(
            SkillsMatchScore: skillsScore,
            ApplicationQualityScore: qualityScore,
            TimingScore: timingScore
        );
        
        return (totalScore, breakdown);
    }
}
