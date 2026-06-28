using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Login;

/// <summary>
/// Error codes for login operations
/// </summary>
public static class LoginErrors
{
    public static Error InvalidCredentials => new Error(
        "Login.InvalidCredentials",
        "Invalid email or password");

    public static Error UserNotActive => new Error(
        "Login.UserNotActive",
        "User account is not active");
}
