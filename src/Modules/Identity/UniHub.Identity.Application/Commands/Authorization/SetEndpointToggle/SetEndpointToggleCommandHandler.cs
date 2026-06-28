using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Application.Authorization;
using UniHub.Identity.Domain.Authorization;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Authorization.SetEndpointToggle;

public sealed class SetEndpointToggleCommandHandler : ICommandHandler<SetEndpointToggleCommand, EndpointToggleItemResponse>
{
    private readonly IEndpointToggleRepository _endpointToggleRepository;
    private readonly IAuthorizationAuditLogRepository _authorizationAuditLogRepository;

    public SetEndpointToggleCommandHandler(
        IEndpointToggleRepository endpointToggleRepository,
        IAuthorizationAuditLogRepository authorizationAuditLogRepository)
    {
        _endpointToggleRepository = endpointToggleRepository;
        _authorizationAuditLogRepository = authorizationAuditLogRepository;
    }

    public async Task<Result<EndpointToggleItemResponse>> Handle(
        SetEndpointToggleCommand request,
        CancellationToken cancellationToken)
    {
        var endpointKey = request.EndpointKey.Trim();
        var existing = await _endpointToggleRepository.GetByEndpointKeyAsync(endpointKey, cancellationToken);

        if (existing is null)
        {
            var createResult = EndpointToggle.Create(endpointKey, request.IsEnabled, request.UpdatedBy, request.Reason);
            if (createResult.IsFailure)
            {
                await WriteAuditAsync("EndpointToggle.Create", endpointKey, false, createResult.Error.Message, cancellationToken);
                return Result.Failure<EndpointToggleItemResponse>(createResult.Error);
            }

            var created = createResult.Value;
            await _endpointToggleRepository.AddAsync(created, cancellationToken);
            await WriteAuditAsync("EndpointToggle.Create", endpointKey, true, BuildDetail(request), cancellationToken);

            return Result.Success(Map(created));
        }

        var setStatusResult = existing.SetStatus(request.IsEnabled, request.UpdatedBy, request.Reason);
        if (setStatusResult.IsFailure)
        {
            await WriteAuditAsync("EndpointToggle.Update", endpointKey, false, setStatusResult.Error.Message, cancellationToken);
            return Result.Failure<EndpointToggleItemResponse>(setStatusResult.Error);
        }

        await _endpointToggleRepository.UpdateAsync(existing, cancellationToken);
        await WriteAuditAsync("EndpointToggle.Update", endpointKey, true, BuildDetail(request), cancellationToken);

        return Result.Success(Map(existing));
    }

    private async Task WriteAuditAsync(
        string action,
        string endpointKey,
        bool isSuccess,
        string? detail,
        CancellationToken cancellationToken)
    {
        var auditResult = AuthorizationAuditLog.Create(
            actorUserId: null,
            action: action,
            targetType: "EndpointToggle",
            targetKey: endpointKey,
            isSuccess: isSuccess,
            detail: detail);

        if (auditResult.IsSuccess)
        {
            await _authorizationAuditLogRepository.AddAsync(auditResult.Value, cancellationToken);
        }
    }

    private static EndpointToggleItemResponse Map(EndpointToggle toggle)
    {
        return new EndpointToggleItemResponse(
            toggle.EndpointKey,
            toggle.IsEnabled,
            toggle.Reason,
            toggle.UpdatedBy,
            toggle.UpdatedAtUtc,
            toggle.Version);
    }

    private static string BuildDetail(SetEndpointToggleCommand request)
    {
        var statusText = request.IsEnabled ? "enabled" : "disabled";
        return $"Endpoint '{request.EndpointKey}' set to {statusText}. Reason: {request.Reason ?? "N/A"}";
    }
}
