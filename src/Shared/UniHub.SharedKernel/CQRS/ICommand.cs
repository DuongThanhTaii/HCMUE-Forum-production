using MediatR;
using UniHub.SharedKernel.Results;

namespace UniHub.SharedKernel.CQRS;

/// <summary>
/// Marker interface for commands that return a Result.
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Marker interface for commands that return a Result with a value.
/// </summary>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}
