using UniHub.Career.Domain.Applications.Events;
using UniHub.Career.Domain.JobPostings;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Applications;

/// <summary>
/// Represents a job application submitted by a candidate.
/// Aggregate root for the Application bounded context.
/// </summary>
public sealed class Application : AggregateRoot<ApplicationId>
{
    /// <summary>Private parameterless constructor for EF Core.</summary>
    private Application() { }

    private Application(ApplicationId id) : base(id) { }

    /// <summary>The job posting being applied to.</summary>
    public JobPostingId JobPostingId { get; private set; } = null!;

    /// <summary>The user ID of the applicant (candidate).</summary>
    public Guid ApplicantId { get; private set; }

    /// <summary>Current status of the application.</summary>
    public ApplicationStatus Status { get; private set; }

    /// <summary>Resume/CV attached to the application.</summary>
    public Resume Resume { get; private set; } = null!;

    /// <summary>Optional cover letter.</summary>
    public CoverLetter? CoverLetter { get; private set; }

    /// <summary>Date and time when the application was submitted.</summary>
    public DateTime SubmittedAt { get; private set; }

    /// <summary>Date and time of the last status change.</summary>
    public DateTime? LastStatusChangedAt { get; private set; }

    /// <summary>User ID of the last person who changed the status.</summary>
    public Guid? LastStatusChangedBy { get; private set; }

    /// <summary>Additional notes or feedback from reviewers.</summary>
    public string? ReviewNotes { get; private set; }

    /// <summary>
    /// Submits a new job application.
    /// </summary>
    public static Result<Application> Submit(
        JobPostingId jobPostingId,
        Guid applicantId,
        Resume resume,
        CoverLetter? coverLetter = null)
    {
        if (jobPostingId == null || jobPostingId.Value == Guid.Empty)
            return Result.Failure<Application>(ApplicationErrors.JobPostingIdEmpty);

        if (applicantId == Guid.Empty)
            return Result.Failure<Application>(ApplicationErrors.ApplicantIdEmpty);

        if (resume == null)
            return Result.Failure<Application>(ApplicationErrors.ResumeRequired);

        var application = new Application(ApplicationId.CreateUnique())
        {
            JobPostingId = jobPostingId,
            ApplicantId = applicantId,
            Resume = resume,
            CoverLetter = coverLetter,
            Status = ApplicationStatus.Pending,
            SubmittedAt = DateTime.UtcNow
        };

        application.AddDomainEvent(new ApplicationSubmittedEvent(
            application.Id.Value,
            jobPostingId.Value,
            applicantId,
            application.SubmittedAt,
            coverLetter != null,
            true));

        return Result.Success(application);
    }

    /// <summary>
    /// Moves the application to Reviewing status.
    /// </summary>
    public Result MoveToReviewing(Guid reviewerId)
    {
        if (reviewerId == Guid.Empty)
            return Result.Failure(ApplicationErrors.ReviewerIdEmpty);

        if (Status == ApplicationStatus.Withdrawn)
            return Result.Failure(ApplicationErrors.AlreadyWithdrawn);

        if (Status == ApplicationStatus.Rejected)
            return Result.Failure(ApplicationErrors.AlreadyRejected);

        if (Status == ApplicationStatus.Accepted)
            return Result.Failure(ApplicationErrors.AlreadyAccepted);

        var oldStatus = Status;
        Status = ApplicationStatus.Reviewing;
        LastStatusChangedAt = DateTime.UtcNow;
        LastStatusChangedBy = reviewerId;

        AddDomainEvent(new ApplicationStatusChangedEvent(
            Id.Value,
            oldStatus,
            Status,
            reviewerId,
            LastStatusChangedAt.Value,
            null));

        return Result.Success();
    }

    /// <summary>
    /// Shortlists the application for next stage.
    /// </summary>
    public Result Shortlist(Guid reviewerId, string? notes = null)
    {
        if (reviewerId == Guid.Empty)
            return Result.Failure(ApplicationErrors.ReviewerIdEmpty);

        if (Status == ApplicationStatus.Withdrawn)
            return Result.Failure(ApplicationErrors.CannotRejectWithdrawnApplication);

        if (Status == ApplicationStatus.Rejected)
            return Result.Failure(ApplicationErrors.AlreadyRejected);

        if (Status == ApplicationStatus.Accepted)
            return Result.Failure(ApplicationErrors.AlreadyAccepted);

        var oldStatus = Status;
        Status = ApplicationStatus.Shortlisted;
        LastStatusChangedAt = DateTime.UtcNow;
        LastStatusChangedBy = reviewerId;
        ReviewNotes = notes;

        AddDomainEvent(new ApplicationStatusChangedEvent(
            Id.Value,
            oldStatus,
            Status,
            reviewerId,
            LastStatusChangedAt.Value,
            notes));

        return Result.Success();
    }

    /// <summary>
    /// Marks the application as interviewed.
    /// </summary>
    public Result MarkAsInterviewed(Guid reviewerId, string? notes = null)
    {
        if (reviewerId == Guid.Empty)
            return Result.Failure(ApplicationErrors.ReviewerIdEmpty);

        if (Status == ApplicationStatus.Withdrawn)
            return Result.Failure(ApplicationErrors.CannotRejectWithdrawnApplication);

        if (Status == ApplicationStatus.Rejected)
            return Result.Failure(ApplicationErrors.AlreadyRejected);

        if (Status == ApplicationStatus.Accepted)
            return Result.Failure(ApplicationErrors.AlreadyAccepted);

        var oldStatus = Status;
        Status = ApplicationStatus.Interviewed;
        LastStatusChangedAt = DateTime.UtcNow;
        LastStatusChangedBy = reviewerId;
        ReviewNotes = notes;

        AddDomainEvent(new ApplicationStatusChangedEvent(
            Id.Value,
            oldStatus,
            Status,
            reviewerId,
            LastStatusChangedAt.Value,
            notes));

        return Result.Success();
    }

    /// <summary>
    /// Extends a job offer to the candidate.
    /// </summary>
    public Result Offer(Guid offeredBy, string? offerDetails = null)
    {
        if (offeredBy == Guid.Empty)
            return Result.Failure(ApplicationErrors.ReviewerIdEmpty);

        if (Status == ApplicationStatus.Withdrawn)
            return Result.Failure(ApplicationErrors.CannotOfferWithdrawnApplication);

        if (Status == ApplicationStatus.Rejected)
            return Result.Failure(ApplicationErrors.AlreadyRejected);

        if (Status == ApplicationStatus.Accepted)
            return Result.Failure(ApplicationErrors.AlreadyAccepted);

        var oldStatus = Status;
        Status = ApplicationStatus.Offered;
        LastStatusChangedAt = DateTime.UtcNow;
        LastStatusChangedBy = offeredBy;
        ReviewNotes = offerDetails;

        AddDomainEvent(new ApplicationOfferedEvent(
            Id.Value,
            offeredBy,
            LastStatusChangedAt.Value,
            offerDetails));

        AddDomainEvent(new ApplicationStatusChangedEvent(
            Id.Value,
            oldStatus,
            Status,
            offeredBy,
            LastStatusChangedAt.Value,
            "Job offer extended"));

        return Result.Success();
    }

    /// <summary>
    /// Candidate accepts the job offer.
    /// </summary>
    public Result Accept(Guid acceptingUserId)
    {
        if (acceptingUserId != ApplicantId)
            return Result.Failure(ApplicationErrors.NotApplicant);

        if (Status == ApplicationStatus.Accepted)
            return Result.Failure(ApplicationErrors.AlreadyAccepted);

        if (Status != ApplicationStatus.Offered)
            return Result.Failure(ApplicationErrors.MustBeOfferedToAccept);

        var oldStatus = Status;
        Status = ApplicationStatus.Accepted;
        LastStatusChangedAt = DateTime.UtcNow;
        LastStatusChangedBy = acceptingUserId;

        AddDomainEvent(new ApplicationAcceptedEvent(
            Id.Value,
            ApplicantId,
            LastStatusChangedAt.Value));

        AddDomainEvent(new ApplicationStatusChangedEvent(
            Id.Value,
            oldStatus,
            Status,
            acceptingUserId,
            LastStatusChangedAt.Value,
            "Offer accepted by candidate"));

        return Result.Success();
    }

    /// <summary>
    /// Rejects the application.
    /// </summary>
    public Result Reject(Guid rejectedBy, string? reason = null)
    {
        if (rejectedBy == Guid.Empty)
            return Result.Failure(ApplicationErrors.ReviewerIdEmpty);

        if (Status == ApplicationStatus.Withdrawn)
            return Result.Failure(ApplicationErrors.CannotRejectWithdrawnApplication);

        if (Status == ApplicationStatus.Rejected)
            return Result.Failure(ApplicationErrors.AlreadyRejected);

        if (Status == ApplicationStatus.Accepted)
            return Result.Failure(ApplicationErrors.AlreadyAccepted);

        var oldStatus = Status;
        Status = ApplicationStatus.Rejected;
        LastStatusChangedAt = DateTime.UtcNow;
        LastStatusChangedBy = rejectedBy;
        ReviewNotes = reason;

        AddDomainEvent(new ApplicationRejectedEvent(
            Id.Value,
            rejectedBy,
            LastStatusChangedAt.Value,
            reason));

        AddDomainEvent(new ApplicationStatusChangedEvent(
            Id.Value,
            oldStatus,
            Status,
            rejectedBy,
            LastStatusChangedAt.Value,
            reason));

        return Result.Success();
    }

    /// <summary>
    /// Candidate withdraws their application.
    /// </summary>
    public Result Withdraw(Guid withdrawingUserId, string? reason = null)
    {
        if (withdrawingUserId != ApplicantId)
            return Result.Failure(ApplicationErrors.NotApplicant);

        if (Status == ApplicationStatus.Withdrawn)
            return Result.Failure(ApplicationErrors.AlreadyWithdrawn);

        if (Status == ApplicationStatus.Rejected)
            return Result.Failure(ApplicationErrors.CannotWithdrawAfterRejected);

        if (Status == ApplicationStatus.Accepted)
            return Result.Failure(ApplicationErrors.CannotWithdrawAfterAccepted);

        var oldStatus = Status;
        Status = ApplicationStatus.Withdrawn;
        LastStatusChangedAt = DateTime.UtcNow;
        LastStatusChangedBy = withdrawingUserId;
        ReviewNotes = reason;

        AddDomainEvent(new ApplicationWithdrawnEvent(
            Id.Value,
            ApplicantId,
            LastStatusChangedAt.Value,
            reason));

        AddDomainEvent(new ApplicationStatusChangedEvent(
            Id.Value,
            oldStatus,
            Status,
            withdrawingUserId,
            LastStatusChangedAt.Value,
            reason));

        return Result.Success();
    }

    /// <summary>
    /// Checks if the application is still active (not withdrawn, rejected, or accepted).
    /// </summary>
    public bool IsActive() =>
        Status != ApplicationStatus.Withdrawn &&
        Status != ApplicationStatus.Rejected &&
        Status != ApplicationStatus.Accepted;

    /// <summary>
    /// Checks if the application is in a final state (accepted, rejected, or withdrawn).
    /// </summary>
    public bool IsFinal() =>
        Status == ApplicationStatus.Accepted ||
        Status == ApplicationStatus.Rejected ||
        Status == ApplicationStatus.Withdrawn;

    /// <summary>
    /// Checks if the application can be reviewed (not in final state).
    /// </summary>
    public bool CanBeReviewed() => !IsFinal();
}
