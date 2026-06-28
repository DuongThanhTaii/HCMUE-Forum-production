namespace UniHub.SharedKernel.Results;

/// <summary>
/// Extension methods for Result.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Maps a successful result to a new result using the provided function.
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> mappingFunc)
    {
        return result.IsSuccess
            ? Result.Success(mappingFunc(result.Value))
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Binds a successful result to a new result using the provided function.
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> bindFunc)
    {
        return result.IsSuccess
            ? bindFunc(result.Value)
            : Result.Failure<TOut>(result.Error);
    }

    /// <summary>
    /// Matches the result to either success or failure handler.
    /// </summary>
    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    /// <summary>
    /// Returns the value if successful, otherwise returns the default value.
    /// </summary>
    public static TValue GetValueOrDefault<TValue>(
        this Result<TValue> result,
        TValue defaultValue = default!)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }
}
