using FluentAssertions;
using UniHub.Career.Domain.JobPostings;
using UniHub.Career.Domain.JobPostings.Events;

namespace UniHub.Career.Domain.Tests.JobPostings;

public class JobPostingTests
{
    #region Test Helpers

    private static readonly Guid ValidCompanyId = Guid.NewGuid();
    private static readonly Guid ValidPosterId = Guid.NewGuid();

    private static WorkLocation CreateValidLocation()
        => WorkLocation.Create("Hồ Chí Minh", "Quận 1").Value;

    private static SalaryRange CreateValidSalary()
        => SalaryRange.Create(10_000_000, 20_000_000, "VND", "month").Value;

    private static JobPosting CreateValidJobPosting(
        string title = "Senior .NET Developer",
        string description = "We are looking for a skilled .NET developer to join our team.",
        JobType jobType = JobType.FullTime,
        ExperienceLevel experienceLevel = ExperienceLevel.Senior,
        DateTime? deadline = null)
    {
        return JobPosting.Create(
            title,
            description,
            ValidCompanyId,
            ValidPosterId,
            jobType,
            experienceLevel,
            CreateValidLocation(),
            CreateValidSalary(),
            deadline ?? DateTime.UtcNow.AddDays(30)).Value;
    }

    private static JobPosting CreatePublishedJobPosting()
    {
        var job = CreateValidJobPosting();
        job.Publish();
        return job;
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        var location = CreateValidLocation();
        var salary = CreateValidSalary();
        var deadline = DateTime.UtcNow.AddDays(30);

        var result = JobPosting.Create(
            "Senior .NET Developer",
            "We are looking for a skilled .NET developer.",
            ValidCompanyId,
            ValidPosterId,
            JobType.FullTime,
            ExperienceLevel.Senior,
            location,
            salary,
            deadline);

        result.IsSuccess.Should().BeTrue();
        var job = result.Value;
        job.Title.Should().Be("Senior .NET Developer");
        job.Description.Should().Be("We are looking for a skilled .NET developer.");
        job.CompanyId.Should().Be(ValidCompanyId);
        job.PostedBy.Should().Be(ValidPosterId);
        job.JobType.Should().Be(JobType.FullTime);
        job.ExperienceLevel.Should().Be(ExperienceLevel.Senior);
        job.Location.Should().Be(location);
        job.Salary.Should().Be(salary);
        job.Status.Should().Be(JobPostingStatus.Draft);
        job.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        job.ViewCount.Should().Be(0);
        job.ApplicationCount.Should().Be(0);
        job.Requirements.Should().BeEmpty();
        job.Tags.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithNullSalary_ShouldSucceed()
    {
        var result = JobPosting.Create(
            "Intern Developer",
            "Internship position.",
            ValidCompanyId,
            ValidPosterId,
            JobType.Internship,
            ExperienceLevel.Entry,
            CreateValidLocation());

        result.IsSuccess.Should().BeTrue();
        result.Value.Salary.Should().BeNull();
    }

    [Fact]
    public void Create_WithNullDeadline_ShouldSucceed()
    {
        var result = JobPosting.Create(
            "Developer",
            "Description here.",
            ValidCompanyId,
            ValidPosterId,
            JobType.FullTime,
            ExperienceLevel.Mid,
            CreateValidLocation(),
            CreateValidSalary());

        result.IsSuccess.Should().BeTrue();
        result.Value.Deadline.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseDomainEvent()
    {
        var job = CreateValidJobPosting();

        job.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<JobPostingCreatedEvent>();

        var evt = (JobPostingCreatedEvent)job.DomainEvents.First();
        evt.JobPostingId.Should().Be(job.Id.Value);
        evt.CompanyId.Should().Be(ValidCompanyId);
        evt.Title.Should().Be("Senior .NET Developer");
        evt.JobType.Should().Be(JobType.FullTime);
        evt.ExperienceLevel.Should().Be(ExperienceLevel.Senior);
    }

    [Fact]
    public void Create_ShouldTrimTitle()
    {
        var job = JobPosting.Create(
            "  Senior .NET Developer  ", "Description.",
            ValidCompanyId, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation()).Value;

        job.Title.Should().Be("Senior .NET Developer");
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyTitle_ShouldReturnFailure(string? title)
    {
        var result = JobPosting.Create(
            title!, "Description.",
            ValidCompanyId, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.TitleEmpty);
    }

    [Fact]
    public void Create_WithTitleTooLong_ShouldReturnFailure()
    {
        var longTitle = new string('A', JobPosting.MaxTitleLength + 1);

        var result = JobPosting.Create(
            longTitle, "Description.",
            ValidCompanyId, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.TitleTooLong);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyDescription_ShouldReturnFailure(string? description)
    {
        var result = JobPosting.Create(
            "Title", description!,
            ValidCompanyId, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.DescriptionEmpty);
    }

    [Fact]
    public void Create_WithDescriptionTooLong_ShouldReturnFailure()
    {
        var longDesc = new string('A', JobPosting.MaxDescriptionLength + 1);

        var result = JobPosting.Create(
            "Title", longDesc,
            ValidCompanyId, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.DescriptionTooLong);
    }

    [Fact]
    public void Create_WithEmptyCompanyId_ShouldReturnFailure()
    {
        var result = JobPosting.Create(
            "Title", "Description.",
            Guid.Empty, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.CompanyIdEmpty);
    }

    [Fact]
    public void Create_WithEmptyPosterId_ShouldReturnFailure()
    {
        var result = JobPosting.Create(
            "Title", "Description.",
            ValidCompanyId, Guid.Empty,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.PostedByEmpty);
    }

    [Fact]
    public void Create_WithDeadlineInPast_ShouldReturnFailure()
    {
        var result = JobPosting.Create(
            "Title", "Description.",
            ValidCompanyId, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation(),
            deadline: DateTime.UtcNow.AddDays(-1));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.DeadlineInPast);
    }

    #endregion

    #region Publish Tests

    [Fact]
    public void Publish_FromDraft_ShouldSucceed()
    {
        var job = CreateValidJobPosting();

        var result = job.Publish();

        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Published);
        job.PublishedAt.Should().NotBeNull();
        job.DomainEvents.Should().Contain(e => e is JobPostingPublishedEvent);
    }

    [Fact]
    public void Publish_FromPaused_ShouldSucceed()
    {
        var job = CreatePublishedJobPosting();
        job.Pause();

        var result = job.Publish();

        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Published);
    }

    [Fact]
    public void Publish_WhenAlreadyPublished_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();

        var result = job.Publish();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.AlreadyPublished);
    }

    [Fact]
    public void Publish_WhenClosed_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();
        job.Close("Filled");

        var result = job.Publish();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.AlreadyClosed);
    }

    [Fact]
    public void Publish_WhenExpired_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();
        job.MarkAsExpired();

        var result = job.Publish();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.AlreadyExpired);
    }

    #endregion

    #region Pause Tests

    [Fact]
    public void Pause_WhenPublished_ShouldSucceed()
    {
        var job = CreatePublishedJobPosting();

        var result = job.Pause();

        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Paused);
    }

    [Fact]
    public void Pause_WhenNotPublished_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting(); // Draft

        var result = job.Pause();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.NotPublished);
    }

    #endregion

    #region Close Tests

    [Fact]
    public void Close_WhenPublished_ShouldSucceed()
    {
        var job = CreatePublishedJobPosting();

        var result = job.Close("Position filled");

        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Closed);
        job.ClosedAt.Should().NotBeNull();
        job.DomainEvents.Should().Contain(e => e is JobPostingClosedEvent);
    }

    [Fact]
    public void Close_WhenDraft_ShouldSucceed()
    {
        var job = CreateValidJobPosting();

        var result = job.Close("Changed plans");

        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Closed);
    }

    [Fact]
    public void Close_WhenAlreadyClosed_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();
        job.Close("Filled");

        var result = job.Close("Again");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.AlreadyClosed);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Close_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        var job = CreatePublishedJobPosting();

        var result = job.Close(reason!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.CloseReasonRequired);
    }

    #endregion

    #region MarkAsExpired Tests

    [Fact]
    public void MarkAsExpired_WhenPublished_ShouldSucceed()
    {
        var job = CreatePublishedJobPosting();

        var result = job.MarkAsExpired();

        result.IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Expired);
        job.DomainEvents.Should().Contain(e => e is JobPostingExpiredEvent);
    }

    [Fact]
    public void MarkAsExpired_WhenAlreadyExpired_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();
        job.MarkAsExpired();

        var result = job.MarkAsExpired();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.AlreadyExpired);
    }

    [Fact]
    public void MarkAsExpired_WhenClosed_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();
        job.Close("Done");

        var result = job.MarkAsExpired();

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.AlreadyClosed);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WhenDraft_ShouldSucceed()
    {
        var job = CreateValidJobPosting();
        var newLocation = WorkLocation.Create("Hà Nội").Value;
        var newSalary = SalaryRange.Create(15_000_000, 30_000_000, "VND", "month").Value;

        var result = job.Update(
            "Updated Title",
            "Updated description.",
            JobType.Remote,
            ExperienceLevel.Mid,
            newLocation,
            newSalary,
            DateTime.UtcNow.AddDays(60));

        result.IsSuccess.Should().BeTrue();
        job.Title.Should().Be("Updated Title");
        job.Description.Should().Be("Updated description.");
        job.JobType.Should().Be(JobType.Remote);
        job.ExperienceLevel.Should().Be(ExperienceLevel.Mid);
        job.Location.Should().Be(newLocation);
        job.Salary.Should().Be(newSalary);
        job.UpdatedAt.Should().NotBeNull();
        job.DomainEvents.Should().Contain(e => e is JobPostingUpdatedEvent);
    }

    [Fact]
    public void Update_WhenPaused_ShouldSucceed()
    {
        var job = CreatePublishedJobPosting();
        job.Pause();

        var result = job.Update(
            "New Title", "New desc.",
            JobType.PartTime, ExperienceLevel.Junior,
            CreateValidLocation(), null,
            DateTime.UtcNow.AddDays(30));

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Update_WhenPublished_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();

        var result = job.Update(
            "New Title", "New desc.",
            JobType.PartTime, ExperienceLevel.Junior,
            CreateValidLocation(), null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.CannotEditPublished);
    }

    [Fact]
    public void Update_WhenClosed_ShouldReturnFailure()
    {
        var job = CreatePublishedJobPosting();
        job.Close("Done");

        var result = job.Update(
            "New Title", "New desc.",
            JobType.PartTime, ExperienceLevel.Junior,
            CreateValidLocation(), null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.InvalidStatus);
    }

    [Fact]
    public void Update_WithEmptyTitle_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();

        var result = job.Update("", "Desc.", JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation(), null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.TitleEmpty);
    }

    [Fact]
    public void Update_WithEmptyDescription_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();

        var result = job.Update("Title", "", JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation(), null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.DescriptionEmpty);
    }

    #endregion

    #region Requirement Tests

    [Fact]
    public void AddRequirement_ShouldSucceed()
    {
        var job = CreateValidJobPosting();
        var req = JobRequirement.Create("C#", true).Value;

        var result = job.AddRequirement(req);

        result.IsSuccess.Should().BeTrue();
        job.Requirements.Should().ContainSingle().Which.Skill.Should().Be("C#");
    }

    [Fact]
    public void AddRequirement_Multiple_ShouldSucceed()
    {
        var job = CreateValidJobPosting();
        job.AddRequirement(JobRequirement.Create("C#").Value);
        job.AddRequirement(JobRequirement.Create("SQL").Value);
        job.AddRequirement(JobRequirement.Create("Docker", false).Value);

        job.Requirements.Should().HaveCount(3);
    }

    [Fact]
    public void AddRequirement_Duplicate_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();
        job.AddRequirement(JobRequirement.Create("C#").Value);

        var result = job.AddRequirement(JobRequirement.Create("c#").Value); // case insensitive

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.DuplicateRequirement);
    }

    [Fact]
    public void AddRequirement_ExceedsMax_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();
        for (int i = 0; i < JobPosting.MaxRequirements; i++)
            job.AddRequirement(JobRequirement.Create($"Skill-{i}").Value);

        var result = job.AddRequirement(JobRequirement.Create("One more").Value);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.TooManyRequirements);
    }

    [Fact]
    public void RemoveRequirement_ShouldSucceed()
    {
        var job = CreateValidJobPosting();
        job.AddRequirement(JobRequirement.Create("C#").Value);
        job.AddRequirement(JobRequirement.Create("SQL").Value);

        var result = job.RemoveRequirement("C#");

        result.IsSuccess.Should().BeTrue();
        job.Requirements.Should().ContainSingle().Which.Skill.Should().Be("SQL");
    }

    [Fact]
    public void RemoveRequirement_CaseInsensitive_ShouldSucceed()
    {
        var job = CreateValidJobPosting();
        job.AddRequirement(JobRequirement.Create("C#").Value);

        var result = job.RemoveRequirement("c#");

        result.IsSuccess.Should().BeTrue();
        job.Requirements.Should().BeEmpty();
    }

    [Fact]
    public void RemoveRequirement_NotFound_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();

        var result = job.RemoveRequirement("Java");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.RequirementNotFound);
    }

    #endregion

    #region Tag Tests

    [Fact]
    public void AddTag_ShouldSucceed()
    {
        var job = CreateValidJobPosting();

        var result = job.AddTag("dotnet");

        result.IsSuccess.Should().BeTrue();
        job.Tags.Should().ContainSingle().Which.Should().Be("dotnet");
    }

    [Fact]
    public void AddTag_ShouldNormalizeToLowerCase()
    {
        var job = CreateValidJobPosting();

        job.AddTag("DotNet");

        job.Tags.Should().Contain("dotnet");
    }

    [Fact]
    public void AddTag_Duplicate_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();
        job.AddTag("dotnet");

        var result = job.AddTag("DotNet"); // same after normalization

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.DuplicateTag);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AddTag_Empty_ShouldReturnFailure(string? tag)
    {
        var job = CreateValidJobPosting();

        var result = job.AddTag(tag!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.TagEmpty);
    }

    [Fact]
    public void AddTag_TooLong_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();
        var longTag = new string('a', JobPosting.MaxTagLength + 1);

        var result = job.AddTag(longTag);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.TagTooLong);
    }

    [Fact]
    public void AddTag_ExceedsMax_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();
        for (int i = 0; i < JobPosting.MaxTags; i++)
            job.AddTag($"tag-{i}");

        var result = job.AddTag("one-more");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(JobPostingErrors.TooManyTags);
    }

    [Fact]
    public void RemoveTag_ShouldSucceed()
    {
        var job = CreateValidJobPosting();
        job.AddTag("dotnet");
        job.AddTag("csharp");

        var result = job.RemoveTag("dotnet");

        result.IsSuccess.Should().BeTrue();
        job.Tags.Should().ContainSingle().Which.Should().Be("csharp");
    }

    [Fact]
    public void RemoveTag_NotFound_ShouldReturnFailure()
    {
        var job = CreateValidJobPosting();

        var result = job.RemoveTag("nonexistent");

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Counter Tests

    [Fact]
    public void IncrementViewCount_ShouldIncrement()
    {
        var job = CreateValidJobPosting();

        job.IncrementViewCount();
        job.IncrementViewCount();
        job.IncrementViewCount();

        job.ViewCount.Should().Be(3);
    }

    [Fact]
    public void IncrementApplicationCount_ShouldIncrement()
    {
        var job = CreateValidJobPosting();

        job.IncrementApplicationCount();
        job.IncrementApplicationCount();

        job.ApplicationCount.Should().Be(2);
    }

    #endregion

    #region IsAcceptingApplications Tests

    [Fact]
    public void IsAcceptingApplications_WhenPublished_ShouldReturnTrue()
    {
        var job = CreatePublishedJobPosting();

        job.IsAcceptingApplications().Should().BeTrue();
    }

    [Fact]
    public void IsAcceptingApplications_WhenDraft_ShouldReturnFalse()
    {
        var job = CreateValidJobPosting();

        job.IsAcceptingApplications().Should().BeFalse();
    }

    [Fact]
    public void IsAcceptingApplications_WhenClosed_ShouldReturnFalse()
    {
        var job = CreatePublishedJobPosting();
        job.Close("Filled");

        job.IsAcceptingApplications().Should().BeFalse();
    }

    #endregion

    #region Lifecycle Flow Tests

    [Fact]
    public void FullLifecycle_Draft_Publish_Pause_Publish_Close()
    {
        var job = CreateValidJobPosting();
        job.Status.Should().Be(JobPostingStatus.Draft);

        job.Publish().IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Published);

        job.Pause().IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Paused);

        job.Publish().IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Published);

        job.Close("Position filled").IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Closed);
        job.ClosedAt.Should().NotBeNull();
    }

    [Fact]
    public void FullLifecycle_Draft_Publish_Expire()
    {
        var job = CreateValidJobPosting();

        job.Publish().IsSuccess.Should().BeTrue();
        job.MarkAsExpired().IsSuccess.Should().BeTrue();
        job.Status.Should().Be(JobPostingStatus.Expired);
    }

    [Fact]
    public void CheckAndExpire_WhenNotPublished_ShouldReturnFalse()
    {
        var job = CreateValidJobPosting();

        job.CheckAndExpire().Should().BeFalse();
    }

    [Fact]
    public void CheckAndExpire_WhenNoDeadline_ShouldReturnFalse()
    {
        var job = JobPosting.Create(
            "Title", "Description.",
            ValidCompanyId, ValidPosterId,
            JobType.FullTime, ExperienceLevel.Mid,
            CreateValidLocation()).Value;
        job.Publish();

        job.CheckAndExpire().Should().BeFalse();
    }

    #endregion

    #region ID Tests

    [Fact]
    public void JobPostingId_CreateUnique_ShouldGenerateUniqueIds()
    {
        var id1 = JobPostingId.CreateUnique();
        var id2 = JobPostingId.CreateUnique();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void JobPostingId_Create_ShouldWrappGuid()
    {
        var guid = Guid.NewGuid();
        var id = JobPostingId.Create(guid);

        id.Value.Should().Be(guid);
    }

    #endregion
}
