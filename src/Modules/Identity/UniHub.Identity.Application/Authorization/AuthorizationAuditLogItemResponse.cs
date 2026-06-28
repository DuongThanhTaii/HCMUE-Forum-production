namespace UniHub.Identity.Application.Authorization;

public sealed record AuthorizationAuditLogItemResponse(
    Guid AuditLogId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetKey,
    bool IsSuccess,
    string? Detail,
    DateTime OccurredAtUtc);
