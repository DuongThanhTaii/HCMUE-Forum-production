using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace UniHub.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that logs request and response information.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestGuid = Guid.NewGuid().ToString();

        _logger.LogInformation(
            "[START] {RequestName} ({RequestGuid}) - Request: {@Request}",
            requestName,
            requestGuid,
            request);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();

            stopwatch.Stop();

            _logger.LogInformation(
                "[END] {RequestName} ({RequestGuid}) completed in {ElapsedMilliseconds}ms - Response: {@Response}",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds,
                response);

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            stopwatch.Stop();

            _logger.LogInformation(
                "[CANCELLED] {RequestName} ({RequestGuid}) cancelled after {ElapsedMilliseconds}ms",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "[ERROR] {RequestName} ({RequestGuid}) failed after {ElapsedMilliseconds}ms",
                requestName,
                requestGuid,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
