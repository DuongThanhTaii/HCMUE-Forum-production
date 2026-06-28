using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Domain errors for JobPosting aggregate.
/// </summary>
public static class JobPostingErrors
{
    public static readonly Error NotFound = new(
        "JobPosting.NotFound",
        "Job posting was not found.");

    public static readonly Error TitleEmpty = new(
        "JobPosting.TitleEmpty",
        "Job posting title is required.");

    public static readonly Error TitleTooLong = new(
        "JobPosting.TitleTooLong",
        $"Job posting title cannot exceed {JobPosting.MaxTitleLength} characters.");

    public static readonly Error DescriptionEmpty = new(
        "JobPosting.DescriptionEmpty",
        "Job posting description is required.");

    public static readonly Error DescriptionTooLong = new(
        "JobPosting.DescriptionTooLong",
        $"Job posting description cannot exceed {JobPosting.MaxDescriptionLength} characters.");

    public static readonly Error CompanyIdEmpty = new(
        "JobPosting.CompanyIdEmpty",
        "Company ID is required.");

    public static readonly Error PostedByEmpty = new(
        "JobPosting.PostedByEmpty",
        "PostedBy (recruiter user ID) is required.");

    public static readonly Error DeadlineInPast = new(
        "JobPosting.DeadlineInPast",
        "Application deadline cannot be in the past.");

    public static readonly Error AlreadyPublished = new(
        "JobPosting.AlreadyPublished",
        "Job posting is already published.");

    public static readonly Error NotPublished = new(
        "JobPosting.NotPublished",
        "Job posting is not currently published.");

    public static readonly Error AlreadyClosed = new(
        "JobPosting.AlreadyClosed",
        "Job posting is already closed.");

    public static readonly Error CannotEditPublished = new(
        "JobPosting.CannotEditPublished",
        "Cannot edit core fields of a published job posting. Pause it first.");

    public static readonly Error AlreadyExpired = new(
        "JobPosting.AlreadyExpired",
        "Job posting has already expired.");

    public static readonly Error InvalidStatus = new(
        "JobPosting.InvalidStatus",
        "Job posting is not in a valid status for this operation.");

    public static readonly Error DuplicateRequirement = new(
        "JobPosting.DuplicateRequirement",
        "This skill requirement already exists on the job posting.");

    public static readonly Error RequirementNotFound = new(
        "JobPosting.RequirementNotFound",
        "The specified skill requirement was not found on the job posting.");

    public static readonly Error TooManyRequirements = new(
        "JobPosting.TooManyRequirements",
        $"A job posting cannot have more than {JobPosting.MaxRequirements} requirements.");

    public static readonly Error TagEmpty = new(
        "JobPosting.TagEmpty",
        "Tag cannot be empty.");

    public static readonly Error TagTooLong = new(
        "JobPosting.TagTooLong",
        $"Tag cannot exceed {JobPosting.MaxTagLength} characters.");

    public static readonly Error TooManyTags = new(
        "JobPosting.TooManyTags",
        $"A job posting cannot have more than {JobPosting.MaxTags} tags.");

    public static readonly Error DuplicateTag = new(
        "JobPosting.DuplicateTag",
        "This tag already exists on the job posting.");

    public static readonly Error CloseReasonRequired = new(
        "JobPosting.CloseReasonRequired",
        "A reason is required when closing a job posting.");
}
