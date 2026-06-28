using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters.Events;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.Recruiters;

public sealed class Recruiter : AggregateRoot<RecruiterId>
{
    public Guid UserId { get; private set; }
    public CompanyId CompanyId { get; private set; }
    public RecruiterPermissions Permissions { get; private set; }
    public RecruiterStatus Status { get; private set; }
    public Guid AddedBy { get; private set; }
    public DateTime AddedAt { get; private set; }
    public DateTime? LastModifiedAt { get; private set; }

    private Recruiter(
        RecruiterId id,
        Guid userId,
        CompanyId companyId,
        RecruiterPermissions permissions,
        Guid addedBy) : base(id)
    {
        UserId = userId;
        CompanyId = companyId;
        Permissions = permissions;
        Status = RecruiterStatus.Active;
        AddedBy = addedBy;
        AddedAt = DateTime.UtcNow;
    }

    // For EF Core
    private Recruiter()
    {
        CompanyId = null!;
        Permissions = null!;
    }

    /// <summary>
    /// Factory method to add a recruiter to a company
    /// </summary>
    public static Result<Recruiter> Add(
        Guid userId,
        CompanyId companyId,
        RecruiterPermissions permissions,
        Guid addedBy)
    {
        if (userId == Guid.Empty)
        {
            return Result.Failure<Recruiter>(new Error("Recruiter.InvalidUserId", "User ID cannot be empty"));
        }

        if (companyId == null || companyId.Value == Guid.Empty)
        {
            return Result.Failure<Recruiter>(new Error("Recruiter.InvalidCompanyId", "Company ID cannot be empty"));
        }

        if (addedBy == Guid.Empty)
        {
            return Result.Failure<Recruiter>(new Error("Recruiter.InvalidAddedBy", "AddedBy user ID cannot be empty"));
        }

        var recruiter = new Recruiter(
            RecruiterId.CreateUnique(),
            userId,
            companyId,
            permissions,
            addedBy);

        recruiter.AddDomainEvent(new RecruiterAddedEvent(
            recruiter.Id,
            userId,
            companyId,
            addedBy,
            permissions));

        return Result.Success(recruiter);
    }

    /// <summary>
    /// Update recruiter permissions
    /// </summary>
    public Result UpdatePermissions(RecruiterPermissions newPermissions, Guid updatedBy)
    {
        if (updatedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Recruiter.InvalidUpdatedBy", "UpdatedBy user ID cannot be empty"));
        }

        if (Status == RecruiterStatus.Inactive)
        {
            return Result.Failure(RecruiterErrors.InactiveRecruiter);
        }

        Permissions = newPermissions;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new RecruiterPermissionsUpdatedEvent(Id, updatedBy, newPermissions));

        return Result.Success();
    }

    /// <summary>
    /// Deactivate the recruiter
    /// </summary>
    public Result Deactivate(Guid deactivatedBy)
    {
        if (deactivatedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Recruiter.InvalidDeactivatedBy", "DeactivatedBy user ID cannot be empty"));
        }

        if (UserId == deactivatedBy)
        {
            return Result.Failure(RecruiterErrors.CannotDeactivateSelf);
        }

        if (Status == RecruiterStatus.Inactive)
        {
            return Result.Failure(RecruiterErrors.AlreadyInactive);
        }

        Status = RecruiterStatus.Inactive;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new RecruiterDeactivatedEvent(Id, deactivatedBy));

        return Result.Success();
    }

    /// <summary>
    /// Reactivate the recruiter
    /// </summary>
    public Result Reactivate(Guid reactivatedBy)
    {
        if (reactivatedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Recruiter.InvalidReactivatedBy", "ReactivatedBy user ID cannot be empty"));
        }

        if (Status == RecruiterStatus.Active)
        {
            return Result.Failure(RecruiterErrors.AlreadyActive);
        }

        Status = RecruiterStatus.Active;
        LastModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new RecruiterReactivatedEvent(Id, reactivatedBy));

        return Result.Success();
    }

    /// <summary>
    /// Check if recruiter is active
    /// </summary>
    public bool IsActive() => Status == RecruiterStatus.Active;

    /// <summary>
    /// Check if recruiter has specific permission
    /// </summary>
    public bool HasPermission(Func<RecruiterPermissions, bool> permissionCheck)
    {
        if (!IsActive())
        {
            return false;
        }

        return permissionCheck(Permissions);
    }
}
