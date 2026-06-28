using FluentValidation;
using UniHub.Career.Domain.Companies;

namespace UniHub.Career.Application.Commands.Companies.RegisterCompany;

/// <summary>
/// Validator for RegisterCompanyCommand.
/// </summary>
public sealed class RegisterCompanyCommandValidator : AbstractValidator<RegisterCompanyCommand>
{
    public RegisterCompanyCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Company name is required.")
            .MaximumLength(Company.MaxNameLength).WithMessage($"Company name cannot exceed {Company.MaxNameLength} characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(Company.MaxDescriptionLength).WithMessage($"Description cannot exceed {Company.MaxDescriptionLength} characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(ContactInfo.MaxEmailLength).WithMessage($"Email cannot exceed {ContactInfo.MaxEmailLength} characters.");

        RuleFor(x => x.Phone)
            .MaximumLength(ContactInfo.MaxPhoneLength).When(x => !string.IsNullOrWhiteSpace(x.Phone))
            .WithMessage($"Phone cannot exceed {ContactInfo.MaxPhoneLength} characters.");

        RuleFor(x => x.Address)
            .MaximumLength(ContactInfo.MaxAddressLength).When(x => !string.IsNullOrWhiteSpace(x.Address))
            .WithMessage($"Address cannot exceed {ContactInfo.MaxAddressLength} characters.");

        RuleFor(x => x.RegisteredBy)
            .NotEmpty().WithMessage("RegisteredBy is required.");

        RuleFor(x => x.Website)
            .Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.Website))
            .WithMessage("Website must be a valid URL starting with http:// or https://")
            .MaximumLength(Company.MaxWebsiteLength).When(x => !string.IsNullOrWhiteSpace(x.Website))
            .WithMessage($"Website URL cannot exceed {Company.MaxWebsiteLength} characters.");

        RuleFor(x => x.FoundedYear)
            .InclusiveBetween(Company.MinFoundedYear, DateTime.UtcNow.Year)
            .When(x => x.FoundedYear.HasValue)
            .WithMessage($"Founded year must be between {Company.MinFoundedYear} and {DateTime.UtcNow.Year}.");

        RuleFor(x => x.LinkedIn)
            .Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.LinkedIn))
            .WithMessage("LinkedIn URL must be a valid URL starting with http:// or https://")
            .MaximumLength(SocialLinks.MaxUrlLength).When(x => !string.IsNullOrWhiteSpace(x.LinkedIn))
            .WithMessage($"LinkedIn URL cannot exceed {SocialLinks.MaxUrlLength} characters.");

        RuleFor(x => x.Facebook)
            .Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.Facebook))
            .WithMessage("Facebook URL must be a valid URL starting with http:// or https://");

        RuleFor(x => x.Twitter)
            .Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.Twitter))
            .WithMessage("Twitter URL must be a valid URL starting with http:// or https://");

        RuleFor(x => x.Instagram)
            .Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.Instagram))
            .WithMessage("Instagram URL must be a valid URL starting with http:// or https://");

        RuleFor(x => x.YouTube)
            .Must(BeValidUrl).When(x => !string.IsNullOrWhiteSpace(x.YouTube))
            .WithMessage("YouTube URL must be a valid URL starting with http:// or https://");
    }

    private static bool BeValidUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true;

        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
               url.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
    }
}
