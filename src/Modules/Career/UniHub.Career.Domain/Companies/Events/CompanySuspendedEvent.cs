using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Companies.Events;

/// <summary>
/// Raised when a company is suspended by an administrator.
/// </summary>
public sealed record CompanySuspendedEvent(
    Guid CompanyId,
    string Name,
    string Reason,
    Guid SuspendedBy,
    DateTime SuspendedAt) : IDomainEvent;
