using System.Diagnostics;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace UniHub.API.Middlewares;

public sealed class UserActionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserActionLoggingMiddleware> _logger;
    private readonly UserActionLoggingOptions _options;

    public UserActionLoggingMiddleware(
        RequestDelegate next,
        ILogger<UserActionLoggingMiddleware> logger,
        IOptions<UserActionLoggingOptions> options)
    {
        _next = next;
        _logger = logger;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context, IUserActionLogStore logStore)
    {
        if (!_options.Enabled || IsExcludedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        var correlationHeaderName = ResolveCorrelationHeaderName();
        var correlationId = GetOrCreateCorrelationId(context, correlationHeaderName);
        context.Response.Headers[correlationHeaderName] = correlationId;

        var startedAtUtc = DateTime.UtcNow;
        var stopwatch = Stopwatch.StartNew();

        var capture = _options.CaptureHttpDetails;
        var requestHeadersJson = "{}";
        string? requestContentType = null;
        string? requestBodyPreview = null;
        var requestBodyTruncated = false;

        if (capture)
        {
            requestHeadersJson = UserActionHttpCapture.SerializeRequestHeaders(
                context.Request.Headers,
                _options.MaxCapturedHeadersJsonChars);
            requestContentType = context.Request.ContentType;

            if (UserActionHttpCapture.ShouldCaptureRequestBody(context.Request))
            {
                context.Request.EnableBuffering();
                var (preview, trunc) = await UserActionHttpCapture.ReadBodyPreviewAsync(
                        context.Request.Body,
                        _options.MaxCapturedBodyBytes,
                        context.RequestAborted)
                    .ConfigureAwait(false);
                context.Request.Body.Position = 0;
                requestBodyPreview = preview;
                requestBodyTruncated = trunc;
            }
        }

        Stream originalResponseBody = context.Response.Body;
        MemoryStream? responseBuffer = null;
        if (capture)
        {
            responseBuffer = new MemoryStream();
            context.Response.Body = responseBuffer;
        }

        Exception? pipelineException = null;
        try
        {
            await _next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            pipelineException = exception;
            throw;
        }
        finally
        {
            stopwatch.Stop();

            var statusCode = pipelineException is null
                ? context.Response.StatusCode
                : MapExceptionToStatusCode(pipelineException);

            var responseHeadersJson = "{}";
            string? responseContentType = null;
            string? responseBodyPreview = null;
            var responseBodyTruncated = false;

            if (capture && responseBuffer is not null)
            {
                responseHeadersJson = UserActionHttpCapture.SerializeResponseHeaders(
                    context.Response.Headers,
                    _options.MaxCapturedHeadersJsonChars);
                responseContentType = context.Response.ContentType;
                responseBuffer.Position = 0;
                if (responseBuffer.Length > 0)
                {
                    (responseBodyPreview, responseBodyTruncated) = UserActionHttpCapture.ReadResponsePreview(
                        responseBuffer,
                        _options.MaxCapturedBodyBytes);
                }

                responseBuffer.Position = 0;
                try
                {
                    await responseBuffer.CopyToAsync(originalResponseBody, context.RequestAborted).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
                catch (ObjectDisposedException)
                {
                    // ignore
                }

                await responseBuffer.DisposeAsync().ConfigureAwait(false);
            }

            var actorUserId = ResolveActorUserId(context.User);
            var endpoint = context.GetEndpoint();
            var endpointName = endpoint?.DisplayName ?? "UnknownEndpoint";
            var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            var method = context.Request.Method;
            var path = context.Request.Path.Value ?? string.Empty;
            var remoteIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var queryString = context.Request.QueryString.Value ?? string.Empty;
            var completedAtUtc = DateTime.UtcNow;
            var result = pipelineException is null ? "Success" : "Error";

            var logEntry = new UserActionLogEntry
            {
                ActionType = "UserAction",
                ActorUserId = actorUserId,
                Method = method,
                Path = path,
                QueryString = queryString,
                Endpoint = endpointName,
                StatusCode = statusCode,
                DurationMs = stopwatch.ElapsedMilliseconds,
                TraceId = traceId,
                CorrelationId = correlationId,
                RemoteIp = remoteIp,
                UserAgent = string.IsNullOrWhiteSpace(userAgent) ? "unknown" : userAgent,
                Scheme = context.Request.Scheme,
                Host = context.Request.Host.Value ?? string.Empty,
                StartedAtUtc = startedAtUtc,
                CompletedAtUtc = completedAtUtc,
                Result = result,
                ExceptionType = pipelineException?.GetType().Name,
                ExceptionMessage = pipelineException?.Message,
                RequestHeadersJson = capture ? requestHeadersJson : "{}",
                RequestContentType = capture ? requestContentType : null,
                RequestBodyPreview = capture ? requestBodyPreview : null,
                RequestBodyTruncated = capture && requestBodyTruncated,
                ResponseHeadersJson = capture ? responseHeadersJson : "{}",
                ResponseContentType = capture ? responseContentType : null,
                ResponseBodyPreview = capture ? responseBodyPreview : null,
                ResponseBodyTruncated = capture && responseBodyTruncated,
            };

            var logPayload = new Dictionary<string, object?>
            {
                ["ActionType"] = "UserAction",
                ["ActorUserId"] = actorUserId,
                ["Method"] = method,
                ["Path"] = path,
                ["Endpoint"] = endpointName,
                ["StatusCode"] = statusCode,
                ["DurationMs"] = stopwatch.ElapsedMilliseconds,
                ["TraceId"] = traceId,
                ["CorrelationId"] = correlationId,
                ["RemoteIp"] = remoteIp,
                ["UserAgent"] = string.IsNullOrWhiteSpace(userAgent) ? "unknown" : userAgent,
                ["StartedAtUtc"] = startedAtUtc,
                ["CompletedAtUtc"] = completedAtUtc,
                ["Result"] = result,
            };

            if (_options.PersistToMongo)
            {
                try
                {
                    await logStore.AppendAsync(logEntry, context.RequestAborted).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // ignore
                }
            }

            using (_logger.BeginScope(logPayload))
            {
                if (statusCode >= 500)
                {
                    _logger.LogError(
                        pipelineException,
                        "User action failed: {Method} {Path} => {StatusCode} in {DurationMs}ms (Actor: {ActorUserId}, CorrelationId: {CorrelationId})",
                        method,
                        path,
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        actorUserId,
                        correlationId);
                }
                else if (statusCode >= 400)
                {
                    _logger.LogWarning(
                        "User action completed with client error: {Method} {Path} => {StatusCode} in {DurationMs}ms (Actor: {ActorUserId}, CorrelationId: {CorrelationId})",
                        method,
                        path,
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        actorUserId,
                        correlationId);
                }
                else
                {
                    _logger.LogInformation(
                        "User action completed: {Method} {Path} => {StatusCode} in {DurationMs}ms (Actor: {ActorUserId}, CorrelationId: {CorrelationId})",
                        method,
                        path,
                        statusCode,
                        stopwatch.ElapsedMilliseconds,
                        actorUserId,
                        correlationId);
                }
            }
        }
    }

    private bool IsExcludedPath(PathString requestPath)
    {
        var pathValue = requestPath.Value;
        if (string.IsNullOrWhiteSpace(pathValue))
        {
            return false;
        }

        foreach (var prefix in _options.ExcludedPathPrefixes)
        {
            if (!string.IsNullOrWhiteSpace(prefix)
                && pathValue.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    private static string ResolveActorUserId(ClaimsPrincipal user)
    {
        return user.FindFirst("sub")?.Value
               ?? user.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? user.Identity?.Name
               ?? "anonymous";
    }

    private string ResolveCorrelationHeaderName()
    {
        return string.IsNullOrWhiteSpace(_options.CorrelationHeaderName)
            ? "X-Correlation-Id"
            : _options.CorrelationHeaderName.Trim();
    }

    private static string GetOrCreateCorrelationId(HttpContext context, string correlationHeaderName)
    {
        if (context.Request.Headers.TryGetValue(correlationHeaderName, out var values)
            && !string.IsNullOrWhiteSpace(values.FirstOrDefault()))
        {
            return values.First()!;
        }

        return Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString("N");
    }

    private static int MapExceptionToStatusCode(Exception exception)
    {
        return exception switch
        {
            OperationCanceledException => 499,
            SharedKernel.Exceptions.ValidationException => StatusCodes.Status400BadRequest,
            SharedKernel.Exceptions.NotFoundException => StatusCodes.Status404NotFound,
            SharedKernel.Exceptions.UnauthorizedException => StatusCodes.Status401Unauthorized,
            SharedKernel.Exceptions.ForbiddenException => StatusCodes.Status403Forbidden,
            SharedKernel.Exceptions.DomainException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError,
        };
    }
}
