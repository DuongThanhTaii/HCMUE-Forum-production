using UniHub.SharedKernel.Results;

namespace UniHub.Identity.Domain.Errors;

public static class UserErrors
{
    public static readonly Error NotFound = new("User.NotFound", "User was not found");
    public static readonly Error EmailAlreadyExists = new("User.EmailAlreadyExists", "User with this email already exists");
    public static readonly Error InvalidCredentials = new("User.InvalidCredentials", "Invalid email or password");
    public static readonly Error AccountSuspended = new("User.AccountSuspended", "User account is suspended");
    public static readonly Error AccountBanned = new("User.AccountBanned", "User account is banned");
    public static readonly Error AccountInactive = new("User.AccountInactive", "User account is inactive");
    public static readonly Error RoleAlreadyAssigned = new("User.RoleAlreadyAssigned", "Role is already assigned to user");
    public static readonly Error RoleNotAssigned = new("User.RoleNotAssigned", "Role is not assigned to user");
    public static readonly Error CannotDeleteLastRole = new("User.CannotDeleteLastRole", "Cannot remove the last role from user");
    public static readonly Error BadgeAlreadyAssigned = new("User.BadgeAlreadyAssigned", "Official badge is already assigned to user");
}