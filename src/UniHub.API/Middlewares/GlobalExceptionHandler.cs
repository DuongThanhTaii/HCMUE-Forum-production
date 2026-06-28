using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UniHub.SharedKernel.Exceptions;

namespace UniHub.API.Middlewares;

/// <summary>
/// Global exception handler middleware that converts exceptions to ProblemDetails responses.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;

        var (statusCode, title, errors) = MapException(exception);

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Exception occurred: {Message}. TraceId: {TraceId}",
                exception.Message,
                traceId);
        }
        else
        {
            _logger.LogWarning(
                "Client error occurred: {Message}. TraceId: {TraceId}",
                exception.Message,
                traceId);
        }

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Type = GetProblemType(statusCode),
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId,
                ["timestamp"] = DateTime.UtcNow
            }
        };

        // Add validation errors if present
        if (errors is not null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        // Include exception details only in development
        if (_environment.IsDevelopment())
        {
            problemDetails.Detail = exception.InnerException is null
                ? exception.Message
                : $"{exception.Message} -> {exception.InnerException.Message}";
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            if (exception.InnerException is not null)
            {
                problemDetails.Extensions["innerExceptionType"] = exception.InnerException.GetType().Name;
            }
        }

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static (int StatusCode, string Title, IDictionary<string, string[]>? Errors) MapException(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException => (
                499,
                "Client Closed Request",
                null),

            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                validationEx.Errors),

            NotFoundException => (
                StatusCodes.Status404NotFound,
                "Resource Not Found",
                null),

            UnauthorizedException => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                null),

            ForbiddenException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                null),

            DomainException => (
                StatusCodes.Status400BadRequest,
                "Business Rule Violation",
                null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                null)
        };
    }

    private static string GetProblemType(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            StatusCodes.Status401Unauthorized => "https://tools.ietf.org/html/rfc9110#section-15.5.2",
            StatusCodes.Status403Forbidden => "https://tools.ietf.org/html/rfc9110#section-15.5.4",
            StatusCodes.Status404NotFound => "https://tools.ietf.org/html/rfc9110#section-15.5.5",
            StatusCodes.Status500InternalServerError => "https://tools.ietf.org/html/rfc9110#section-15.6.1",
            _ => "https://tools.ietf.org/html/rfc9110"
        };
    }
}
