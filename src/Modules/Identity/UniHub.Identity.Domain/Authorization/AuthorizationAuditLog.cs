using UniHub.Identity.Domain.Users;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Authorization;

public sealed class AuthorizationAuditLog : Entity<Guid>
{
    public UserId? ActorUserId { get; private set; }
    public string Action { get; private set; }
    public string TargetType { get; private set; }
    public string? TargetKey { get; private set; }
    public bool IsSuccess { get; private set; }
    public string? Detail { get; private set; }
    public DateTime OccurredAtUtc { get; private set; }

    private AuthorizationAuditLog()
    {
        Action = string.Empty;
        TargetType = string.Empty;
    }

    private AuthorizationAuditLog(
        UserId? actorUserId,
        string action,
        string targetType,
        string? targetKey,
        bool isSuccess,
        string? detail)
    {
        Id = Guid.NewGuid();
        ActorUserId = actorUserId;
        Action = action;
        TargetType = targetType;
        TargetKey = targetKey;
        IsSuccess = isSuccess;
        Detail = detail;
        OccurredAtUtc = DateTime.UtcNow;
    }

    public static Result<AuthorizationAuditLog> Create(
        UserId? actorUserId,
        string action,
        string targetType,
        string? targetKey,
        bool isSuccess,
        string? detail = null)
    {
        if (string.IsNullOrWhiteSpace(action))
        {
            return Result.Failure<AuthorizationAuditLog>(new Error(
                "AuthorizationAuditLog.Action.Empty",
                "Action cannot be empty."));
        }

        if (string.IsNullOrWhiteSpace(targetType))
        {
            return Result.Failure<AuthorizationAuditLog>(new Error(
                "AuthorizationAuditLog.TargetType.Empty",
                "TargetType cannot be empty."));
        }

        if (action.Trim().Length > 120)
        {
            return Result.Failure<AuthorizationAuditLog>(new Error(
                "AuthorizationAuditLog.Action.TooLong",
                "Action cannot exceed 120 characters."));
        }

        if (targetType.Trim().Length > 120)
        {
            return Result.Failure<AuthorizationAuditLog>(new Error(
                "AuthorizationAuditLog.TargetType.TooLong",
                "TargetType cannot exceed 120 characters."));
        }

        if (targetKey?.Length > 200)
        {
            return Result.Failure<AuthorizationAuditLog>(new Error(
                "AuthorizationAuditLog.TargetKey.TooLong",
                "TargetKey cannot exceed 200 characters."));
        }

        if (detail?.Length > 2000)
        {
            return Result.Failure<AuthorizationAuditLog>(new Error(
                "AuthorizationAuditLog.Detail.TooLong",
                "Detail cannot exceed 2000 characters."));
        }

        return Result.Success(new AuthorizationAuditLog(
            actorUserId,
            action.Trim(),
            targetType.Trim(),
            targetKey?.Trim(),
            isSuccess,
            detail?.Trim()));
    }
}
