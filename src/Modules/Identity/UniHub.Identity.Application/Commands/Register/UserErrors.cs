using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Application.Commands.Register;

/// <summary>
/// Error codes for user-related operations
/// </summary>
public static class UserErrors
{
    public static Error EmailAlreadyExists => new Error(
        "User.EmailAlreadyExists",
        "A user with this email address already exists");
}
