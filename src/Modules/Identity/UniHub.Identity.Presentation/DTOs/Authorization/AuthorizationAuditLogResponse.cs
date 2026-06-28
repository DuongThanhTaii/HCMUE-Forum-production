namespace UniHub.Identity.Presentation.DTOs.Authorization;

public sealed record AuthorizationAuditLogResponse(
    Guid AuditLogId,
    Guid? ActorUserId,
    string Action,
    string TargetType,
    string? TargetKey,
    bool IsSuccess,
    string? Detail,
    DateTime OccurredAtUtc);
