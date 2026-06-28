using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Contracts;
using UniHub.API.Middlewares;
using UniHub.API.Observability;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/v1/admin/observability/user-actions")]
[Produces("application/json")]
[Authorize]
public sealed class UserActionLogsController : ControllerBase
{
    private readonly IUserActionLogStore _logStore;
    private readonly UserActionLoggingOptions _options;

    public UserActionLogsController(
        IUserActionLogStore logStore,
        Microsoft.Extensions.Options.IOptions<UserActionLoggingOptions> options)
    {
        _logStore = logStore;
        _options = options.Value;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<UserActionLogSearchResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? actorUserId,
        [FromQuery] string? correlationId,
        [FromQuery] string? traceId,
        [FromQuery] string? method,
        [FromQuery] string? pathContains,
        [FromQuery] int? minStatusCode,
        [FromQuery] int? maxStatusCode,
        [FromQuery] DateTime? fromUtc,
        [FromQuery] DateTime? toUtc,
        [FromQuery] UserActionLogViewType viewType = UserActionLogViewType.Developer,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 100,
        CancellationToken cancellationToken = default)
    {
        var normalizedPage = page <= 0 ? 1 : page;
        var configuredMaxPageSize = _options.MaxQueryPageSize <= 0 ? 500 : _options.MaxQueryPageSize;
        var normalizedPageSize = pageSize <= 0
            ? (_options.DefaultQueryPageSize <= 0 ? 100 : _options.DefaultQueryPageSize)
            : Math.Min(pageSize, configuredMaxPageSize);

        var query = new UserActionLogQuery(
            actorUserId,
            correlationId,
            traceId,
            method,
            pathContains,
            minStatusCode,
            maxStatusCode,
            fromUtc,
            toUtc,
            normalizedPage,
            normalizedPageSize,
            viewType);

        var searchResult = await _logStore.SearchAsync(query, cancellationToken);

        var response = new UserActionLogSearchResponse(
            searchResult.Items.Select(item => MapToItem(item, viewType)).ToList(),
            searchResult.Total,
            searchResult.Page,
            searchResult.PageSize,
            viewType.ToString(),
            [UserActionLogViewType.Developer.ToString(), UserActionLogViewType.Administrator.ToString()],
            _options.PersistToMongo,
            _options.MongoCollectionName);

        return Ok(ApiResponses.Success(response));
    }

    private static UserActionLogItemResponse MapToItem(UserActionLogEntry entry, UserActionLogViewType viewType)
    {
        var isDeveloperView = viewType == UserActionLogViewType.Developer;

        return new UserActionLogItemResponse(
            entry.Id,
            entry.ActorUserId,
            entry.Method,
            entry.Path,
            isDeveloperView ? entry.QueryString : string.Empty,
            entry.Endpoint,
            entry.StatusCode,
            entry.DurationMs,
            entry.TraceId,
            entry.CorrelationId,
            isDeveloperView ? entry.RemoteIp : string.Empty,
            isDeveloperView ? entry.UserAgent : string.Empty,
            entry.Scheme,
            entry.Host,
            entry.StartedAtUtc,
            entry.CompletedAtUtc,
            entry.Result,
            entry.ExceptionType,
            isDeveloperView ? entry.ExceptionMessage : null,
            UserActionLogTerminalFormatter.BuildLine(entry, viewType),
            isDeveloperView ? (entry.RequestHeadersJson ?? "{}") : "{}",
            isDeveloperView ? entry.RequestContentType : null,
            isDeveloperView ? entry.RequestBodyPreview : null,
            isDeveloperView && entry.RequestBodyTruncated,
            isDeveloperView ? (entry.ResponseHeadersJson ?? "{}") : "{}",
            isDeveloperView ? entry.ResponseContentType : null,
            isDeveloperView ? entry.ResponseBodyPreview : null,
            isDeveloperView && entry.ResponseBodyTruncated);
    }
}

public sealed record UserActionLogSearchResponse(
    IReadOnlyList<UserActionLogItemResponse> Items,
    long Total,
    int Page,
    int PageSize,
    string ViewType,
    IReadOnlyList<string> AvailableViewTypes,
    bool PersistToMongo,
    string MongoCollectionName);

public sealed record UserActionLogItemResponse(
    string Id,
    string ActorUserId,
    string Method,
    string Path,
    string QueryString,
    string Endpoint,
    int StatusCode,
    long DurationMs,
    string TraceId,
    string CorrelationId,
    string RemoteIp,
    string UserAgent,
    string Scheme,
    string Host,
    DateTime StartedAtUtc,
    DateTime CompletedAtUtc,
    string Result,
    string? ExceptionType,
    string? ExceptionMessage,
    string TerminalLine,
    string RequestHeadersJson,
    string? RequestContentType,
    string? RequestBodyPreview,
    bool RequestBodyTruncated,
    string ResponseHeadersJson,
    string? ResponseContentType,
    string? ResponseBodyPreview,
    bool ResponseBodyTruncated);
