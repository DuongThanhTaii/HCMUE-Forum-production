using FluentAssertions;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.Applications.Events;
using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Domain.Tests.Applications;

public class ApplicationTests
{
    #region Test Helpers

    private static readonly Guid ValidApplicantId = Guid.NewGuid();
    private static readonly Guid ValidReviewerId = Guid.NewGuid();
    private static readonly JobPostingId ValidJobPostingId = JobPostingId.CreateUnique();

    private static Resume CreateValidResume()
        => Resume.Create("resume.pdf", "https://storage.example.com/resume.pdf", 1024 * 500, "application/pdf").Value;

    private static CoverLetter CreateValidCoverLetter()
        => CoverLetter.Create(new string('A', CoverLetter.MinContentLength)).Value;

    private static Application CreateValidApplication(CoverLetter? coverLetter = null)
    {
        return Application.Submit(
            ValidJobPostingId,
            ValidApplicantId,
            CreateValidResume(),
            coverLetter).Value;
    }

    #endregion

    #region Submit Tests

    [Fact]
    public void Submit_WithValidData_ShouldReturnSuccess()
    {
        var resume = CreateValidResume();
        var coverLetter = CreateValidCoverLetter();

        var result = Application.Submit(
            ValidJobPostingId,
            ValidApplicantId,
            resume,
            coverLetter);

        result.IsSuccess.Should().BeTrue();
        var application = result.Value;
        application.JobPostingId.Should().Be(ValidJobPostingId);
        application.ApplicantId.Should().Be(ValidApplicantId);
        application.Resume.Should().Be(resume);
        application.CoverLetter.Should().Be(coverLetter);
        application.Status.Should().Be(ApplicationStatus.Pending);
        application.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Submit_WithoutCoverLetter_ShouldSucceed()
    {
        var result = Application.Submit(
            ValidJobPostingId,
            ValidApplicantId,
            CreateValidResume());

        result.IsSuccess.Should().BeTrue();
        result.Value.CoverLetter.Should().BeNull();
    }

    [Fact]
    public void Submit_ShouldRaiseApplicationSubmittedEvent()
    {
        var application = CreateValidApplication(CreateValidCoverLetter());

        application.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ApplicationSubmittedEvent>();

        var evt = (ApplicationSubmittedEvent)application.DomainEvents.First();
        evt.ApplicationId.Should().Be(application.Id.Value);
        evt.JobPostingId.Should().Be(ValidJobPostingId.Value);
        evt.ApplicantId.Should().Be(ValidApplicantId);
        evt.HasCoverLetter.Should().BeTrue();
        evt.HasResume.Should().BeTrue();
    }

    [Fact]
    public void Submit_WithNullJobPostingId_ShouldReturnFailure()
    {
        var result = Application.Submit(
            null!,
            ValidApplicantId,
            CreateValidResume());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.JobPostingIdEmpty);
    }

    [Fact]
    public void Submit_WithEmptyApplicantId_ShouldReturnFailure()
    {
        var result = Application.Submit(
            ValidJobPostingId,
            Guid.Empty,
            CreateValidResume());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.ApplicantIdEmpty);
    }

    [Fact]
    public void Submit_WithNullResume_ShouldReturnFailure()
    {
        var result = Application.Submit(
            ValidJobPostingId,
            ValidApplicantId,
            null!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.ResumeRequired);
    }

    #endregion

    #region MoveToReviewing Tests

    [Fact]
    public void MoveToReviewing_WhenPending_ShouldSucceed()
    {
        var application = CreateValidApplication();

        var result = application.MoveToReviewing(ValidReviewerId);

        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Reviewing);
        application.LastStatusChangedBy.Should().Be(ValidReviewerId);
        application.LastStatusChangedAt.Should().NotBeNull();
    }

    [Fact]
    public void MoveToReviewing_ShouldRaiseStatusChangedEvent()
    {
        var application = CreateValidApplication();
        application.ClearDomainEvents();

        application.MoveToReviewing(ValidReviewerId);

        application.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<ApplicationStatusChangedEvent>();
    }

    [Fact]
    public void MoveToReviewing_WithEmptyReviewerId_ShouldReturnFailure()
    {
        var application = CreateValidApplication();

        var result = application.MoveToReviewing(Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.ReviewerIdEmpty);
    }

    [Fact]
    public void MoveToReviewing_WhenWithdrawn_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Withdraw(ValidApplicantId);

        var result = application.MoveToReviewing(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.AlreadyWithdrawn);
    }

    [Fact]
    public void MoveToReviewing_WhenRejected_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Reject(ValidReviewerId);

        var result = application.MoveToReviewing(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.AlreadyRejected);
    }

    #endregion

    #region Shortlist Tests

    [Fact]
    public void Shortlist_WhenReviewing_ShouldSucceed()
    {
        var application = CreateValidApplication();
        application.MoveToReviewing(ValidReviewerId);

        var result = application.Shortlist(ValidReviewerId, "Good candidate");

        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Shortlisted);
        application.ReviewNotes.Should().Be("Good candidate");
    }

    [Fact]
    public void Shortlist_WithoutNotes_ShouldSucceed()
    {
        var application = CreateValidApplication();

        var result = application.Shortlist(ValidReviewerId);

        result.IsSuccess.Should().BeTrue();
        application.ReviewNotes.Should().BeNull();
    }

    [Fact]
    public void Shortlist_WhenWithdrawn_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Withdraw(ValidApplicantId);

        var result = application.Shortlist(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region MarkAsInterviewed Tests

    [Fact]
    public void MarkAsInterviewed_WhenShortlisted_ShouldSucceed()
    {
        var application = CreateValidApplication();
        application.Shortlist(ValidReviewerId);

        var result = application.MarkAsInterviewed(ValidReviewerId, "Interview completed");

        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Interviewed);
        application.ReviewNotes.Should().Be("Interview completed");
    }

    [Fact]
    public void MarkAsInterviewed_WhenWithdrawn_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Withdraw(ValidApplicantId);

        var result = application.MarkAsInterviewed(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Offer Tests

    [Fact]
    public void Offer_WhenInterviewed_ShouldSucceed()
    {
        var application = CreateValidApplication();
        application.MarkAsInterviewed(ValidReviewerId);

        var result = application.Offer(ValidReviewerId, "Salary: $100k");

        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Offered);
        application.ReviewNotes.Should().Be("Salary: $100k");
    }

    [Fact]
    public void Offer_ShouldRaiseOfferedEvent()
    {
        var application = CreateValidApplication();
        application.ClearDomainEvents();

        application.Offer(ValidReviewerId);

        application.DomainEvents.Should().Contain(e => e is ApplicationOfferedEvent);
    }

    [Fact]
    public void Offer_WhenWithdrawn_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Withdraw(ValidApplicantId);

        var result = application.Offer(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.CannotOfferWithdrawnApplication);
    }

    [Fact]
    public void Offer_WhenRejected_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Reject(ValidReviewerId);

        var result = application.Offer(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.AlreadyRejected);
    }

    #endregion

    #region Accept Tests

    [Fact]
    public void Accept_WhenOffered_ShouldSucceed()
    {
        var application = CreateValidApplication();
        application.Offer(ValidReviewerId);

        var result = application.Accept(ValidApplicantId);

        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Accepted);
    }

    [Fact]
    public void Accept_ShouldRaiseAcceptedEvent()
    {
        var application = CreateValidApplication();
        application.Offer(ValidReviewerId);
        application.ClearDomainEvents();

        application.Accept(ValidApplicantId);

        application.DomainEvents.Should().Contain(e => e is ApplicationAcceptedEvent);
    }

    [Fact]
    public void Accept_WhenNotOffered_ShouldReturnFailure()
    {
        var application = CreateValidApplication();

        var result = application.Accept(ValidApplicantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.MustBeOfferedToAccept);
    }

    [Fact]
    public void Accept_ByNonApplicant_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Offer(ValidReviewerId);

        var result = application.Accept(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotApplicant);
    }

    [Fact]
    public void Accept_WhenAlreadyAccepted_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Offer(ValidReviewerId);
        application.Accept(ValidApplicantId);

        var result = application.Accept(ValidApplicantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.AlreadyAccepted);
    }

    #endregion

    #region Reject Tests

    [Fact]
    public void Reject_WhenPending_ShouldSucceed()
    {
        var application = CreateValidApplication();

        var result = application.Reject(ValidReviewerId, "Not qualified");

        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Rejected);
        application.ReviewNotes.Should().Be("Not qualified");
    }

    [Fact]
    public void Reject_ShouldRaiseRejectedEvent()
    {
        var application = CreateValidApplication();
        application.ClearDomainEvents();

        application.Reject(ValidReviewerId);

        application.DomainEvents.Should().Contain(e => e is ApplicationRejectedEvent);
    }

    [Fact]
    public void Reject_WhenWithdrawn_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Withdraw(ValidApplicantId);

        var result = application.Reject(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.CannotRejectWithdrawnApplication);
    }

    [Fact]
    public void Reject_WhenAlreadyRejected_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Reject(ValidReviewerId);

        var result = application.Reject(ValidReviewerId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.AlreadyRejected);
    }

    #endregion

    #region Withdraw Tests

    [Fact]
    public void Withdraw_WhenPending_ShouldSucceed()
    {
        var application = CreateValidApplication();

        var result = application.Withdraw(ValidApplicantId, "Found another job");

        result.IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Withdrawn);
        application.ReviewNotes.Should().Be("Found another job");
    }

    [Fact]
    public void Withdraw_ShouldRaiseWithdrawnEvent()
    {
        var application = CreateValidApplication();
        application.ClearDomainEvents();

        application.Withdraw(ValidApplicantId);

        application.DomainEvents.Should().Contain(e => e is ApplicationWithdrawnEvent);
    }

    [Fact]
    public void Withdraw_ByNonApplicant_ShouldReturnFailure()
    {
        var application = CreateValidApplication();

        var result = application.Withdraw(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.NotApplicant);
    }

    [Fact]
    public void Withdraw_WhenAlreadyWithdrawn_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Withdraw(ValidApplicantId);

        var result = application.Withdraw(ValidApplicantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.AlreadyWithdrawn);
    }

    [Fact]
    public void Withdraw_WhenRejected_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Reject(ValidReviewerId);

        var result = application.Withdraw(ValidApplicantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.CannotWithdrawAfterRejected);
    }

    [Fact]
    public void Withdraw_WhenAccepted_ShouldReturnFailure()
    {
        var application = CreateValidApplication();
        application.Offer(ValidReviewerId);
        application.Accept(ValidApplicantId);

        var result = application.Withdraw(ValidApplicantId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ApplicationErrors.CannotWithdrawAfterAccepted);
    }

    #endregion

    #region Guard Methods Tests

    [Theory]
    [InlineData(ApplicationStatus.Pending, true)]
    [InlineData(ApplicationStatus.Reviewing, true)]
    [InlineData(ApplicationStatus.Shortlisted, true)]
    [InlineData(ApplicationStatus.Interviewed, true)]
    [InlineData(ApplicationStatus.Offered, true)]
    [InlineData(ApplicationStatus.Withdrawn, false)]
    [InlineData(ApplicationStatus.Rejected, false)]
    [InlineData(ApplicationStatus.Accepted, false)]
    public void IsActive_ShouldReturnCorrectValue(ApplicationStatus status, bool expectedIsActive)
    {
        var application = CreateValidApplication();
        
        // Force status change through valid transitions
        switch (status)
        {
            case ApplicationStatus.Reviewing:
                application.MoveToReviewing(ValidReviewerId);
                break;
            case ApplicationStatus.Shortlisted:
                application.Shortlist(ValidReviewerId);
                break;
            case ApplicationStatus.Interviewed:
                application.MarkAsInterviewed(ValidReviewerId);
                break;
            case ApplicationStatus.Offered:
                application.Offer(ValidReviewerId);
                break;
            case ApplicationStatus.Accepted:
                application.Offer(ValidReviewerId);
                application.Accept(ValidApplicantId);
                break;
            case ApplicationStatus.Rejected:
                application.Reject(ValidReviewerId);
                break;
            case ApplicationStatus.Withdrawn:
                application.Withdraw(ValidApplicantId);
                break;
        }

        application.IsActive().Should().Be(expectedIsActive);
    }

    [Theory]
    [InlineData(ApplicationStatus.Pending, false)]
    [InlineData(ApplicationStatus.Reviewing, false)]
    [InlineData(ApplicationStatus.Shortlisted, false)]
    [InlineData(ApplicationStatus.Interviewed, false)]
    [InlineData(ApplicationStatus.Offered, false)]
    [InlineData(ApplicationStatus.Withdrawn, true)]
    [InlineData(ApplicationStatus.Rejected, true)]
    [InlineData(ApplicationStatus.Accepted, true)]
    public void IsFinal_ShouldReturnCorrectValue(ApplicationStatus status, bool expectedIsFinal)
    {
        var application = CreateValidApplication();
        
        // Force status change through valid transitions
        switch (status)
        {
            case ApplicationStatus.Reviewing:
                application.MoveToReviewing(ValidReviewerId);
                break;
            case ApplicationStatus.Shortlisted:
                application.Shortlist(ValidReviewerId);
                break;
            case ApplicationStatus.Interviewed:
                application.MarkAsInterviewed(ValidReviewerId);
                break;
            case ApplicationStatus.Offered:
                application.Offer(ValidReviewerId);
                break;
            case ApplicationStatus.Accepted:
                application.Offer(ValidReviewerId);
                application.Accept(ValidApplicantId);
                break;
            case ApplicationStatus.Rejected:
                application.Reject(ValidReviewerId);
                break;
            case ApplicationStatus.Withdrawn:
                application.Withdraw(ValidApplicantId);
                break;
        }

        application.IsFinal().Should().Be(expectedIsFinal);
    }

    [Fact]
    public void CanBeReviewed_WhenActive_ShouldReturnTrue()
    {
        var application = CreateValidApplication();
        application.CanBeReviewed().Should().BeTrue();
    }

    [Fact]
    public void CanBeReviewed_WhenFinal_ShouldReturnFalse()
    {
        var application = CreateValidApplication();
        application.Reject(ValidReviewerId);

        application.CanBeReviewed().Should().BeFalse();
    }

    #endregion

    #region Lifecycle Flow Tests

    [Fact]
    public void FullLifecycle_Submit_Review_Shortlist_Interview_Offer_Accept()
    {
        var application = CreateValidApplication();
        application.Status.Should().Be(ApplicationStatus.Pending);

        application.MoveToReviewing(ValidReviewerId).IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Reviewing);

        application.Shortlist(ValidReviewerId).IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Shortlisted);

        application.MarkAsInterviewed(ValidReviewerId).IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Interviewed);

        application.Offer(ValidReviewerId).IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Offered);

        application.Accept(ValidApplicantId).IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Accepted);
    }

    [Fact]
    public void FullLifecycle_Submit_Reject()
    {
        var application = CreateValidApplication();

        application.Reject(ValidReviewerId).IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Rejected);
    }

    [Fact]
    public void FullLifecycle_Submit_Withdraw()
    {
        var application = CreateValidApplication();

        application.Withdraw(ValidApplicantId).IsSuccess.Should().BeTrue();
        application.Status.Should().Be(ApplicationStatus.Withdrawn);
    }

    #endregion

    #region ID Tests

    [Fact]
    public void ApplicationId_CreateUnique_ShouldGenerateUniqueIds()
    {
        var id1 = Domain.Applications.ApplicationId.CreateUnique();
        var id2 = Domain.Applications.ApplicationId.CreateUnique();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void ApplicationId_Create_ShouldWrapGuid()
    {
        var guid = Guid.NewGuid();
        var id = Domain.Applications.ApplicationId.Create(guid);

        id.Value.Should().Be(guid);
    }

    #endregion
}
