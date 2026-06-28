using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Authorization;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Queries.Authorization.GetAuthorizationAuditLogs;

public sealed class GetAuthorizationAuditLogsQueryHandler
    : IQueryHandler<GetAuthorizationAuditLogsQuery, IReadOnlyList<AuthorizationAuditLogItemResponse>>
{
    private readonly IAuthorizationAuditLogRepository _authorizationAuditLogRepository;

    public GetAuthorizationAuditLogsQueryHandler(IAuthorizationAuditLogRepository authorizationAuditLogRepository)
    {
        _authorizationAuditLogRepository = authorizationAuditLogRepository;
    }

    public async Task<Result<IReadOnlyList<AuthorizationAuditLogItemResponse>>> Handle(
        GetAuthorizationAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        var take = request.Take;
        if (take <= 0)
        {
            take = 100;
        }

        if (take > 500)
        {
            take = 500;
        }

        var logs = await _authorizationAuditLogRepository.GetRecentAsync(take, cancellationToken);

        var filteredLogs = logs.AsEnumerable();

        if (request.UserId.HasValue)
        {
            filteredLogs = filteredLogs.Where(item => item.ActorUserId?.Value == request.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.EndpointKey))
        {
            filteredLogs = filteredLogs.Where(item =>
                string.Equals(item.TargetType, "EndpointToggle", StringComparison.OrdinalIgnoreCase)
                && string.Equals(item.TargetKey, request.EndpointKey, StringComparison.OrdinalIgnoreCase));
        }

        if (request.IsSuccess.HasValue)
        {
            filteredLogs = filteredLogs.Where(item => item.IsSuccess == request.IsSuccess.Value);
        }

        if (request.FromUtc.HasValue)
        {
            filteredLogs = filteredLogs.Where(item => item.OccurredAtUtc >= request.FromUtc.Value);
        }

        if (request.ToUtc.HasValue)
        {
            filteredLogs = filteredLogs.Where(item => item.OccurredAtUtc <= request.ToUtc.Value);
        }

        var response = filteredLogs
            .OrderByDescending(item => item.OccurredAtUtc)
            .Select(item => new AuthorizationAuditLogItemResponse(
                item.Id,
                item.ActorUserId?.Value,
                item.Action,
                item.TargetType,
                item.TargetKey,
                item.IsSuccess,
                item.Detail,
                item.OccurredAtUtc))
            .ToList();

        return Result.Success<IReadOnlyList<AuthorizationAuditLogItemResponse>>(response);
    }
}
