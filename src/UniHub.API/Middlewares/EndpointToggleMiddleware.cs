using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using UniHub.Identity.Application.Abstractions;

namespace UniHub.API.Middlewares;

public sealed class EndpointToggleMiddleware
{
    private const string MaintenanceToggleKey = "System.Maintenance.Mode";
    private readonly RequestDelegate _next;
    private readonly ILogger<EndpointToggleMiddleware> _logger;

    public EndpointToggleMiddleware(RequestDelegate next, ILogger<EndpointToggleMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IEndpointToggleRepository endpointToggleRepository)
    {
        var isBypassPath = IsBypassPath(context.Request.Path);
        var maintenanceToggle = await endpointToggleRepository.GetByEndpointKeyAsync(MaintenanceToggleKey, context.RequestAborted);
        if (maintenanceToggle?.IsEnabled == true && !isBypassPath)
        {
            var maintenanceDetails = new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Maintenance Mode",
                Detail = maintenanceToggle.Reason ?? "System is temporarily under maintenance.",
                Type = "https://tools.ietf.org/html/rfc9110#section-15.6.4",
                Instance = context.Request.Path
            };

            maintenanceDetails.Extensions["reasonCode"] = "MaintenanceModeEnabled";
            maintenanceDetails.Extensions["timestamp"] = DateTime.UtcNow;

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(maintenanceDetails, context.RequestAborted);
            return;
        }

        // Never block control-plane and health/docs routes with endpoint toggles.
        if (isBypassPath)
        {
            await _next(context);
            return;
        }

        var endpoint = context.GetEndpoint();
        if (endpoint is null)
        {
            await _next(context);
            return;
        }

        var endpointKey = BuildEndpointKey(endpoint);
        if (string.IsNullOrWhiteSpace(endpointKey))
        {
            await _next(context);
            return;
        }

        var toggle = await endpointToggleRepository.GetByEndpointKeyAsync(endpointKey, context.RequestAborted);
        if (toggle is null || toggle.IsEnabled)
        {
            await _next(context);
            return;
        }

        _logger.LogWarning(
            "Request blocked because endpoint toggle is disabled. EndpointKey: {EndpointKey}, Path: {Path}",
            endpointKey,
            context.Request.Path.Value);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Endpoint Disabled",
            Detail = toggle.Reason ?? "This endpoint is temporarily disabled.",
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            Instance = context.Request.Path
        };

        problemDetails.Extensions["endpointKey"] = endpointKey;
        problemDetails.Extensions["reasonCode"] = "EndpointDisabled";
        problemDetails.Extensions["timestamp"] = DateTime.UtcNow;

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(problemDetails, context.RequestAborted);
    }

    private static string? BuildEndpointKey(Endpoint endpoint)
    {
        var descriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
        if (descriptor is null)
        {
            return null;
        }

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
            return null;
        }

        return segments[moduleIndex + 1];
    }

    private static bool IsBypassPath(PathString path)
    {
        if (!path.HasValue)
        {
            return false;
        }

        var value = path.Value!;
        return value.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("/api/v1/admin/authorization/maintenance-mode", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("/api/v1/admin/authorization/toggles", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase) ||
               value.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase);
    }
}
