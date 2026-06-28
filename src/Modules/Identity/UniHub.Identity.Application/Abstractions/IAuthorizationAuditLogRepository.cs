using UniHub.Identity.Domain.Authorization;

namespace UniHub.Identity.Application.Abstractions;

public interface IAuthorizationAuditLogRepository
{
    Task AddAsync(AuthorizationAuditLog auditLog, CancellationToken cancellationToken = default);
    Task<List<AuthorizationAuditLog>> GetRecentAsync(int take = 100, CancellationToken cancellationToken = default);
}
