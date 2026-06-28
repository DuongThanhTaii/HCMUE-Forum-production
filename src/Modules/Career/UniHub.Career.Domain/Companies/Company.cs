using UniHub.Career.Domain.Companies.Events;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Companies;

/// <summary>
/// Company Aggregate Root - represents an organization posting jobs on the platform.
/// Manages lifecycle: Pending → Verified → Active/Suspended/Inactive.
/// </summary>
public sealed class Company : AggregateRoot<CompanyId>
{
    #region Constants

    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 5000;
    public const int MaxWebsiteLength = 500;
    public const int MaxLogoUrlLength = 1000;
    public const int MaxBenefitLength = 200;
    public const int MaxBenefits = 20;
    public const int MinFoundedYear = 1800;

    #endregion

    #region Properties

    /// <summary>Company name.</summary>
    public string Name { get; private set; }

    /// <summary>Company description/about.</summary>
    public string Description { get; private set; }

    /// <summary>Primary industry/sector.</summary>
    public Industry Industry { get; private set; }

    /// <summary>Company size by employee count.</summary>
    public CompanySize Size { get; private set; }

    /// <summary>Company website URL (optional).</summary>
    public string? Website { get; private set; }

    /// <summary>Company logo URL (optional).</summary>
    public string? LogoUrl { get; private set; }

    /// <summary>Year the company was founded (optional).</summary>
    public int? FoundedYear { get; private set; }

    /// <summary>Current verification/operational status.</summary>
    public CompanyStatus Status { get; private set; }

    /// <summary>Contact information.</summary>
    public ContactInfo ContactInfo { get; private set; }

    /// <summary>Social media links.</summary>
    public SocialLinks SocialLinks { get; private set; }

    /// <summary>The user who registered this company.</summary>
    public Guid RegisteredBy { get; private set; }

    /// <summary>When the company was registered.</summary>
    public DateTime RegisteredAt { get; private set; }

    /// <summary>When the company was verified (if verified).</summary>
    public DateTime? VerifiedAt { get; private set; }

    /// <summary>Who verified the company.</summary>
    public Guid? VerifiedBy { get; private set; }

    /// <summary>When the company was last updated.</summary>
    public DateTime? UpdatedAt { get; private set; }

    /// <summary>Total number of job postings by this company.</summary>
    public int TotalJobPostings { get; private set; }

    #endregion

    #region Collections

    private readonly List<string> _benefits = new();
    /// <summary>Employee benefits offered by the company.</summary>
    public IReadOnlyList<string> Benefits => _benefits.AsReadOnly();

    #endregion

    #region Constructors

    /// <summary>Private constructor for EF Core.</summary>
    private Company()
    {
        Name = string.Empty;
        Description = string.Empty;
        ContactInfo = null!;
        SocialLinks = null!;
    }

    private Company(
        CompanyId id,
        string name,
        string description,
        Industry industry,
        CompanySize size,
        ContactInfo contactInfo,
        Guid registeredBy,
        string? website,
        string? logoUrl,
        int? foundedYear,
        SocialLinks? socialLinks) : base(id)
    {
        Name = name;
        Description = description;
        Industry = industry;
        Size = size;
        ContactInfo = contactInfo;
        RegisteredBy = registeredBy;
        Website = website;
        LogoUrl = logoUrl;
        FoundedYear = foundedYear;
        SocialLinks = socialLinks ?? SocialLinks.Empty();
        Status = CompanyStatus.Pending;
        RegisteredAt = DateTime.UtcNow;
        TotalJobPostings = 0;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Registers a new company in Pending status.
    /// </summary>
    public static Result<Company> Register(
        string name,
        string description,
        Industry industry,
        CompanySize size,
        ContactInfo contactInfo,
        Guid registeredBy,
        string? website = null,
        string? logoUrl = null,
        int? foundedYear = null,
        SocialLinks? socialLinks = null)
    {
        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure<Company>(CompanyErrors.NameEmpty);

        if (name.Length > MaxNameLength)
            return Result.Failure<Company>(CompanyErrors.NameTooLong);

        // Validate description
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure<Company>(CompanyErrors.DescriptionEmpty);

        if (description.Length > MaxDescriptionLength)
            return Result.Failure<Company>(CompanyErrors.DescriptionTooLong);

        // Validate registeredBy
        if (registeredBy == Guid.Empty)
            return Result.Failure<Company>(CompanyErrors.RegisteredByEmpty);

        // Validate website
        if (website != null)
        {
            if (website.Length > MaxWebsiteLength)
                return Result.Failure<Company>(CompanyErrors.WebsiteTooLong);

            if (!IsValidUrl(website))
                return Result.Failure<Company>(CompanyErrors.InvalidWebsiteUrl);
        }

        // Validate logo URL
        if (logoUrl?.Length > MaxLogoUrlLength)
            return Result.Failure<Company>(CompanyErrors.LogoUrlTooLong);

        // Validate founded year
        if (foundedYear.HasValue)
        {
            var currentYear = DateTime.UtcNow.Year;
            if (foundedYear.Value < MinFoundedYear || foundedYear.Value > currentYear)
                return Result.Failure<Company>(CompanyErrors.InvalidFoundedYear);
        }

        var company = new Company(
            CompanyId.CreateUnique(),
            name.Trim(),
            description.Trim(),
            industry,
            size,
            contactInfo,
            registeredBy,
            website?.Trim(),
            logoUrl?.Trim(),
            foundedYear,
            socialLinks);

        company.AddDomainEvent(new CompanyRegisteredEvent(
            company.Id.Value,
            company.Name,
            contactInfo.Email,
            industry,
            size,
            registeredBy,
            company.RegisteredAt));

        return Result.Success(company);
    }

    #endregion

    #region Behavior Methods

    /// <summary>
    /// Verifies the company, allowing them to post jobs.
    /// </summary>
    public Result Verify(Guid verifiedBy)
    {
        if (Status == CompanyStatus.Verified)
            return Result.Failure(CompanyErrors.AlreadyVerified);

        if (verifiedBy == Guid.Empty)
            return Result.Failure(new Error("Company.VerifiedByEmpty", "VerifiedBy user ID is required."));

        Status = CompanyStatus.Verified;
        VerifiedAt = DateTime.UtcNow;
        VerifiedBy = verifiedBy;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CompanyVerifiedEvent(
            Id.Value,
            Name,
            verifiedBy,
            VerifiedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Suspends the company, preventing them from posting new jobs.
    /// </summary>
    public Result Suspend(string reason, Guid suspendedBy)
    {
        if (Status == CompanyStatus.Suspended)
            return Result.Failure(CompanyErrors.AlreadySuspended);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(CompanyErrors.SuspensionReasonRequired);

        if (suspendedBy == Guid.Empty)
            return Result.Failure(new Error("Company.SuspendedByEmpty", "SuspendedBy user ID is required."));

        Status = CompanyStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CompanySuspendedEvent(
            Id.Value,
            Name,
            reason,
            suspendedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Reactivates a suspended company.
    /// </summary>
    public Result Reactivate(Guid reactivatedBy)
    {
        if (Status != CompanyStatus.Suspended)
            return Result.Failure(CompanyErrors.NotSuspended);

        if (reactivatedBy == Guid.Empty)
            return Result.Failure(new Error("Company.ReactivatedByEmpty", "ReactivatedBy user ID is required."));

        // Return to Verified status if they were verified before
        Status = VerifiedAt.HasValue ? CompanyStatus.Verified : CompanyStatus.Pending;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CompanyReactivatedEvent(
            Id.Value,
            Name,
            reactivatedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Deactivates the company account voluntarily.
    /// </summary>
    public Result Deactivate(string reason)
    {
        if (Status == CompanyStatus.Inactive)
            return Result.Failure(CompanyErrors.AlreadyInactive);

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(CompanyErrors.DeactivationReasonRequired);

        Status = CompanyStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CompanyDeactivatedEvent(
            Id.Value,
            Name,
            reason,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Updates the company profile information.
    /// </summary>
    public Result UpdateProfile(
        string name,
        string description,
        Industry industry,
        CompanySize size,
        ContactInfo contactInfo,
        string? website,
        string? logoUrl,
        int? foundedYear,
        SocialLinks? socialLinks)
    {
        if (Status == CompanyStatus.Suspended)
            return Result.Failure(CompanyErrors.CannotUpdateWhileSuspended);

        // Validate name
        if (string.IsNullOrWhiteSpace(name))
            return Result.Failure(CompanyErrors.NameEmpty);

        if (name.Length > MaxNameLength)
            return Result.Failure(CompanyErrors.NameTooLong);

        // Validate description
        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure(CompanyErrors.DescriptionEmpty);

        if (description.Length > MaxDescriptionLength)
            return Result.Failure(CompanyErrors.DescriptionTooLong);

        // Validate website
        if (website != null)
        {
            if (website.Length > MaxWebsiteLength)
                return Result.Failure(CompanyErrors.WebsiteTooLong);

            if (!IsValidUrl(website))
                return Result.Failure(CompanyErrors.InvalidWebsiteUrl);
        }

        // Validate logo URL
        if (logoUrl?.Length > MaxLogoUrlLength)
            return Result.Failure(CompanyErrors.LogoUrlTooLong);

        // Validate founded year
        if (foundedYear.HasValue)
        {
            var currentYear = DateTime.UtcNow.Year;
            if (foundedYear.Value < MinFoundedYear || foundedYear.Value > currentYear)
                return Result.Failure(CompanyErrors.InvalidFoundedYear);
        }

        Name = name.Trim();
        Description = description.Trim();
        Industry = industry;
        Size = size;
        ContactInfo = contactInfo;
        Website = website?.Trim();
        LogoUrl = logoUrl?.Trim();
        FoundedYear = foundedYear;
        SocialLinks = socialLinks ?? SocialLinks.Empty();
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CompanyProfileUpdatedEvent(
            Id.Value,
            Name,
            UpdatedAt.Value));

        return Result.Success();
    }

    /// <summary>
    /// Adds an employee benefit to the company profile.
    /// </summary>
    public Result AddBenefit(string benefit)
    {
        if (string.IsNullOrWhiteSpace(benefit))
            return Result.Failure(CompanyErrors.BenefitEmpty);

        if (benefit.Length > MaxBenefitLength)
            return Result.Failure(CompanyErrors.BenefitTooLong);

        if (_benefits.Count >= MaxBenefits)
            return Result.Failure(CompanyErrors.TooManyBenefits);

        var trimmedBenefit = benefit.Trim();

        if (_benefits.Any(b => b.Equals(trimmedBenefit, StringComparison.OrdinalIgnoreCase)))
            return Result.Failure(CompanyErrors.DuplicateBenefit);

        _benefits.Add(trimmedBenefit);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Removes an employee benefit from the company profile.
    /// </summary>
    public Result RemoveBenefit(string benefit)
    {
        var existingBenefit = _benefits.FirstOrDefault(
            b => b.Equals(benefit, StringComparison.OrdinalIgnoreCase));

        if (existingBenefit is null)
            return Result.Failure(CompanyErrors.BenefitNotFound);

        _benefits.Remove(existingBenefit);
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Increments the total job postings count.
    /// </summary>
    public void IncrementJobPostingCount()
    {
        TotalJobPostings++;
    }

    /// <summary>
    /// Decrements the total job postings count (when a job is deleted).
    /// </summary>
    public void DecrementJobPostingCount()
    {
        if (TotalJobPostings > 0)
            TotalJobPostings--;
    }

    /// <summary>
    /// Checks if the company can post new jobs.
    /// </summary>
    public bool CanPostJobs()
        => Status == CompanyStatus.Verified;

    /// <summary>
    /// Checks if the company is active and operational.
    /// </summary>
    public bool IsActive()
        => Status == CompanyStatus.Verified || Status == CompanyStatus.Pending;

    #endregion

    #region Helper Methods

    private static bool IsValidUrl(string url)
        => url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
           url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);

    #endregion
}
