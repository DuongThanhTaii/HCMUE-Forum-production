using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;

namespace UniHub.Identity.Infrastructure.Persistence.Repositories;

public sealed class AuthorizationAuditLogRepository : IAuthorizationAuditLogRepository
{
    private static readonly List<AuthorizationAuditLog> AuditLogs = new();
    private static readonly object LockObj = new();

    public Task AddAsync(AuthorizationAuditLog auditLog, CancellationToken cancellationToken = default)
    {
        lock (LockObj)
        {
            AuditLogs.Add(auditLog);
        }

        return Task.CompletedTask;
    }

    public Task<List<AuthorizationAuditLog>> GetRecentAsync(int take = 100, CancellationToken cancellationToken = default)
    {
        if (take <= 0)
        {
            take = 100;
        }

        lock (LockObj)
        {
            var result = AuditLogs
                .OrderByDescending(item => item.OccurredAtUtc)
                .Take(take)
                .ToList();

            return Task.FromResult(result);
        }
    }
}
