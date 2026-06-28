using UniHub.Career.Domain.JobPostings.Events;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// JobPosting Aggregate Root - represents a job listing posted by a company.
/// Manages the full lifecycle: Draft → Published → Paused/Closed/Expired.
/// </summary>
public sealed class JobPosting : AggregateRoot<JobPostingId>
{
    #region Constants

    public const int MaxTitleLength = 200;
    public const int MaxDescriptionLength = 10000;
    public const int MaxRequirements = 30;
    public const int MaxTags = 15;
    public const int MaxTagLength = 50;

    #endregion

    #region Properties

    /// <summary>Job title (e.g., ".NET Developer", "React Frontend Intern").</summary>
    public string Title { get; private set; }

    /// <summary>Detailed job description in Markdown or plain text.</summary>
    public string Description { get; private set; }

    /// <summary>The company that posted this job.</summary>
    public Guid CompanyId { get; private set; }

    /// <summary>The recruiter/user who created this posting.</summary>
    public Guid PostedBy { get; private set; }

    /// <summary>Type of job (FullTime, PartTime, Internship, etc.).</summary>
    public JobType JobType { get; private set; }

    /// <summary>Required experience level.</summary>
    public ExperienceLevel ExperienceLevel { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public JobPostingStatus Status { get; private set; }

    /// <summary>Salary range (optional).</summary>
    public SalaryRange? Salary { get; private set; }

    /// <summary>Work location.</summary>
    public WorkLocation Location { get; private set; }

    /// <summary>Application deadline (optional - null means no deadline).</summary>
    public DateTime? Deadline { get; private set; }

    /// <summary>When this posting was created.</summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>When this posting was last updated.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>When this posting was published.</summary>
    public DateTime? PublishedAt { get; private set; }

    /// <summary>When this posting was closed.</summary>
    public DateTime? ClosedAt { get; private set; }

    /// <summary>Total number of views.</summary>
    public int ViewCount { get; private set; }

    /// <summary>Total number of applications received.</summary>
    public int ApplicationCount { get; private set; }

    #endregion

    #region Collections

    private readonly List<JobRequirement> _requirements = new();
    /// <summary>Required and preferred skills/qualifications.</summary>
    public IReadOnlyList<JobRequirement> Requirements => _requirements.AsReadOnly();

    private readonly List<string> _tags = new();
    /// <summary>Search tags for categorization (e.g., "dotnet", "ai", "remote").</summary>
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    #endregion

    #region Constructors

    /// <summary>Private constructor for EF Core.</summary>
    private JobPosting()
    {
        Title = string.Empty;
        Description = string.Empty;
        Location = null!;
    }

    private JobPosting(
        JobPostingId id,
        string title,
        string description,
        Guid companyId,
        Guid postedBy,
        JobType jobType,
        ExperienceLevel experienceLevel,
        WorkLocation location,
        SalaryRange? salary,
        DateTime? deadline) : base(id)
    {
        Title = title;
        Description = description;
        CompanyId = companyId;
        PostedBy = postedBy;
        JobType = jobType;
        ExperienceLevel = experienceLevel;
        Location = location;
        Salary = salary;
        Deadline = deadline;
        Status = JobPostingStatus.Draft;
        CreatedAt = DateTime.UtcNow;
        ViewCount = 0;
        ApplicationCount = 0;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Creates a new job posting in Draft status.
    /// </summary>
    public static Result<JobPosting> Create(
        string title,
        string description,
        Guid companyId,
        Guid postedBy,
        JobType jobType,
        ExperienceLevel experienceLevel,
        WorkLocation location,
        SalaryRange? salary = null,
        DateTime? deadline = null)
    {
        // Validate title
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure<JobPosting>(JobPostingErrors.TitleEmpty);

        if (title.Length > MaxTitleLength)
            return Result.Failure<JobPosting>(JobPostingErrors.TitleTooLong);

        // Validate description
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<JobPosting>(JobPostingErrors.DescriptionEmpty);

        if (description.Length > MaxDescriptionLength)
            return Result.Failure<JobPosting>(JobPostingErrors.DescriptionTooLong);

        // Validate IDs
        if (companyId == Guid.Empty)
            return Result.Failure<JobPosting>(JobPostingErrors.CompanyIdEmpty);

        if (postedBy == Guid.Empty)
            return Result.Failure<JobPosting>(JobPostingErrors.PostedByEmpty);

        // Validate deadline
        if (deadline.HasValue && deadline.Value < DateTime.UtcNow)
            return Result.Failure<JobPosting>(JobPostingErrors.DeadlineInPast);

        var jobPosting = new JobPosting(
            JobPostingId.CreateUnique(),
            title.Trim(),
            description.Trim(),
            companyId,
            postedBy,
            jobType,
            experienceLevel,
            location,
            salary,
            deadline);

        jobPosting.AddDomainEvent(new JobPostingCreatedEvent(
            jobPosting.Id.Value,
            companyId,
            jobPosting.Title,
            jobType,
            experienceLevel,
            location.ToString(),
            jobPosting.CreatedAt));

        return Result.Success(jobPosting);
    }

    #endregion

    #region Behavior Methods

    /// <summary>
    /// Updates the job posting details. Only allowed when Draft or Paused.
    /// </summary>
    public Result Update(
        string title,
        string description,
        JobType jobType,
        ExperienceLevel experienceLevel,
        WorkLocation location,
        SalaryRange? salary,
        DateTime? deadline)
    {
        if (Status == JobPostingStatus.Closed || Status == JobPostingStatus.Expired)
            return Result.Failure(JobPostingErrors.InvalidStatus);

        if (Status == JobPostingStatus.Published)
            return Result.Failure(JobPostingErrors.CannotEditPublished);

        // Validate title
        if (string.IsNullOrWhiteSpace(title))
            return Result.Failure(JobPostingErrors.TitleEmpty);

        if (title.Length > MaxTitleLength)
            return Result.Failure(JobPostingErrors.TitleTooLong);

        // Validate description
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(JobPostingErrors.DescriptionEmpty);

        if (description.Length > MaxDescriptionLength)
            return Result.Failure(JobPostingErrors.DescriptionTooLong);

        // Validate deadline
        if (deadline.HasValue && deadline.Value < DateTime.UtcNow)
            return Result.Failure(JobPostingErrors.DeadlineInPast);

        Title = title.Trim();
        Description = description.Trim();
        JobType = jobType;
        ExperienceLevel = experienceLevel;
        Location = location;
        Salary = salary;
        Deadline = deadline;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new JobPostingUpdatedEvent(
            Id.Value,
            Title,
            UpdatedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Publishes the job posting, making it visible and accepting applications.
    /// Only Draft and Paused postings can be published.
    /// </summary>
    public Result Publish()
    {
        if (Status == JobPostingStatus.Published)
            return Result.Failure(JobPostingErrors.AlreadyPublished);

        if (Status == JobPostingStatus.Closed)
            return Result.Failure(JobPostingErrors.AlreadyClosed);

        if (Status == JobPostingStatus.Expired)
            return Result.Failure(JobPostingErrors.AlreadyExpired);

        // Check if deadline has already passed
        if (Deadline.HasValue && Deadline.Value < DateTime.UtcNow)
            return Result.Failure(JobPostingErrors.DeadlineInPast);

        Status = JobPostingStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new JobPostingPublishedEvent(
            Id.Value,
            CompanyId,
            Title,
            PublishedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Pauses a published job posting, temporarily hiding it.
    /// </summary>
    public Result Pause()
    {
        if (Status != JobPostingStatus.Published)
            return Result.Failure(JobPostingErrors.NotPublished);

        Status = JobPostingStatus.Paused;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Closes a job posting permanently with a reason.
    /// </summary>
    public Result Close(string reason)
    {
        if (Status == JobPostingStatus.Closed)
            return Result.Failure(JobPostingErrors.AlreadyClosed);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(JobPostingErrors.CloseReasonRequired);

        Status = JobPostingStatus.Closed;
        ClosedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new JobPostingClosedEvent(
            Id.Value,
            CompanyId,
            Title,
            reason,
            ClosedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Marks the job posting as expired (called when deadline passes).
    /// </summary>
    public Result MarkAsExpired()
    {
        if (Status == JobPostingStatus.Expired)
            return Result.Failure(JobPostingErrors.AlreadyExpired);

        if (Status == JobPostingStatus.Closed)
            return Result.Failure(JobPostingErrors.AlreadyClosed);

        Status = JobPostingStatus.Expired;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new JobPostingExpiredEvent(
            Id.Value,
            CompanyId,
            Title,
            Deadline ?? DateTime.UtcNow,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Adds a skill/qualification requirement to the job posting.
    /// </summary>
    public Result AddRequirement(JobRequirement requirement)
    {
        if (_requirements.Count >= MaxRequirements)
            return Result.Failure(JobPostingErrors.TooManyRequirements);

        if (_requirements.Any(r => r.Skill.Equals(requirement.Skill, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(JobPostingErrors.DuplicateRequirement);

        _requirements.Add(requirement);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Removes a skill/qualification requirement from the job posting.
    /// </summary>
    public Result RemoveRequirement(string skill)
    {
        var requirement = _requirements.FirstOrDefault(
            r => r.Skill.Equals(skill, StringComparison.OrdinalIgnoreCase));

        if (requirement is null)
            return Result.Failure(JobPostingErrors.RequirementNotFound);

        _requirements.Remove(requirement);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Adds a search tag to the job posting.
    /// </summary>
    public Result AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return Result.Failure(JobPostingErrors.TagEmpty);

        if (tag.Length > MaxTagLength)
            return Result.Failure(JobPostingErrors.TagTooLong);

        if (_tags.Count >= MaxTags)
            return Result.Failure(JobPostingErrors.TooManyTags);

        var normalizedTag = tag.Trim().ToLowerInvariant();

        if (_tags.Contains(normalizedTag))
            return Result.Failure(JobPostingErrors.DuplicateTag);

        _tags.Add(normalizedTag);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Removes a search tag from the job posting.
    /// </summary>
    public Result RemoveTag(string tag)
    {
        var normalizedTag = tag.Trim().ToLowerInvariant();

        if (!_tags.Remove(normalizedTag))
            return Result.Failure(new Error("JobPosting.TagNotFound", $"Tag '{tag}' was not found."));

        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Increments the total view count.
    /// </summary>
    public void IncrementViewCount()
    {
        ViewCount++;
    }

    /// <summary>
    /// Increments the application count.
    /// </summary>
    public void IncrementApplicationCount()
    {
        ApplicationCount++;
    }

    /// <summary>
    /// Checks if the job posting is currently accepting applications.
    /// </summary>
    public bool IsAcceptingApplications()
    {
        if (Status != JobPostingStatus.Published)
            return false;

        if (Deadline.HasValue && Deadline.Value < DateTime.UtcNow)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if the deadline has passed and auto-expires if needed.
    /// Returns true if the posting was expired.
    /// </summary>
    public bool CheckAndExpire()
    {
        if (Status != JobPostingStatus.Published)
            return false;

        if (!Deadline.HasValue || Deadline.Value >= DateTime.UtcNow)
            return false;

        MarkAsExpired();
        return true;
    }

    #endregion
}
