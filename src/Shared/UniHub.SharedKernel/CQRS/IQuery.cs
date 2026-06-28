using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.SharedKernel.CQRS;

/// <summary>
/// Marker interface for queries that return a Result with a value.
/// </summary>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}
