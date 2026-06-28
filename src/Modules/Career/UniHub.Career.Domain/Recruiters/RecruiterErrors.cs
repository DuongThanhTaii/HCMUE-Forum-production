using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Recruiters;

public static class RecruiterErrors
{
    public static readonly Error NoPermissionsGranted = new(
        "Recruiter.NoPermissionsGranted",
        "At least one permission must be granted to a recruiter");

    public static readonly Error NotFound = new(
        "Recruiter.NotFound",
        "The recruiter was not found");

    public static readonly Error AlreadyExists = new(
        "Recruiter.AlreadyExists",
        "This user is already a recruiter for this company");

    public static readonly Error AlreadyActive = new(
        "Recruiter.AlreadyActive",
        "The recruiter is already active");

    public static readonly Error AlreadyInactive = new(
        "Recruiter.AlreadyInactive",
        "The recruiter is already inactive");

    public static readonly Error CannotRemoveSelf = new(
        "Recruiter.CannotRemoveSelf",
        "A recruiter cannot remove themselves");

    public static readonly Error CannotDeactivateSelf = new(
        "Recruiter.CannotDeactivateSelf",
        "A recruiter cannot deactivate themselves");

    public static readonly Error InsufficientPermissions = new(
        "Recruiter.InsufficientPermissions",
        "You do not have permission to perform this action");

    public static readonly Error InactiveRecruiter = new(
        "Recruiter.InactiveRecruiter",
        "This recruiter is inactive and cannot perform actions");
}
