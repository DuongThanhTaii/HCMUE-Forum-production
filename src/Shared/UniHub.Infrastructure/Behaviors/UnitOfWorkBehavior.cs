using MediatR;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Persistence;
using UniHub.SharedKernel.Results;

namespace UniHub.Infrastructure.Behaviors;

/// <summary>
/// Persists changes after successful command handlers (single SaveChanges per request).
/// </summary>
public sealed class UnitOfWorkBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        if (!IsCommandRequest(request) || !IsSuccessfulResult(response))
        {
            return response;
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return response;
    }

    private static bool IsCommandRequest(TRequest request)
    {
        var t = request.GetType();
        if (typeof(ICommand).IsAssignableFrom(t))
        {
            return true;
        }

        return t.GetInterfaces().Any(static i =>
            i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));
    }

    private static bool IsSuccessfulResult(TResponse response) =>
        response is Result r && r.IsSuccess;
}
