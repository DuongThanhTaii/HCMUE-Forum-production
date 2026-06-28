using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Companies.Events;

/// <summary>
/// Raised when a company deactivates their account.
/// </summary>
public sealed record CompanyDeactivatedEvent(
    Guid CompanyId,
    string Name,
    string Reason,
    DateTime DeactivatedAt) : IDomainEvent;
