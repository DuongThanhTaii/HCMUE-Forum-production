using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.SharedKernel.CQRS;

/// <summary>
/// Handler interface for queries that return a Result with a value.
/// </summary>
/// <typeparam name="TQuery">The type of query to handle.</typeparam>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
