using FluentValidation;
using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Application.Commands.JobPostings.UpdateJobPosting;

/// <summary>
/// Validator for UpdateJobPostingCommand.
/// </summary>
public sealed class UpdateJobPostingCommandValidator : AbstractValidator<UpdateJobPostingCommand>
{
    public UpdateJobPostingCommandValidator()
    {
        RuleFor(x => x.JobPostingId)
            .NotEmpty()
            .WithMessage("JobPostingId is required");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(JobPosting.MaxTitleLength)
            .WithMessage($"Title must not exceed {JobPosting.MaxTitleLength} characters");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(JobPosting.MaxDescriptionLength)
            .WithMessage($"Description must not exceed {JobPosting.MaxDescriptionLength} characters");

        RuleFor(x => x.City)
            .NotEmpty()
            .WithMessage("City is required")
            .MaximumLength(WorkLocation.MaxCityLength)
            .WithMessage($"City must not exceed {WorkLocation.MaxCityLength} characters");

        When(x => !string.IsNullOrWhiteSpace(x.District), () =>
        {
            RuleFor(x => x.District)
                .MaximumLength(WorkLocation.MaxDistrictLength)
                .WithMessage($"District must not exceed {WorkLocation.MaxDistrictLength} characters");
        });

        When(x => !string.IsNullOrWhiteSpace(x.Address), () =>
        {
            RuleFor(x => x.Address)
                .MaximumLength(WorkLocation.MaxAddressLength)
                .WithMessage($"Address must not exceed {WorkLocation.MaxAddressLength} characters");
        });

        // Salary validation
        When(x => x.MinSalary.HasValue || x.MaxSalary.HasValue, () =>
        {
            RuleFor(x => x.MinSalary)
                .NotNull()
                .WithMessage("MinSalary is required when salary is specified")
                .GreaterThan(0)
                .WithMessage("MinSalary must be greater than 0");

            RuleFor(x => x.MaxSalary)
                .NotNull()
                .WithMessage("MaxSalary is required when salary is specified")
                .GreaterThan(0)
                .WithMessage("MaxSalary must be greater than 0");

            RuleFor(x => x.SalaryCurrency)
                .NotEmpty()
                .WithMessage("SalaryCurrency is required when salary is specified");

            RuleFor(x => x.SalaryPeriod)
                .NotEmpty()
                .WithMessage("SalaryPeriod is required when salary is specified");
        });

        // Deadline validation
        When(x => x.Deadline.HasValue, () =>
        {
            RuleFor(x => x.Deadline)
                .Must(d => d == null || d.Value > DateTime.UtcNow)
                .WithMessage("Deadline must be in the future");
        });
    }
}
