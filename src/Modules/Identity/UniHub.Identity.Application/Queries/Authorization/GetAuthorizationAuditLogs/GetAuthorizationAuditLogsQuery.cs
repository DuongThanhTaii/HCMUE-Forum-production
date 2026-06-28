using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;

namespace UniHub.Identity.Application.Queries.Authorization.GetAuthorizationAuditLogs;

public sealed record GetAuthorizationAuditLogsQuery(
    Guid? UserId,
    string? EndpointKey,
    bool? IsSuccess,
    DateTime? FromUtc,
    DateTime? ToUtc,
    int Take = 100) : IQuery<IReadOnlyList<AuthorizationAuditLogItemResponse>>;
