using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Companies;

/// <summary>
/// Domain errors for Company aggregate.
/// </summary>
public static class CompanyErrors
{
    public static readonly Error NotFound = new(
        "Company.NotFound",
        "Company was not found.");

    public static readonly Error NameEmpty = new(
        "Company.NameEmpty",
        "Company name is required.");

    public static readonly Error NameTooLong = new(
        "Company.NameTooLong",
        $"Company name cannot exceed {Company.MaxNameLength} characters.");

    public static readonly Error DescriptionEmpty = new(
        "Company.DescriptionEmpty",
        "Company description is required.");

    public static readonly Error DescriptionTooLong = new(
        "Company.DescriptionTooLong",
        $"Company description cannot exceed {Company.MaxDescriptionLength} characters.");

    public static readonly Error WebsiteTooLong = new(
        "Company.WebsiteTooLong",
        $"Website URL cannot exceed {Company.MaxWebsiteLength} characters.");

    public static readonly Error InvalidWebsiteUrl = new(
        "Company.InvalidWebsiteUrl",
        "Website URL must start with http:// or https://");

    public static readonly Error LogoUrlTooLong = new(
        "Company.LogoUrlTooLong",
        $"Logo URL cannot exceed {Company.MaxLogoUrlLength} characters.");

    public static readonly Error InvalidFoundedYear = new(
        "Company.InvalidFoundedYear",
        "Founded year must be between 1800 and current year.");

    public static readonly Error RegisteredByEmpty = new(
        "Company.RegisteredByEmpty",
        "RegisteredBy user ID is required.");

    public static readonly Error AlreadyVerified = new(
        "Company.AlreadyVerified",
        "Company is already verified.");

    public static readonly Error NotVerified = new(
        "Company.NotVerified",
        "Company is not verified.");

    public static readonly Error AlreadySuspended = new(
        "Company.AlreadySuspended",
        "Company is already suspended.");

    public static readonly Error NotSuspended = new(
        "Company.NotSuspended",
        "Company is not currently suspended.");

    public static readonly Error AlreadyInactive = new(
        "Company.AlreadyInactive",
        "Company is already inactive.");

    public static readonly Error NotActive = new(
        "Company.NotActive",
        "Company is not currently active.");

    public static readonly Error SuspensionReasonRequired = new(
        "Company.SuspensionReasonRequired",
        "A reason is required when suspending a company.");

    public static readonly Error DeactivationReasonRequired = new(
        "Company.DeactivationReasonRequired",
        "A reason is required when deactivating a company.");

    public static readonly Error CannotUpdateWhileSuspended = new(
        "Company.CannotUpdateWhileSuspended",
        "Cannot update company profile while suspended.");

    public static readonly Error InvalidStatus = new(
        "Company.InvalidStatus",
        "Company is not in a valid status for this operation.");

    public static readonly Error BenefitEmpty = new(
        "Company.BenefitEmpty",
        "Benefit description cannot be empty.");

    public static readonly Error BenefitTooLong = new(
        "Company.BenefitTooLong",
        $"Benefit description cannot exceed {Company.MaxBenefitLength} characters.");

    public static readonly Error DuplicateBenefit = new(
        "Company.DuplicateBenefit",
        "This benefit already exists.");

    public static readonly Error BenefitNotFound = new(
        "Company.BenefitNotFound",
        "The specified benefit was not found.");

    public static readonly Error TooManyBenefits = new(
        "Company.TooManyBenefits",
        $"A company cannot have more than {Company.MaxBenefits} benefits.");
}
