using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;
using System.Diagnostics;

namespace UniHub.Career.Application.Queries.JobMatching.GetMatchingJobsForUser;

internal sealed class GetMatchingJobsForUserQueryHandler
    : IQueryHandler<GetMatchingJobsForUserQuery, JobMatchingResponse>
{
    private readonly IJobPostingRepository _jobPostingRepository;
    private readonly ICompanyRepository _companyRepository;
    
    private const int MinimumMatchThreshold = 30;
    private const int SkillsWeight = 40;
    private const int ExperienceLevelWeight = 20;
    private const int LocationWeight = 15;
    private const int SalaryWeight = 15;
    private const int JobTypeWeight = 5;
    private const int RecencyWeight = 5;

    public GetMatchingJobsForUserQueryHandler(
        IJobPostingRepository jobPostingRepository,
        ICompanyRepository companyRepository)
    {
        _jobPostingRepository = jobPostingRepository;
        _companyRepository = companyRepository;
    }

    public async Task<Result<JobMatchingResponse>> Handle(
        GetMatchingJobsForUserQuery request, 
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        // Get all published job postings
        var jobPostings = await _jobPostingRepository.GetPublishedAsync(cancellationToken);
        
        // Calculate match scores for each job
        var matchedJobs = new List<(JobPosting Job, int MatchPercentage, MatchBreakdown Breakdown, List<string> MatchingSkills, List<string> MissingSkills)>();
        
        foreach (var job in jobPostings)
        {
            var (matchPercentage, breakdown, matchingSkills, missingSkills) = CalculateMatchScore(job, request);
            
            if (matchPercentage >= MinimumMatchThreshold)
            {
                matchedJobs.Add((job, matchPercentage, breakdown, matchingSkills, missingSkills));
            }
        }
        
        // Sort by match percentage descending
        var sortedMatches = matchedJobs.OrderByDescending(m => m.MatchPercentage).ToList();
        
        // Apply pagination
        var totalMatches = sortedMatches.Count;
        var totalPages = (int)Math.Ceiling(totalMatches / (double)request.PageSize);
        
        var paginatedMatches = sortedMatches
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();
        
        // Enrich with company names
        var matchDtos = new List<JobMatchDto>();
        
        foreach (var match in paginatedMatches)
        {
            var company = await _companyRepository.GetByIdAsync(
                Domain.Companies.CompanyId.Create(match.Job.CompanyId),
                cancellationToken);
            
            if (company == null)
                continue;
            
            var jobRequirements = match.Job.Requirements.Select(r => r.Skill).ToList();
            
            matchDtos.Add(new JobMatchDto(
                JobPostingId: match.Job.Id.Value,
                Title: match.Job.Title,
                CompanyName: company.Name,
                CompanyId: match.Job.CompanyId,
                MatchPercentage: match.MatchPercentage,
                Breakdown: match.Breakdown,
                JobType: match.Job.JobType.ToString(),
                ExperienceLevel: match.Job.ExperienceLevel.ToString(),
                City: match.Job.Location.City,
                IsRemote: match.Job.Location.IsRemote,
                Salary: match.Job.Salary != null 
                    ? new SalaryInfo(
                        match.Job.Salary.MinAmount,
                        match.Job.Salary.MaxAmount,
                        match.Job.Salary.Currency.ToString(),
                        match.Job.Salary.Period.ToString())
                    : null,
                RequiredSkills: jobRequirements,
                MatchingSkills: match.MatchingSkills,
                MissingSkills: match.MissingSkills,
                PublishedAt: match.Job.PublishedAt ?? DateTime.UtcNow,
                ViewCount: match.Job.ViewCount,
                ApplicationCount: match.Job.ApplicationCount
            ));
        }
        
        stopwatch.Stop();
        
        var averageMatch = matchedJobs.Any() 
            ? matchedJobs.Average(m => m.MatchPercentage) 
            : 0;
        
        var metadata = new MatchingMetadata(
            TotalJobsEvaluated: jobPostings.Count,
            AverageMatchPercentage: Math.Round(averageMatch, 2),
            ProcessingTime: stopwatch.Elapsed,
            UserSkills: request.Skills,
            PreferredExperienceLevel: request.ExperienceLevel
        );
        
        var response = new JobMatchingResponse(
            Matches: matchDtos,
            TotalMatches: totalMatches,
            CurrentPage: request.Page,
            PageSize: request.PageSize,
            TotalPages: totalPages,
            Metadata: metadata
        );
        
        return Result<JobMatchingResponse>.Success(response);
    }

    private (int MatchPercentage, MatchBreakdown Breakdown, List<string> MatchingSkills, List<string> MissingSkills) CalculateMatchScore(
        JobPosting job, 
        GetMatchingJobsForUserQuery request)
    {
        // 1. Skills matching (40%)
        var jobSkills = job.Requirements.Select(r => r.Skill.ToLower()).ToList();
        var userSkills = request.Skills.Select(s => s.ToLower()).ToList();
        
        var matchingSkills = userSkills.Intersect(jobSkills).ToList();
        var missingSkills = jobSkills.Except(userSkills).ToList();
        
        var skillsScore = jobSkills.Any()
            ? (int)Math.Round((matchingSkills.Count / (double)jobSkills.Count) * SkillsWeight)
            : 0;
        
        // 2. Experience level matching (20%)
        var experienceLevelScore = 0;
        if (!string.IsNullOrEmpty(request.ExperienceLevel))
        {
            experienceLevelScore = job.ExperienceLevel.ToString().Equals(request.ExperienceLevel, StringComparison.OrdinalIgnoreCase)
                ? ExperienceLevelWeight
                : ExperienceLevelWeight / 2; // Partial match for adjacent levels
        }
        
        // 3. Location matching (15%)
        var locationScore = 0;
        if (request.PreferRemote && job.Location.IsRemote)
        {
            locationScore = LocationWeight;
        }
        else if (!string.IsNullOrEmpty(request.PreferredCity) && 
                 job.Location.City.Equals(request.PreferredCity, StringComparison.OrdinalIgnoreCase))
        {
            locationScore = LocationWeight;
        }
        else if (!string.IsNullOrEmpty(request.PreferredCity))
        {
            locationScore = LocationWeight / 3; // Partial match for different city
        }
        
        // 4. Salary matching (15%)
        var salaryScore = 0;
        if (job.Salary != null && (request.MinSalary.HasValue || request.MaxSalary.HasValue))
        {
            var userMin = request.MinSalary ?? 0;
            var userMax = request.MaxSalary ?? decimal.MaxValue;
            var jobMin = job.Salary.MinAmount;
            var jobMax = job.Salary.MaxAmount;
            
            // Check if ranges overlap
            if (jobMax >= userMin && jobMin <= userMax)
            {
                // Full match if job salary is within or above user expectations
                if (jobMin >= userMin && jobMax >= userMax)
                {
                    salaryScore = SalaryWeight;
                }
                else
                {
                    salaryScore = SalaryWeight / 2; // Partial match
                }
            }
        }
        
        // 5. Job type matching (5%)
        var jobTypeScore = 0;
        if (request.JobTypes != null && request.JobTypes.Any())
        {
            jobTypeScore = request.JobTypes.Any(jt => 
                job.JobType.ToString().Equals(jt, StringComparison.OrdinalIgnoreCase))
                ? JobTypeWeight
                : 0;
        }
        
        // 6. Recency score (5%) - prefer recent postings
        var recencyScore = 0;
        if (job.PublishedAt.HasValue)
        {
            var daysSincePublished = (DateTime.UtcNow - job.PublishedAt.Value).TotalDays;
            if (daysSincePublished <= 7)
                recencyScore = RecencyWeight;
            else if (daysSincePublished <= 30)
                recencyScore = RecencyWeight * 2 / 3;
            else if (daysSincePublished <= 90)
                recencyScore = RecencyWeight / 3;
        }
        
        var totalScore = skillsScore + experienceLevelScore + locationScore + salaryScore + jobTypeScore + recencyScore;
        
        var breakdown = new MatchBreakdown(
            SkillsScore: skillsScore,
            ExperienceLevelScore: experienceLevelScore,
            LocationScore: locationScore,
            SalaryScore: salaryScore,
            JobTypeScore: jobTypeScore,
            RecencyScore: recencyScore
        );
        
        return (totalScore, breakdown, matchingSkills, missingSkills);
    }
}
