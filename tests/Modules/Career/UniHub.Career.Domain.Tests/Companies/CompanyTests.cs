using FluentAssertions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Companies.Events;

namespace UniHub.Career.Domain.Tests.Companies;

public class CompanyTests
{
    #region Test Helpers

    private static readonly Guid ValidRegisteredBy = Guid.NewGuid();

    private static ContactInfo CreateValidContactInfo()
        => ContactInfo.Create("contact@company.com", "+84123456789", "123 Main St").Value;

    private static SocialLinks CreateValidSocialLinks()
        => SocialLinks.Create("https://linkedin.com/company/test").Value;

    private static Company CreateValidCompany(
        string name = "Tech Solutions Inc",
        string description = "A leading technology company providing innovative solutions.",
        Industry industry = Industry.Technology,
        CompanySize size = CompanySize.Medium,
        int? foundedYear = 2015)
    {
        return Company.Register(
            name,
            description,
            industry,
            size,
            CreateValidContactInfo(),
            ValidRegisteredBy,
            "https://techsolutions.com",
            "https://cdn.example.com/logo.png",
            foundedYear,
            CreateValidSocialLinks()).Value;
    }

    private static Company CreateVerifiedCompany()
    {
        var company = CreateValidCompany();
        company.Verify(Guid.NewGuid());
        return company;
    }

    #endregion

    #region Register Tests

    [Fact]
    public void Register_WithValidData_ShouldReturnSuccess()
    {
        var contactInfo = CreateValidContactInfo();
        var socialLinks = CreateValidSocialLinks();

        var result = Company.Register(
            "Tech Solutions Inc",
            "A leading technology company.",
            Industry.Technology,
            CompanySize.Medium,
            contactInfo,
            ValidRegisteredBy,
            "https://techsolutions.com",
            "https://cdn.example.com/logo.png",
            2015,
            socialLinks);

        result.IsSuccess.Should().BeTrue();
        var company = result.Value;
        company.Name.Should().Be("Tech Solutions Inc");
        company.Description.Should().Be("A leading technology company.");
        company.Industry.Should().Be(Industry.Technology);
        company.Size.Should().Be(CompanySize.Medium);
        company.ContactInfo.Should().Be(contactInfo);
        company.RegisteredBy.Should().Be(ValidRegisteredBy);
        company.Website.Should().Be("https://techsolutions.com");
        company.LogoUrl.Should().Be("https://cdn.example.com/logo.png");
        company.FoundedYear.Should().Be(2015);
        company.SocialLinks.Should().Be(socialLinks);
        company.Status.Should().Be(CompanyStatus.Pending);
        company.RegisteredAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        company.TotalJobPostings.Should().Be(0);
        company.Benefits.Should().BeEmpty();
    }

    [Fact]
    public void Register_WithMinimalData_ShouldSucceed()
    {
        var result = Company.Register(
            "Startup Co",
            "New startup",
            Industry.Technology,
            CompanySize.Startup,
            CreateValidContactInfo(),
            ValidRegisteredBy);

        result.IsSuccess.Should().BeTrue();
        result.Value.Website.Should().BeNull();
        result.Value.LogoUrl.Should().BeNull();
        result.Value.FoundedYear.Should().BeNull();
    }

    [Fact]
    public void Register_ShouldRaiseDomainEvent()
    {
        var company = CreateValidCompany();

        company.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<CompanyRegisteredEvent>();

        var evt = (CompanyRegisteredEvent)company.DomainEvents.First();
        evt.CompanyId.Should().Be(company.Id.Value);
        evt.Name.Should().Be("Tech Solutions Inc");
        evt.Industry.Should().Be(Industry.Technology);
        evt.Size.Should().Be(CompanySize.Medium);
        evt.RegisteredBy.Should().Be(ValidRegisteredBy);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Register_WithEmptyName_ShouldReturnFailure(string? name)
    {
        var result = Company.Register(
            name!,
            "Description",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.NameEmpty);
    }

    [Fact]
    public void Register_WithNameTooLong_ShouldReturnFailure()
    {
        var longName = new string('A', Company.MaxNameLength + 1);

        var result = Company.Register(
            longName,
            "Description",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.NameTooLong);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Register_WithEmptyDescription_ShouldReturnFailure(string? description)
    {
        var result = Company.Register(
            "Company Name",
            description!,
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.DescriptionEmpty);
    }

    [Fact]
    public void Register_WithDescriptionTooLong_ShouldReturnFailure()
    {
        var longDesc = new string('A', Company.MaxDescriptionLength + 1);

        var result = Company.Register(
            "Company",
            longDesc,
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.DescriptionTooLong);
    }

    [Fact]
    public void Register_WithEmptyRegisteredBy_ShouldReturnFailure()
    {
        var result = Company.Register(
            "Company",
            "Description",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            Guid.Empty);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.RegisteredByEmpty);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://test.com")]
    public void Register_WithInvalidWebsite_ShouldReturnFailure(string invalidWebsite)
    {
        var result = Company.Register(
            "Company",
            "Description",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy,
            invalidWebsite);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.InvalidWebsiteUrl);
    }

    [Fact]
    public void Register_WithWebsiteTooLong_ShouldReturnFailure()
    {
        var longUrl = "https://" + new string('a', Company.MaxWebsiteLength);

        var result = Company.Register(
            "Company",
            "Description",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy,
            longUrl);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.WebsiteTooLong);
    }

    [Theory]
    [InlineData(1799)]
    [InlineData(2100)]
    public void Register_WithInvalidFoundedYear_ShouldReturnFailure(int invalidYear)
    {
        var result = Company.Register(
            "Company",
            "Description",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy,
            foundedYear: invalidYear);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.InvalidFoundedYear);
    }

    [Fact]
    public void Register_TrimsNameAndDescription()
    {
        var company = Company.Register(
            "  Company Name  ",
            "  Description  ",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            ValidRegisteredBy).Value;

        company.Name.Should().Be("Company Name");
        company.Description.Should().Be("Description");
    }

    #endregion

    #region Verify Tests

    [Fact]
    public void Verify_WhenPending_ShouldSucceed()
    {
        var company = CreateValidCompany();
        var verifierId = Guid.NewGuid();

        var result = company.Verify(verifierId);

        result.IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Verified);
        company.VerifiedAt.Should().NotBeNull();
        company.VerifiedBy.Should().Be(verifierId);
        company.DomainEvents.Should().Contain(e => e is CompanyVerifiedEvent);
    }

    [Fact]
    public void Verify_WhenAlreadyVerified_ShouldReturnFailure()
    {
        var company = CreateVerifiedCompany();

        var result = company.Verify(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.AlreadyVerified);
    }

    [Fact]
    public void Verify_WithEmptyVerifierId_ShouldReturnFailure()
    {
        var company = CreateValidCompany();

        var result = company.Verify(Guid.Empty);

        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Suspend Tests

    [Fact]
    public void Suspend_WhenVerified_ShouldSucceed()
    {
        var company = CreateVerifiedCompany();
        var suspenderId = Guid.NewGuid();

        var result = company.Suspend("Policy violation", suspenderId);

        result.IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Suspended);
        company.DomainEvents.Should().Contain(e => e is CompanySuspendedEvent);
    }

    [Fact]
    public void Suspend_WhenAlreadySuspended_ShouldReturnFailure()
    {
        var company = CreateVerifiedCompany();
        company.Suspend("Reason", Guid.NewGuid());

        var result = company.Suspend("Another reason", Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.AlreadySuspended);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Suspend_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        var company = CreateVerifiedCompany();

        var result = company.Suspend(reason!, Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.SuspensionReasonRequired);
    }

    #endregion

    #region Reactivate Tests

    [Fact]
    public void Reactivate_WhenSuspended_ShouldSucceed()
    {
        var company = CreateVerifiedCompany();
        company.Suspend("Reason", Guid.NewGuid());

        var result = company.Reactivate(Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Verified);
        company.DomainEvents.Should().Contain(e => e is CompanyReactivatedEvent);
    }

    [Fact]
    public void Reactivate_WhenNotSuspended_ShouldReturnFailure()
    {
        var company = CreateVerifiedCompany();

        var result = company.Reactivate(Guid.NewGuid());

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.NotSuspended);
    }

    [Fact]
    public void Reactivate_SuspendedPendingCompany_ShouldReturnToPending()
    {
        var company = CreateValidCompany(); // Pending
        company.Suspend("Reason", Guid.NewGuid());

        company.Reactivate(Guid.NewGuid());

        company.Status.Should().Be(CompanyStatus.Pending);
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WhenActive_ShouldSucceed()
    {
        var company = CreateVerifiedCompany();

        var result = company.Deactivate("Closing business");

        result.IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Inactive);
        company.DomainEvents.Should().Contain(e => e is CompanyDeactivatedEvent);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFailure()
    {
        var company = CreateVerifiedCompany();
        company.Deactivate("Reason");

        var result = company.Deactivate("Another reason");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.AlreadyInactive);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Deactivate_WithEmptyReason_ShouldReturnFailure(string? reason)
    {
        var company = CreateVerifiedCompany();

        var result = company.Deactivate(reason!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.DeactivationReasonRequired);
    }

    #endregion

    #region UpdateProfile Tests

    [Fact]
    public void UpdateProfile_WhenValid_ShouldSucceed()
    {
        var company = CreateVerifiedCompany();
        var newContact = ContactInfo.Create("new@company.com").Value;

        var result = company.UpdateProfile(
            "Updated Company Name",
            "Updated description",
            Industry.Finance,
            CompanySize.Large,
            newContact,
            "https://newwebsite.com",
            "https://newlogo.png",
            2020,
            SocialLinks.Empty());

        result.IsSuccess.Should().BeTrue();
        company.Name.Should().Be("Updated Company Name");
        company.Description.Should().Be("Updated description");
        company.Industry.Should().Be(Industry.Finance);
        company.Size.Should().Be(CompanySize.Large);
        company.ContactInfo.Should().Be(newContact);
        company.Website.Should().Be("https://newwebsite.com");
        company.UpdatedAt.Should().NotBeNull();
        company.DomainEvents.Should().Contain(e => e is CompanyProfileUpdatedEvent);
    }

    [Fact]
    public void UpdateProfile_WhenSuspended_ShouldReturnFailure()
    {
        var company = CreateVerifiedCompany();
        company.Suspend("Reason", Guid.NewGuid());

        var result = company.UpdateProfile(
            "New Name",
            "New desc",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            null, null, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.CannotUpdateWhileSuspended);
    }

    [Fact]
    public void UpdateProfile_WithInvalidData_ShouldReturnFailure()
    {
        var company = CreateVerifiedCompany();

        var result = company.UpdateProfile(
            "",
            "Description",
            Industry.Technology,
            CompanySize.Small,
            CreateValidContactInfo(),
            null, null, null, null);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.NameEmpty);
    }

    #endregion

    #region Benefits Tests

    [Fact]
    public void AddBenefit_ShouldSucceed()
    {
        var company = CreateValidCompany();

        var result = company.AddBenefit("Health Insurance");

        result.IsSuccess.Should().BeTrue();
        company.Benefits.Should().ContainSingle().Which.Should().Be("Health Insurance");
    }

    [Fact]
    public void AddBenefit_Multiple_ShouldSucceed()
    {
        var company = CreateValidCompany();
        company.AddBenefit("Health Insurance");
        company.AddBenefit("Free Lunch");
        company.AddBenefit("Remote Work");

        company.Benefits.Should().HaveCount(3);
    }

    [Fact]
    public void AddBenefit_Duplicate_ShouldReturnFailure()
    {
        var company = CreateValidCompany();
        company.AddBenefit("Health Insurance");

        var result = company.AddBenefit("health insurance"); // case insensitive

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.DuplicateBenefit);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void AddBenefit_WithEmpty_ShouldReturnFailure(string? benefit)
    {
        var company = CreateValidCompany();

        var result = company.AddBenefit(benefit!);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.BenefitEmpty);
    }

    [Fact]
    public void AddBenefit_TooLong_ShouldReturnFailure()
    {
        var company = CreateValidCompany();
        var longBenefit = new string('A', Company.MaxBenefitLength + 1);

        var result = company.AddBenefit(longBenefit);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.BenefitTooLong);
    }

    [Fact]
    public void AddBenefit_ExceedsMax_ShouldReturnFailure()
    {
        var company = CreateValidCompany();
        for (int i = 0; i < Company.MaxBenefits; i++)
            company.AddBenefit($"Benefit {i}");

        var result = company.AddBenefit("One more");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.TooManyBenefits);
    }

    [Fact]
    public void RemoveBenefit_ShouldSucceed()
    {
        var company = CreateValidCompany();
        company.AddBenefit("Health Insurance");
        company.AddBenefit("Free Lunch");

        var result = company.RemoveBenefit("Health Insurance");

        result.IsSuccess.Should().BeTrue();
        company.Benefits.Should().ContainSingle().Which.Should().Be("Free Lunch");
    }

    [Fact]
    public void RemoveBenefit_CaseInsensitive_ShouldSucceed()
    {
        var company = CreateValidCompany();
        company.AddBenefit("Health Insurance");

        var result = company.RemoveBenefit("health insurance");

        result.IsSuccess.Should().BeTrue();
        company.Benefits.Should().BeEmpty();
    }

    [Fact]
    public void RemoveBenefit_NotFound_ShouldReturnFailure()
    {
        var company = CreateValidCompany();

        var result = company.RemoveBenefit("Nonexistent");

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CompanyErrors.BenefitNotFound);
    }

    #endregion

    #region Counter Tests

    [Fact]
    public void IncrementJobPostingCount_ShouldIncrement()
    {
        var company = CreateValidCompany();

        company.IncrementJobPostingCount();
        company.IncrementJobPostingCount();
        company.IncrementJobPostingCount();

        company.TotalJobPostings.Should().Be(3);
    }

    [Fact]
    public void DecrementJobPostingCount_ShouldDecrement()
    {
        var company = CreateValidCompany();
        company.IncrementJobPostingCount();
        company.IncrementJobPostingCount();

        company.DecrementJobPostingCount();

        company.TotalJobPostings.Should().Be(1);
    }

    [Fact]
    public void DecrementJobPostingCount_WhenZero_ShouldStayZero()
    {
        var company = CreateValidCompany();

        company.DecrementJobPostingCount();

        company.TotalJobPostings.Should().Be(0);
    }

    #endregion

    #region Guard Methods Tests

    [Fact]
    public void CanPostJobs_WhenVerified_ShouldReturnTrue()
    {
        var company = CreateVerifiedCompany();
        company.CanPostJobs().Should().BeTrue();
    }

    [Fact]
    public void CanPostJobs_WhenPending_ShouldReturnFalse()
    {
        var company = CreateValidCompany();
        company.CanPostJobs().Should().BeFalse();
    }

    [Fact]
    public void CanPostJobs_WhenSuspended_ShouldReturnFalse()
    {
        var company = CreateVerifiedCompany();
        company.Suspend("Reason", Guid.NewGuid());
        company.CanPostJobs().Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenVerified_ShouldReturnTrue()
    {
        var company = CreateVerifiedCompany();
        company.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenPending_ShouldReturnTrue()
    {
        var company = CreateValidCompany();
        company.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenSuspended_ShouldReturnFalse()
    {
        var company = CreateVerifiedCompany();
        company.Suspend("Reason", Guid.NewGuid());
        company.IsActive().Should().BeFalse();
    }

    [Fact]
    public void IsActive_WhenInactive_ShouldReturnFalse()
    {
        var company = CreateVerifiedCompany();
        company.Deactivate("Reason");
        company.IsActive().Should().BeFalse();
    }

    #endregion

    #region Lifecycle Flow Tests

    [Fact]
    public void FullLifecycle_Register_Verify_Suspend_Reactivate()
    {
        var company = CreateValidCompany();
        company.Status.Should().Be(CompanyStatus.Pending);

        company.Verify(Guid.NewGuid()).IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Verified);

        company.Suspend("Policy violation", Guid.NewGuid()).IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Suspended);

        company.Reactivate(Guid.NewGuid()).IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Verified);
    }

    [Fact]
    public void FullLifecycle_Register_Verify_Deactivate()
    {
        var company = CreateValidCompany();

        company.Verify(Guid.NewGuid()).IsSuccess.Should().BeTrue();
        company.Deactivate("Closing business").IsSuccess.Should().BeTrue();
        company.Status.Should().Be(CompanyStatus.Inactive);
    }

    #endregion

    #region ID Tests

    [Fact]
    public void CompanyId_CreateUnique_ShouldGenerateUniqueIds()
    {
        var id1 = CompanyId.CreateUnique();
        var id2 = CompanyId.CreateUnique();

        id1.Should().NotBe(id2);
    }

    [Fact]
    public void CompanyId_Create_ShouldWrapGuid()
    {
        var guid = Guid.NewGuid();
        var id = CompanyId.Create(guid);

        id.Value.Should().Be(guid);
    }

    #endregion
}
