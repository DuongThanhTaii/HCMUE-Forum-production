using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Companies.Events;

/// <summary>
/// Raised when a company's profile is updated.
/// </summary>
public sealed record CompanyProfileUpdatedEvent(
    Guid CompanyId,
    string Name,
    DateTime UpdatedAt) : IDomainEvent;
