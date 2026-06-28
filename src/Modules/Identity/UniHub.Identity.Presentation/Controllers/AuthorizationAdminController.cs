using MediatR;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using UniHub.Contracts;
using UniHub.Identity.Application.Authorization;
using UniHub.Identity.Application.Abstractions;
using UniHub.Identity.Domain.Authorization;
using UniHub.Identity.Application.Commands.Authorization.RevokeGroupPermissionOverride;
using UniHub.Identity.Application.Commands.Authorization.RevokeUserPermissionOverride;
using UniHub.Identity.Application.Commands.Authorization.SetEndpointToggle;
using UniHub.Identity.Application.Commands.Authorization.UpsertGroupPermissionOverride;
using UniHub.Identity.Application.Commands.Authorization.UpsertUserPermissionOverride;
using UniHub.Identity.Application.Queries.Authorization.GetAuthorizationAuditLogs;
using UniHub.Identity.Application.Queries.Authorization.GetEndpointToggleByKey;
using UniHub.Identity.Application.Queries.Authorization.GetEndpointToggles;
using UniHub.Identity.Application.Queries.Authorization.GetGroupPermissionOverrides;
using UniHub.Identity.Application.Queries.Authorization.GetUserPermissionOverrides;
using UniHub.Identity.Presentation.DTOs.Authorization;

namespace UniHub.Identity.Presentation.Controllers;

[Route("api/v1/admin/authorization")]
[Produces("application/json")]
[RequirePermission("admin.system.manage")]
public sealed class AuthorizationAdminController : BaseApiController
{
    private const string MaintenanceToggleKey = "System.Maintenance.Mode";
    private readonly ISender _sender;
    private readonly IUserGroupRepository _userGroupRepository;
    private readonly IEndpointToggleRepository _endpointToggleRepository;
    private readonly IEnumerable<EndpointDataSource> _endpointDataSources;

    public AuthorizationAdminController(
        ISender sender,
        IUserGroupRepository userGroupRepository,
        IEndpointToggleRepository endpointToggleRepository,
        IEnumerable<EndpointDataSource> endpointDataSources)
    {
        _sender = sender;
        _userGroupRepository = userGroupRepository;
        _endpointToggleRepository = endpointToggleRepository;
        _endpointDataSources = endpointDataSources;
    }

    [HttpGet("groups")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroups(CancellationToken cancellationToken)
    {
        var groups = await _userGroupRepository.GetAllAsync(cancellationToken);
        var response = groups
            .Select(group => new
            {
                id = group.Id,
                name = group.Name,
                description = group.Description,
                isActive = group.IsActive,
                memberCount = group.Members.Count
            })
            .ToList();

        return Ok(ApiResponses.Success(response));
    }

    [HttpGet("users/{userId:guid}/overrides")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PermissionOverrideResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserOverrides(Guid userId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetUserPermissionOverridesQuery(userId), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        IReadOnlyList<PermissionOverrideResponse> response = result.Value.Select(MapToResponse).ToList();
        return Ok(ApiResponses.Success(response));
    }

    [HttpPost("users/{userId:guid}/overrides")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertUserOverride(
        Guid userId,
        [FromBody] UpsertPermissionOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpsertUserPermissionOverrideCommand(
            userId,
            request.PermissionId,
            request.ScopeType,
            request.ScopeValue,
            request.Effect,
            request.Reason,
            request.ExpiresAtUtc);

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("User permission override upserted successfully"));
    }

    [HttpDelete("users/{userId:guid}/overrides")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeUserOverride(
        Guid userId,
        [FromQuery] Guid permissionId,
        [FromQuery] string scopeType,
        [FromQuery] string? scopeValue,
        CancellationToken cancellationToken)
    {
        var command = new RevokeUserPermissionOverrideCommand(userId, permissionId, scopeType, scopeValue);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("User permission override revoked successfully"));
    }

    [HttpGet("groups/{groupId:guid}/overrides")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PermissionOverrideResponse>>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroupOverrides(Guid groupId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetGroupPermissionOverridesQuery(groupId), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        IReadOnlyList<PermissionOverrideResponse> response = result.Value.Select(MapToResponse).ToList();
        return Ok(ApiResponses.Success(response));
    }

    [HttpPost("groups/{groupId:guid}/overrides")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpsertGroupOverride(
        Guid groupId,
        [FromBody] UpsertPermissionOverrideRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpsertGroupPermissionOverrideCommand(
            groupId,
            request.PermissionId,
            request.ScopeType,
            request.ScopeValue,
            request.Effect,
            request.Reason,
            request.ExpiresAtUtc);

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Group permission override upserted successfully"));
    }

    [HttpDelete("groups/{groupId:guid}/overrides")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RevokeGroupOverride(
        Guid groupId,
        [FromQuery] Guid permissionId,
        [FromQuery] string scopeType,
        [FromQuery] string? scopeValue,
        CancellationToken cancellationToken)
    {
        var command = new RevokeGroupPermissionOverrideCommand(groupId, permissionId, scopeType, scopeValue);
        var result = await _sender.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success("Group permission override revoked successfully"));
    }

    [HttpGet("toggles")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<EndpointToggleResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEndpointToggles(CancellationToken cancellationToken)
    {
        await EnsureDiscoveredTogglesAsync(cancellationToken);

        var result = await _sender.Send(new GetEndpointTogglesQuery(), cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        IReadOnlyList<EndpointToggleResponse> response = result.Value.Select(MapEndpointToggleToResponse).ToList();
        return Ok(ApiResponses.Success(response));
    }

    [HttpGet("toggles/{endpointKey}")]
    [ProducesResponseType(typeof(ApiResponse<EndpointToggleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEndpointToggleByKey(string endpointKey, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetEndpointToggleByKeyQuery(endpointKey), cancellationToken);
        if (result.IsFailure)
        {
            return NotFound(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(MapEndpointToggleToResponse(result.Value)));
    }

    [HttpPut("toggles/{endpointKey}")]
    [ProducesResponseType(typeof(ApiResponse<EndpointToggleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetEndpointToggle(
        string endpointKey,
        [FromBody] SetEndpointToggleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetEndpointToggleCommand(
            endpointKey,
            request.IsEnabled,
            GetActorIdentifier(),
            request.Reason);

        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        return Ok(ApiResponses.Success(MapEndpointToggleToResponse(result.Value)));
    }

    [HttpGet("maintenance-mode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMaintenanceMode(CancellationToken cancellationToken)
    {
        var toggle = await _endpointToggleRepository.GetByEndpointKeyAsync(MaintenanceToggleKey, cancellationToken);
        var response = new MaintenanceModeResponse(
            IsEnabled: toggle?.IsEnabled == true,
            Reason: toggle?.Reason,
            UpdatedBy: toggle?.UpdatedBy ?? "system",
            UpdatedAtUtc: toggle?.UpdatedAtUtc ?? DateTime.UtcNow,
            Version: toggle?.Version ?? 1);

        return Ok(ApiResponses.Success(response));
    }

    [HttpPut("maintenance-mode")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetMaintenanceMode(
        [FromBody] SetMaintenanceModeRequest request,
        CancellationToken cancellationToken)
    {
        var command = new SetEndpointToggleCommand(
            MaintenanceToggleKey,
            request.IsEnabled,
            GetActorIdentifier(),
            request.Reason);
        var result = await _sender.Send(command, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        var response = new MaintenanceModeResponse(
            IsEnabled: result.Value.IsEnabled,
            Reason: result.Value.Reason,
            UpdatedBy: result.Value.UpdatedBy,
            UpdatedAtUtc: result.Value.UpdatedAtUtc,
            Version: result.Value.Version);

        return Ok(ApiResponses.Success(response));
    }

    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AuthorizationAuditLogResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAuthorizationAuditLogs(
        [FromQuery] Guid? userId,
        [FromQuery] string? endpointKey,
        [FromQuery] bool? isSuccess,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuthorizationAuditLogsQuery(
            userId,
            endpointKey,
            isSuccess,
            fromUtc,
            toUtc,
            take);

        var result = await _sender.Send(query, cancellationToken);
        if (result.IsFailure)
        {
            return BadRequest(ApiResponses.Failure(result.Error.Message));
        }

        IReadOnlyList<AuthorizationAuditLogResponse> response = result.Value.Select(MapAuditLogToResponse).ToList();
        return Ok(ApiResponses.Success(response));
    }

    private static PermissionOverrideResponse MapToResponse(PermissionOverrideItemResponse item)
    {
        return new PermissionOverrideResponse(
            item.OverrideId,
            item.PermissionId,
            item.PermissionCode,
            item.ScopeType,
            item.ScopeValue,
            item.Effect,
            item.Reason,
            item.ExpiresAtUtc,
            item.CreatedAtUtc,
            item.UpdatedAtUtc,
            item.IsRevoked);
    }

    private static EndpointToggleResponse MapEndpointToggleToResponse(EndpointToggleItemResponse item)
    {
        return new EndpointToggleResponse(
            item.EndpointKey,
            item.IsEnabled,
            item.Reason,
            item.UpdatedBy,
            item.UpdatedAtUtc,
            item.Version);
    }

    private static AuthorizationAuditLogResponse MapAuditLogToResponse(AuthorizationAuditLogItemResponse item)
    {
        return new AuthorizationAuditLogResponse(
            item.AuditLogId,
            item.ActorUserId,
            item.Action,
            item.TargetType,
            item.TargetKey,
            item.IsSuccess,
            item.Detail,
            item.OccurredAtUtc);
    }

    private string GetActorIdentifier()
    {
        return User?.FindFirst("sub")?.Value
               ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User?.Identity?.Name
               ?? "system";
    }

    private async Task EnsureDiscoveredTogglesAsync(CancellationToken cancellationToken)
    {
        var discoveredKeys = DiscoverAuthorizedEndpointKeys();
        discoveredKeys.Add(MaintenanceToggleKey);

        foreach (var key in discoveredKeys)
        {
            var existing = await _endpointToggleRepository.GetByEndpointKeyAsync(key, cancellationToken);
            if (existing is not null)
            {
                continue;
            }

            var created = EndpointToggle.Create(key, false, "system-seed", "Maintenance mode is off by default");
            if (key != MaintenanceToggleKey)
            {
                created = EndpointToggle.Create(key, true, "system-seed", "Auto discovered toggle");
            }
            if (created.IsSuccess)
            {
                await _endpointToggleRepository.AddAsync(created.Value, cancellationToken);
            }
        }
    }

    private HashSet<string> DiscoverAuthorizedEndpointKeys()
    {
        var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var source in _endpointDataSources)
        {
            foreach (var endpoint in source.Endpoints)
            {
                var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                if (descriptor is null)
                {
                    continue;
                }

                if (!IsApiControllerRoute(endpoint))
                {
                    continue;
                }

                var key = BuildEndpointKey(descriptor);
                if (!string.IsNullOrWhiteSpace(key))
                {
                    keys.Add(key);
                }
            }
        }

        return keys;
    }

    private static string? BuildEndpointKey(ControllerActionDescriptor descriptor)
    {
        var module = ExtractModuleName(descriptor.ControllerTypeInfo.Namespace);
        if (string.IsNullOrWhiteSpace(module))
        {
            return null;
        }

        var controllerName = descriptor.ControllerName;
        if (controllerName.EndsWith("Controller", StringComparison.Ordinal))
        {
            controllerName = controllerName[..^"Controller".Length];
        }

        return $"UniHub.{module}.{controllerName}.{descriptor.ActionName}";
    }

    private static bool IsApiControllerRoute(Endpoint endpoint)
    {
        if (endpoint is not RouteEndpoint routeEndpoint)
        {
            return false;
        }

        var raw = routeEndpoint.RoutePattern.RawText;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        return raw.StartsWith("api/", StringComparison.OrdinalIgnoreCase) ||
               raw.StartsWith("/api/", StringComparison.OrdinalIgnoreCase);
    }

    private static string? ExtractModuleName(string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            return null;
        }

        if (@namespace.Contains("UniHub.API.Controllers", StringComparison.OrdinalIgnoreCase))
        {
            return "API";
        }

        var segments = @namespace.Split('.', StringSplitOptions.RemoveEmptyEntries);
        var moduleIndex = Array.IndexOf(segments, "Modules");
        if (moduleIndex < 0 || moduleIndex + 1 >= segments.Length)
        {
            // Fallback for namespaces like "UniHub.Forum.Presentation.Controllers"
            if (segments.Length >= 2 &&
                segments[0].Equals("UniHub", StringComparison.OrdinalIgnoreCase))
            {
                return segments[1];
            }

            return null;
        }

        return segments[moduleIndex + 1];
    }
}
