using UniHub.Learning.Domain.Faculties.Events;
using UniHub.Learning.Domain.Faculties.ValueObjects;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Faculties;

/// <summary>
/// Faculty Aggregate Root - Quản lý thông tin khoa
/// </summary>
public sealed class Faculty : AggregateRoot<FacultyId>
{
    public FacultyCode Code { get; private set; }
    public FacultyName Name { get; private set; }
    public FacultyDescription Description { get; private set; }
    public FacultyStatus Status { get; private set; }
    
    /// <summary>
    /// Manager (trưởng khoa) của Faculty này
    /// </summary>
    public Guid? ManagerId { get; private set; }
    
    /// <summary>
    /// Số lượng Course thuộc Faculty này
    /// </summary>
    public int CourseCount { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // EF Core constructor
    private Faculty()
    {
        Code = null!;
        Name = null!;
        Description = null!;
    }

    private Faculty(
        FacultyId id,
        FacultyCode code,
        FacultyName name,
        FacultyDescription description,
        Guid createdBy)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        Status = FacultyStatus.Active;
        CourseCount = 0;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Factory method để tạo Faculty mới
    /// </summary>
    public static Result<Faculty> Create(
        FacultyCode code,
        FacultyName name,
        FacultyDescription description,
        Guid createdBy,
        Guid? managerId = null)
    {
        if (createdBy == Guid.Empty)
        {
            return Result.Failure<Faculty>(
                new Error("Faculty.InvalidCreator", "Creator ID cannot be empty"));
        }

        if (managerId.HasValue && managerId.Value == Guid.Empty)
        {
            return Result.Failure<Faculty>(
                new Error("Faculty.InvalidManager", "Manager ID cannot be empty"));
        }

        var faculty = new Faculty(
            FacultyId.CreateUnique(),
            code,
            name,
            description,
            createdBy)
        {
            ManagerId = managerId
        };

        // Domain event
        faculty.AddDomainEvent(new FacultyCreatedEvent(
            faculty.Id.Value,
            code.Value,
            name.Value,
            createdBy,
            DateTime.UtcNow));

        return Result.Success(faculty);
    }

    /// <summary>
    /// Update faculty information
    /// </summary>
    public Result Update(
        FacultyName name,
        FacultyDescription description)
    {
        if (Status == FacultyStatus.Deleted)
        {
            return Result.Failure(new Error("Faculty.Deleted", "Cannot update a deleted faculty"));
        }

        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FacultyUpdatedEvent(Id.Value, CreatedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Assign manager to faculty
    /// </summary>
    public Result AssignManager(Guid managerId, Guid assignedBy)
    {
        if (managerId == Guid.Empty)
        {
            return Result.Failure(new Error("Faculty.InvalidManager", "Manager ID cannot be empty"));
        }

        if (assignedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Faculty.InvalidAssigner", "Assigner ID cannot be empty"));
        }

        if (Status == FacultyStatus.Deleted)
        {
            return Result.Failure(new Error("Faculty.Deleted", "Cannot assign manager to a deleted faculty"));
        }

        if (ManagerId == managerId)
        {
            return Result.Failure(new Error("Faculty.ManagerAlreadyAssigned", 
                "This user is already the manager of this faculty"));
        }

        ManagerId = managerId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ManagerAssignedEvent(
            Id.Value,
            managerId,
            assignedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Remove manager from faculty
    /// </summary>
    public Result RemoveManager(Guid removedBy)
    {
        if (removedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Faculty.InvalidRemover", "Remover ID cannot be empty"));
        }

        if (Status == FacultyStatus.Deleted)
        {
            return Result.Failure(new Error("Faculty.Deleted", "Cannot remove manager from a deleted faculty"));
        }

        if (!ManagerId.HasValue)
        {
            return Result.Failure(new Error("Faculty.NoManager", "This faculty does not have a manager"));
        }

        var previousManagerId = ManagerId.Value;
        ManagerId = null;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ManagerRemovedEvent(
            Id.Value,
            previousManagerId,
            removedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Check if user is manager of this faculty
    /// </summary>
    public bool IsManager(Guid userId)
    {
        return ManagerId.HasValue && ManagerId.Value == userId;
    }

    /// <summary>
    /// Deactivate faculty (ngừng hoạt động)
    /// </summary>
    public Result Deactivate(Guid deactivatedBy)
    {
        if (deactivatedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Faculty.InvalidDeactivator", "Deactivator ID cannot be empty"));
        }

        if (Status == FacultyStatus.Deleted)
        {
            return Result.Failure(new Error("Faculty.Deleted", "Cannot deactivate a deleted faculty"));
        }

        if (Status == FacultyStatus.Inactive)
        {
            return Result.Failure(new Error("Faculty.AlreadyInactive", "Faculty is already inactive"));
        }

        Status = FacultyStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FacultyDeactivatedEvent(Id.Value, deactivatedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Activate faculty (khôi phục hoạt động)
    /// </summary>
    public Result Activate(Guid activatedBy)
    {
        if (activatedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Faculty.InvalidActivator", "Activator ID cannot be empty"));
        }

        if (Status == FacultyStatus.Deleted)
        {
            return Result.Failure(new Error("Faculty.Deleted", "Cannot activate a deleted faculty"));
        }

        if (Status == FacultyStatus.Active)
        {
            return Result.Failure(new Error("Faculty.AlreadyActive", "Faculty is already active"));
        }

        Status = FacultyStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FacultyActivatedEvent(Id.Value, activatedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Delete faculty (soft delete)
    /// </summary>
    public Result Delete(Guid deletedBy)
    {
        if (deletedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Faculty.InvalidDeleter", "Deleter ID cannot be empty"));
        }

        if (Status == FacultyStatus.Deleted)
        {
            return Result.Failure(new Error("Faculty.AlreadyDeleted", "Faculty is already deleted"));
        }

        Status = FacultyStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new FacultyDeletedEvent(Id.Value, deletedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Increment course count when course is added
    /// </summary>
    public void IncrementCourseCount()
    {
        CourseCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrement course count when course is removed
    /// </summary>
    public void DecrementCourseCount()
    {
        if (CourseCount > 0)
        {
            CourseCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Check if faculty is active
    /// </summary>
    public bool IsActive() => Status == FacultyStatus.Active;

    /// <summary>
    /// Check if faculty is inactive
    /// </summary>
    public bool IsInactive() => Status == FacultyStatus.Inactive;

    /// <summary>
    /// Check if faculty is deleted
    /// </summary>
    public bool IsDeleted() => Status == FacultyStatus.Deleted;
}
