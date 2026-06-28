using UniHub.Learning.Domain.Courses.Events;
using UniHub.Learning.Domain.Courses.ValueObjects;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Learning.Domain.Courses;

/// <summary>
/// Course Aggregate Root - Quản lý thông tin môn học
/// </summary>
public sealed class Course : AggregateRoot<CourseId>
{
    private readonly List<Guid> _moderatorIds = new();

    public CourseCode Code { get; private set; }
    public CourseName Name { get; private set; }
    public CourseDescription Description { get; private set; }
    public Semester Semester { get; private set; }
    public CourseStatus Status { get; private set; }
    
    /// <summary>
    /// Faculty ID mà course này thuộc về
    /// </summary>
    public Guid? FacultyId { get; private set; }
    
    /// <summary>
    /// List of moderator IDs (giảng viên/moderator được assign cho course này)
    /// </summary>
    public IReadOnlyList<Guid> ModeratorIds => _moderatorIds.AsReadOnly();
    
    /// <summary>
    /// Số credits của course (default = 3)
    /// </summary>
    public int Credits { get; private set; }
    
    /// <summary>
    /// Số lượng document thuộc course này
    /// </summary>
    public int DocumentCount { get; private set; }
    
    /// <summary>
    /// Số lượng student enrolled (nếu có tính năng enrollment)
    /// </summary>
    public int EnrollmentCount { get; private set; }
    
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    // EF Core constructor
    private Course()
    {
        Code = null!;
        Name = null!;
        Description = null!;
        Semester = null!;
    }

    private Course(
        CourseId id,
        CourseCode code,
        CourseName name,
        CourseDescription description,
        Semester semester,
        int credits,
        Guid? facultyId,
        Guid createdBy)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        Semester = semester;
        Credits = credits;
        FacultyId = facultyId;
        Status = CourseStatus.Active;
        DocumentCount = 0;
        EnrollmentCount = 0;
        CreatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;
    }

    /// <summary>
    /// Factory method để tạo Course mới
    /// </summary>
    public static Result<Course> Create(
        CourseCode code,
        CourseName name,
        CourseDescription description,
        Semester semester,
        int credits,
        Guid createdBy,
        Guid? facultyId = null)
    {
        if (createdBy == Guid.Empty)
        {
            return Result.Failure<Course>(
                new Error("Course.InvalidCreator", "Creator ID cannot be empty"));
        }

        if (credits <= 0 || credits > 10)
        {
            return Result.Failure<Course>(
                new Error("Course.InvalidCredits", "Credits must be between 1 and 10"));
        }

        var course = new Course(
            CourseId.CreateUnique(),
            code,
            name,
            description,
            semester,
            credits,
            facultyId,
            createdBy);

        // Domain event
        course.AddDomainEvent(new CourseCreatedEvent(
            course.Id.Value,
            code.Value,
            name.Value,
            createdBy,
            DateTime.UtcNow));

        return Result.Success(course);
    }

    /// <summary>
    /// Update course information
    /// </summary>
    public Result Update(
        CourseName name,
        CourseDescription description,
        Semester semester,
        int credits,
        Guid? facultyId)
    {
        if (Status == CourseStatus.Deleted)
        {
            return Result.Failure(new Error("Course.Deleted", "Cannot update a deleted course"));
        }

        if (credits <= 0 || credits > 10)
        {
            return Result.Failure(new Error("Course.InvalidCredits", "Credits must be between 1 and 10"));
        }

        Name = name;
        Description = description;
        Semester = semester;
        Credits = credits;
        FacultyId = facultyId;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CourseUpdatedEvent(Id.Value, CreatedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Assign moderator to course
    /// </summary>
    public Result AssignModerator(Guid moderatorId, Guid assignedBy)
    {
        if (moderatorId == Guid.Empty)
        {
            return Result.Failure(new Error("Course.InvalidModerator", "Moderator ID cannot be empty"));
        }

        if (assignedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Course.InvalidAssigner", "Assigner ID cannot be empty"));
        }

        if (Status == CourseStatus.Deleted)
        {
            return Result.Failure(new Error("Course.Deleted", "Cannot assign moderator to a deleted course"));
        }

        if (_moderatorIds.Contains(moderatorId))
        {
            return Result.Failure(new Error("Course.ModeratorAlreadyAssigned", 
                "This moderator is already assigned to this course"));
        }

        _moderatorIds.Add(moderatorId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ModeratorAssignedEvent(
            Id.Value,
            moderatorId,
            assignedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Remove moderator from course
    /// </summary>
    public Result RemoveModerator(Guid moderatorId, Guid removedBy)
    {
        if (moderatorId == Guid.Empty)
        {
            return Result.Failure(new Error("Course.InvalidModerator", "Moderator ID cannot be empty"));
        }

        if (removedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Course.InvalidRemover", "Remover ID cannot be empty"));
        }

        if (Status == CourseStatus.Deleted)
        {
            return Result.Failure(new Error("Course.Deleted", "Cannot remove moderator from a deleted course"));
        }

        if (!_moderatorIds.Contains(moderatorId))
        {
            return Result.Failure(new Error("Course.ModeratorNotFound", 
                "This moderator is not assigned to this course"));
        }

        _moderatorIds.Remove(moderatorId);
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new ModeratorRemovedEvent(
            Id.Value,
            moderatorId,
            removedBy,
            DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Check if user is moderator of this course
    /// </summary>
    public bool IsModerator(Guid userId)
    {
        return _moderatorIds.Contains(userId);
    }

    /// <summary>
    /// Archive course (course completed but still viewable)
    /// </summary>
    public Result Archive(Guid archivedBy)
    {
        if (archivedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Course.InvalidArchiver", "Archiver ID cannot be empty"));
        }

        if (Status == CourseStatus.Deleted)
        {
            return Result.Failure(new Error("Course.Deleted", "Cannot archive a deleted course"));
        }

        if (Status == CourseStatus.Archived)
        {
            return Result.Failure(new Error("Course.AlreadyArchived", "Course is already archived"));
        }

        Status = CourseStatus.Archived;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CourseArchivedEvent(Id.Value, archivedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Activate course (restore from archived)
    /// </summary>
    public Result Activate(Guid activatedBy)
    {
        if (activatedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Course.InvalidActivator", "Activator ID cannot be empty"));
        }

        if (Status == CourseStatus.Deleted)
        {
            return Result.Failure(new Error("Course.Deleted", "Cannot activate a deleted course"));
        }

        if (Status == CourseStatus.Active)
        {
            return Result.Failure(new Error("Course.AlreadyActive", "Course is already active"));
        }

        Status = CourseStatus.Active;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CourseActivatedEvent(Id.Value, activatedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Mark course as completed
    /// </summary>
    public Result Complete()
    {
        if (Status == CourseStatus.Deleted)
        {
            return Result.Failure(new Error("Course.Deleted", "Cannot complete a deleted course"));
        }

        if (Status == CourseStatus.Completed)
        {
            return Result.Failure(new Error("Course.AlreadyCompleted", "Course is already completed"));
        }

        Status = CourseStatus.Completed;
        UpdatedAt = DateTime.UtcNow;

        return Result.Success();
    }

    /// <summary>
    /// Delete course (soft delete)
    /// </summary>
    public Result Delete(Guid deletedBy)
    {
        if (deletedBy == Guid.Empty)
        {
            return Result.Failure(new Error("Course.InvalidDeleter", "Deleter ID cannot be empty"));
        }

        if (Status == CourseStatus.Deleted)
        {
            return Result.Failure(new Error("Course.AlreadyDeleted", "Course is already deleted"));
        }

        Status = CourseStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new CourseDeletedEvent(Id.Value, deletedBy, DateTime.UtcNow));

        return Result.Success();
    }

    /// <summary>
    /// Increment document count when document is added
    /// </summary>
    public void IncrementDocumentCount()
    {
        DocumentCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrement document count when document is removed
    /// </summary>
    public void DecrementDocumentCount()
    {
        if (DocumentCount > 0)
        {
            DocumentCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Increment enrollment count
    /// </summary>
    public void IncrementEnrollmentCount()
    {
        EnrollmentCount++;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Decrement enrollment count
    /// </summary>
    public void DecrementEnrollmentCount()
    {
        if (EnrollmentCount > 0)
        {
            EnrollmentCount--;
            UpdatedAt = DateTime.UtcNow;
        }
    }

    /// <summary>
    /// Check if course is active
    /// </summary>
    public bool IsActive() => Status == CourseStatus.Active;

    /// <summary>
    /// Check if course is archived
    /// </summary>
    public bool IsArchived() => Status == CourseStatus.Archived;

    /// <summary>
    /// Check if course is completed
    /// </summary>
    public bool IsCompleted() => Status == CourseStatus.Completed;

    /// <summary>
    /// Check if course is deleted
    /// </summary>
    public bool IsDeleted() => Status == CourseStatus.Deleted;
}
