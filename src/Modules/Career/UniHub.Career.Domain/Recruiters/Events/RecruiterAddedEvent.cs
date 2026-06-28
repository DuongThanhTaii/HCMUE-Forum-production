using UniHub.Career.Domain.Companies;
using UniHub.SharedKernel.Domain;

namespace UniHub.Career.Domain.Recruiters.Events;

public sealed record RecruiterAddedEvent(
    RecruiterId RecruiterId,
    Guid UserId,
    CompanyId CompanyId,
    Guid AddedBy,
    RecruiterPermissions Permissions) : IDomainEvent;
