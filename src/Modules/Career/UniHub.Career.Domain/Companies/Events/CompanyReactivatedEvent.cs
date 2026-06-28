using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Companies.Events;

/// <summary>
/// Raised when a suspended company is reactivated.
/// </summary>
public sealed record CompanyReactivatedEvent(
    Guid CompanyId,
    string Name,
    Guid ReactivatedBy,
    DateTime ReactivatedAt) : IDomainEvent;
