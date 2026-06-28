using MediatR;
using Microsoft.Extensions.Logging;

namespace UniHub.Infrastructure.Behaviors;

/// <summary>
/// Pipeline behavior that catches unhandled exceptions.
/// </summary>
public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> _logger;

    public UnhandledExceptionBehavior(ILogger<UnhandledExceptionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            var requestName = typeof(TRequest).Name;
            _logger.LogInformation(
                "Request {RequestName} was cancelled by client.",
                requestName);
            throw;
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(
                ex,
                "Unhandled Exception for Request {RequestName} - Request: {@Request}",
                requestName,
                request);

            throw;
        }
    }
}
