using UniHub.API.Middlewares;

namespace UniHub.API.Observability;

public static class UserActionLogTerminalFormatter
{
    public static string BuildLine(UserActionLogEntry entry, UserActionLogViewType viewType)
    {
        var timestamp = entry.CompletedAtUtc.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var status = entry.StatusCode;
        var method = entry.Method;
        var path = entry.Path;
        var actor = string.IsNullOrWhiteSpace(entry.ActorUserId) ? "anonymous" : entry.ActorUserId;
        var duration = entry.DurationMs;

        if (viewType == UserActionLogViewType.Administrator)
        {
            return $"[{timestamp}] [{status}] {method} {path} actor={actor} duration={duration}ms corr={entry.CorrelationId}";
        }

        var trace = string.IsNullOrWhiteSpace(entry.TraceId) ? "n/a" : entry.TraceId;
        var endpoint = string.IsNullOrWhiteSpace(entry.Endpoint) ? "UnknownEndpoint" : entry.Endpoint;
        var exceptionPart = string.IsNullOrWhiteSpace(entry.ExceptionType) ? string.Empty : $" ex={entry.ExceptionType}";

        return $"[{timestamp}] [{status}] {method} {path} endpoint=\"{endpoint}\" actor={actor} duration={duration}ms trace={trace} corr={entry.CorrelationId}{exceptionPart}";
    }
}
