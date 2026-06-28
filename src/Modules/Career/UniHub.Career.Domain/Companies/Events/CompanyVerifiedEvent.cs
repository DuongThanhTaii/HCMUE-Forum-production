using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Companies.Events;

/// <summary>
/// Raised when a company is verified by an administrator.
/// </summary>
public sealed record CompanyVerifiedEvent(
    Guid CompanyId,
    string Name,
    Guid VerifiedBy,
    DateTime VerifiedAt) : IDomainEvent;
