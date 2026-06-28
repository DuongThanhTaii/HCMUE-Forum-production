using System.Diagnostics;
using UniHub.Career.Application.Abstractions;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Application.Queries.JobPostings.SearchJobPostings;

internal sealed class SearchJobPostingsQueryHandler : IQueryHandler<SearchJobPostingsQuery, JobPostingSearchResponse>
{
    private readonly IJobPostingRepository _repository;

    public SearchJobPostingsQueryHandler(IJobPostingRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<JobPostingSearchResponse>> Handle(
        SearchJobPostingsQuery query,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // Get job postings from repository with filters
        var (jobPostings, totalCount) = await _repository.SearchAsync(
            keywords: query.Keywords,
            companyId: query.CompanyId,
            jobType: query.JobType,
            experienceLevel: query.ExperienceLevel,
            status: query.Status,
            city: query.City,
            isRemote: query.IsRemote,
            minSalary: query.MinSalary,
            maxSalary: query.MaxSalary,
            currency: query.Currency,
            requiredSkills: query.RequiredSkills,
            tags: query.Tags,
            postedAfter: query.PostedAfter,
            postedBefore: query.PostedBefore,
            cancellationToken);

        // Calculate relevance scores
        var scoredResults = jobPostings
            .Select(job => new
            {
                JobPosting = job,
                Score = CalculateRelevanceScore(job, query)
            })
            .ToList();

        // Sort based on sort criteria
        var sortedResults = query.SortBy switch
        {
            SearchSortBy.Relevance => query.SortDescending
                ? scoredResults.OrderByDescending(x => x.Score)
                : scoredResults.OrderBy(x => x.Score),
            SearchSortBy.Date => query.SortDescending
                ? scoredResults.OrderByDescending(x => x.JobPosting.PublishedAt ?? x.JobPosting.CreatedAt)
                : scoredResults.OrderBy(x => x.JobPosting.PublishedAt ?? x.JobPosting.CreatedAt),
            SearchSortBy.Salary => query.SortDescending
                ? scoredResults.OrderByDescending(x => x.JobPosting.Salary?.MaxAmount ?? 0)
                : scoredResults.OrderBy(x => x.JobPosting.Salary?.MaxAmount ?? 0),
            SearchSortBy.ViewCount => query.SortDescending
                ? scoredResults.OrderByDescending(x => x.JobPosting.ViewCount)
                : scoredResults.OrderBy(x => x.JobPosting.ViewCount),
            SearchSortBy.ApplicationCount => query.SortDescending
                ? scoredResults.OrderByDescending(x => x.JobPosting.ApplicationCount)
                : scoredResults.OrderBy(x => x.JobPosting.ApplicationCount),
            _ => scoredResults.OrderByDescending(x => x.Score)
        };

        // Apply pagination
        var pagedResults = sortedResults
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        // Map to response DTOs
        var items = pagedResults.Select(result => new JobPostingSearchResult(
            JobPostingId: result.JobPosting.Id.Value,
            Title: result.JobPosting.Title,
            Description: result.JobPosting.Description,
            CompanyId: result.JobPosting.CompanyId,
            JobType: result.JobPosting.JobType.ToString(),
            ExperienceLevel: result.JobPosting.ExperienceLevel.ToString(),
            Status: result.JobPosting.Status.ToString(),
            Salary: result.JobPosting.Salary is not null
                ? new SalaryInfo(
                    result.JobPosting.Salary.MinAmount,
                    result.JobPosting.Salary.MaxAmount,
                    result.JobPosting.Salary.Currency,
                    result.JobPosting.Salary.Period)
                : null,
            Location: new LocationInfo(
                result.JobPosting.Location.City,
                result.JobPosting.Location.District,
                result.JobPosting.Location.Address,
                result.JobPosting.Location.IsRemote),
            Requirements: result.JobPosting.Requirements.Select(r => r.Skill).ToList(),
            Tags: result.JobPosting.Tags.ToList(),
            CreatedAt: result.JobPosting.CreatedAt,
            PublishedAt: result.JobPosting.PublishedAt,
            ViewCount: result.JobPosting.ViewCount,
            ApplicationCount: result.JobPosting.ApplicationCount,
            RelevanceScore: result.Score
        )).ToList();

        stopwatch.Stop();

        var totalPages = (int)Math.Ceiling((double)totalCount / query.PageSize);

        var metadata = new SearchMetadata(
            SearchKeywords: query.Keywords,
            FiltersApplied: CountAppliedFilters(query),
            AverageRelevanceScore: items.Count > 0 ? items.Average(x => x.RelevanceScore) : 0,
            SearchDuration: stopwatch.Elapsed
        );

        var response = new JobPostingSearchResponse(
            Items: items,
            TotalCount: totalCount,
            Page: query.Page,
            PageSize: query.PageSize,
            TotalPages: totalPages,
            Metadata: metadata
        );

        return Result.Success(response);
    }

    private static double CalculateRelevanceScore(JobPosting jobPosting, SearchJobPostingsQuery query)
    {
        double score = 0.0;

        // Keyword matching in title (highest weight)
        if (!string.IsNullOrWhiteSpace(query.Keywords))
        {
            var keywords = query.Keywords.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var titleLower = jobPosting.Title.ToLower();
            var descriptionLower = jobPosting.Description.ToLower();

            foreach (var keyword in keywords)
            {
                if (titleLower.Contains(keyword))
                    score += 10.0;
                if (descriptionLower.Contains(keyword))
                    score += 5.0;
            }

            // Exact title match bonus
            if (titleLower.Contains(query.Keywords.ToLower()))
                score += 15.0;
        }

        // Tag matching
        if (query.Tags is not null && query.Tags.Count > 0)
        {
            var matchingTags = jobPosting.Tags.Count(tag =>
                query.Tags.Contains(tag, StringComparer.OrdinalIgnoreCase));
            score += matchingTags * 8.0;
        }

        // Required skills matching
        if (query.RequiredSkills is not null && query.RequiredSkills.Count > 0)
        {
            var jobSkills = jobPosting.Requirements.Select(r => r.Skill.ToLower()).ToList();
            var matchingSkills = query.RequiredSkills.Count(skill =>
                jobSkills.Contains(skill.ToLower()));
            score += matchingSkills * 12.0;
        }

        // Recency boost (newer jobs get higher score)
        var daysSincePosted = (DateTime.UtcNow - (jobPosting.PublishedAt ?? jobPosting.CreatedAt)).Days;
        if (daysSincePosted <= 1)
            score += 5.0;
        else if (daysSincePosted <= 7)
            score += 3.0;
        else if (daysSincePosted <= 30)
            score += 1.0;

        // Popularity boost
        score += jobPosting.ViewCount * 0.01;
        score += jobPosting.ApplicationCount * 0.5;

        // Remote job bonus (generally popular)
        if (jobPosting.Location.IsRemote)
            score += 2.0;

        // Normalize score to 0-100 range
        return Math.Min(100.0, Math.Max(0.0, score));
    }

    private static int CountAppliedFilters(SearchJobPostingsQuery query)
    {
        int count = 0;

        if (!string.IsNullOrWhiteSpace(query.Keywords)) count++;
        if (query.CompanyId.HasValue) count++;
        if (query.JobType.HasValue) count++;
        if (query.ExperienceLevel.HasValue) count++;
        if (query.Status.HasValue) count++;
        if (!string.IsNullOrWhiteSpace(query.City)) count++;
        if (query.IsRemote.HasValue) count++;
        if (query.MinSalary.HasValue || query.MaxSalary.HasValue) count++;
        if (query.RequiredSkills is not null && query.RequiredSkills.Count > 0) count++;
        if (query.Tags is not null && query.Tags.Count > 0) count++;
        if (query.PostedAfter.HasValue || query.PostedBefore.HasValue) count++;

        return count;
    }
}
