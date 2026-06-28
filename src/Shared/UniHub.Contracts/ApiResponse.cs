namespace UniHub.Contracts;

public sealed record ApiResponse<T>(
    bool Success,
    T? Data,
    string? Message,
    string? Error);

public static class ApiResponses
{
    public static ApiResponse<T> Success<T>(T data, string? message = null)
    {
        return new ApiResponse<T>(true, data, message, null);
    }

    public static ApiResponse<object?> Success(string message)
    {
        return new ApiResponse<object?>(true, null, message, null);
    }

    public static ApiResponse<T> Failure<T>(string error)
    {
        return new ApiResponse<T>(false, default, null, error);
    }

    public static ApiResponse<object?> Failure(string error)
    {
        return new ApiResponse<object?>(false, null, null, error);
    }
}