using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Recruiters;

public sealed class RecruiterPermissions : ValueObject
{
    public bool CanManageJobPostings { get; private set; }
    public bool CanReviewApplications { get; private set; }
    public bool CanUpdateApplicationStatus { get; private set; }
    public bool CanInviteRecruiters { get; private set; }

    /// <summary>Private parameterless constructor for EF Core.</summary>
    private RecruiterPermissions() { }

    private RecruiterPermissions(
        bool canManageJobPostings,
        bool canReviewApplications,
        bool canUpdateApplicationStatus,
        bool canInviteRecruiters)
    {
        CanManageJobPostings = canManageJobPostings;
        CanReviewApplications = canReviewApplications;
        CanUpdateApplicationStatus = canUpdateApplicationStatus;
        CanInviteRecruiters = canInviteRecruiters;
    }

    public static Result<RecruiterPermissions> Create(
        bool canManageJobPostings = true,
        bool canReviewApplications = true,
        bool canUpdateApplicationStatus = true,
        bool canInviteRecruiters = false)
    {
        // At least one permission must be granted
        if (!canManageJobPostings && !canReviewApplications && !canUpdateApplicationStatus && !canInviteRecruiters)
        {
            return Result.Failure<RecruiterPermissions>(RecruiterErrors.NoPermissionsGranted);
        }

        var permissions = new RecruiterPermissions(
            canManageJobPostings,
            canReviewApplications,
            canUpdateApplicationStatus,
            canInviteRecruiters);

        return Result.Success(permissions);
    }

    public static RecruiterPermissions Default()
    {
        return new RecruiterPermissions(
            canManageJobPostings: true,
            canReviewApplications: true,
            canUpdateApplicationStatus: true,
            canInviteRecruiters: false);
    }

    public static RecruiterPermissions Admin()
    {
        return new RecruiterPermissions(
            canManageJobPostings: true,
            canReviewApplications: true,
            canUpdateApplicationStatus: true,
            canInviteRecruiters: true);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CanManageJobPostings;
        yield return CanReviewApplications;
        yield return CanUpdateApplicationStatus;
        yield return CanInviteRecruiters;
    }
}
