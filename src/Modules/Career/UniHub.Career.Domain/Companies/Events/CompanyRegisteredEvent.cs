using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Companies.Events;

/// <summary>
/// Raised when a new company registers on the platform.
/// </summary>
public sealed record CompanyRegisteredEvent(
    Guid CompanyId,
    string Name,
    string Email,
    Industry Industry,
    CompanySize Size,
    Guid RegisteredBy,
    DateTime RegisteredAt) : IDomainEvent;
