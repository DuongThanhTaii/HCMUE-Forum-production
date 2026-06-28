using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Courses.Events;
using UniHub.Learning.Domain.Courses.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Courses;

public class CourseTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid FacultyId = Guid.NewGuid();

    #region Creation Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var code = CourseCode.Create("CS101").Value;
        var name = CourseName.Create("Introduction to CS").Value;
        var description = CourseDescription.Create("Course description").Value;
        var semester = Semester.Create("2024-2025 HK1").Value;
        var credits = 3;

        // Act
        var result = Course.Create(code, name, description, semester, credits, UserId, FacultyId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var course = result.Value;
        course.Code.Should().Be(code);
        course.Name.Should().Be(name);
        course.Description.Should().Be(description);
        course.Semester.Should().Be(semester);
        course.Credits.Should().Be(credits);
        course.CreatedBy.Should().Be(UserId);
        course.FacultyId.Should().Be(FacultyId);
        course.Status.Should().Be(CourseStatus.Active);
        course.ModeratorIds.Should().BeEmpty();
        course.DocumentCount.Should().Be(0);
        course.EnrollmentCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithoutFaculty_ShouldReturnSuccess()
    {
        // Arrange
        var code = CourseCode.Create("CS101").Value;
        var name = CourseName.Create("Introduction to CS").Value;
        var description = CourseDescription.Create("Course description").Value;
        var semester = Semester.Create("2024-2025 HK1").Value;
        var credits = 3;

        // Act
        var result = Course.Create(code, name, description, semester, credits, UserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FacultyId.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseCourseCreatedEvent()
    {
        // Arrange
        var code = CourseCode.Create("CS101").Value;
        var name = CourseName.Create("Introduction to CS").Value;
        var description = CourseDescription.Create("Course description").Value;
        var semester = Semester.Create("2024-2025 HK1").Value;
        var credits = 3;

        // Act
        var course = Course.Create(code, name, description, semester, credits, UserId).Value;

        // Assert
        var domainEvent = course.DomainEvents.OfType<CourseCreatedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.CourseId.Should().Be(course.Id.Value);
        domainEvent.CourseCode.Should().Be(code);
        domainEvent.CourseName.Should().Be(name);
        domainEvent.CreatedBy.Should().Be(UserId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Create_WithInvalidCredits_ShouldReturnFailure(int invalidCredits)
    {
        // Arrange
        var code = CourseCode.Create("CS101").Value;
        var name = CourseName.Create("Introduction to CS").Value;
        var description = CourseDescription.Create("Course description").Value;
        var semester = Semester.Create("2024-2025 HK1").Value;

        // Act
        var result = Course.Create(code, name, description, semester, invalidCredits, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidCredits");
    }

    [Fact]
    public void Create_WithEmptyCreatedBy_ShouldReturnFailure()
    {
        // Arrange
        var code = CourseCode.Create("CS101").Value;
        var name = CourseName.Create("Introduction to CS").Value;
        var description = CourseDescription.Create("Course description").Value;
        var semester = Semester.Create("2024-2025 HK1").Value;

        // Act
        var result = Course.Create(code, name, description, semester, 3, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidCreator");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateAndRaiseEvent()
    {
        // Arrange
        var course = CreateValidCourse();
        var newName = CourseName.Create("Advanced CS").Value;
        var newDescription = CourseDescription.Create("Advanced course").Value;
        var newSemester = Semester.Create("2024-2025 HK2").Value;
        var newCredits = 4;

        // Act
        var result = course.Update(newName, newDescription, newSemester, newCredits, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.Name.Should().Be(newName);
        course.Description.Should().Be(newDescription);
        course.Semester.Should().Be(newSemester);
        course.Credits.Should().Be(newCredits);

        var domainEvent = course.DomainEvents.OfType<CourseUpdatedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.CourseId.Should().Be(course.Id.Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    public void Update_WithInvalidCredits_ShouldReturnFailure(int invalidCredits)
    {
        // Arrange
        var course = CreateValidCourse();
        var newName = CourseName.Create("Advanced CS").Value;
        var newDescription = CourseDescription.Create("Advanced course").Value;
        var newSemester = Semester.Create("2024-2025 HK2").Value;

        // Act
        var result = course.Update(newName, newDescription, newSemester, invalidCredits, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidCredits");
    }



    [Fact]
    public void Update_DeletedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Delete(UserId);
        var newName = CourseName.Create("Advanced CS").Value;
        var newDescription = CourseDescription.Create("Advanced course").Value;
        var newSemester = Semester.Create("2024-2025 HK2").Value;

        // Act
        var result = course.Update(newName, newDescription, newSemester, 3, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.Deleted");
    }

    #endregion

    #region Moderator Management Tests

    [Fact]
    public void AssignModerator_WithValidData_ShouldAddModeratorAndRaiseEvent()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderatorId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        // Act
        var result = course.AssignModerator(moderatorId, assignedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.ModeratorIds.Should().ContainSingle().Which.Should().Be(moderatorId);
        course.IsModerator(moderatorId).Should().BeTrue();

        var domainEvent = course.DomainEvents.OfType<ModeratorAssignedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.CourseId.Should().Be(course.Id.Value);
        domainEvent.ModeratorId.Should().Be(moderatorId);
        domainEvent.AssignedBy.Should().Be(assignedBy);
    }

    [Fact]
    public void AssignModerator_MultipleModerators_ShouldAddAll()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderator1 = Guid.NewGuid();
        var moderator2 = Guid.NewGuid();
        var moderator3 = Guid.NewGuid();

        // Act
        course.AssignModerator(moderator1, UserId);
        course.AssignModerator(moderator2, UserId);
        course.AssignModerator(moderator3, UserId);

        // Assert
        course.ModeratorIds.Should().HaveCount(3);
        course.IsModerator(moderator1).Should().BeTrue();
        course.IsModerator(moderator2).Should().BeTrue();
        course.IsModerator(moderator3).Should().BeTrue();
    }

    [Fact]
    public void AssignModerator_WithEmptyModeratorId_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var result = course.AssignModerator(Guid.Empty, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidModerator");
    }

    [Fact]
    public void AssignModerator_WithEmptyAssignedBy_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderatorId = Guid.NewGuid();

        // Act
        var result = course.AssignModerator(moderatorId, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidAssigner");
    }

    [Fact]
    public void AssignModerator_ToDeletedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Delete(UserId);
        var moderatorId = Guid.NewGuid();

        // Act
        var result = course.AssignModerator(moderatorId, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.Deleted");
    }

    [Fact]
    public void AssignModerator_DuplicateModerator_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderatorId = Guid.NewGuid();
        course.AssignModerator(moderatorId, UserId);

        // Act
        var result = course.AssignModerator(moderatorId, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.ModeratorAlreadyAssigned");
    }

    [Fact]
    public void RemoveModerator_WithValidData_ShouldRemoveModeratorAndRaiseEvent()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderatorId = Guid.NewGuid();
        var removedBy = Guid.NewGuid();
        course.AssignModerator(moderatorId, UserId);

        // Act
        var result = course.RemoveModerator(moderatorId, removedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.ModeratorIds.Should().BeEmpty();
        course.IsModerator(moderatorId).Should().BeFalse();

        var domainEvent = course.DomainEvents.OfType<ModeratorRemovedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.CourseId.Should().Be(course.Id.Value);
        domainEvent.ModeratorId.Should().Be(moderatorId);
        domainEvent.RemovedBy.Should().Be(removedBy);
    }

    [Fact]
    public void RemoveModerator_WithEmptyModeratorId_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var result = course.RemoveModerator(Guid.Empty, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidModerator");
    }

    [Fact]
    public void RemoveModerator_WithEmptyRemovedBy_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderatorId = Guid.NewGuid();

        // Act
        var result = course.RemoveModerator(moderatorId, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidRemover");
    }

    [Fact]
    public void RemoveModerator_FromDeletedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderatorId = Guid.NewGuid();
        course.AssignModerator(moderatorId, UserId);
        course.Delete(UserId);

        // Act
        var result = course.RemoveModerator(moderatorId, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.Deleted");
    }

    [Fact]
    public void RemoveModerator_NotFound_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        var moderatorId = Guid.NewGuid();

        // Act
        var result = course.RemoveModerator(moderatorId, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.ModeratorNotFound");
    }

    [Fact]
    public void IsModerator_WithNonModeratorUser_ShouldReturnFalse()
    {
        // Arrange
        var course = CreateValidCourse();
        var userId = Guid.NewGuid();

        // Act
        var result = course.IsModerator(userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public void Archive_ActiveCourse_ShouldArchiveAndRaiseEvent()
    {
        // Arrange
        var course = CreateValidCourse();
        var archivedBy = Guid.NewGuid();

        // Act
        var result = course.Archive(archivedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.Status.Should().Be(CourseStatus.Archived);
        course.IsArchived().Should().BeTrue();

        var domainEvent = course.DomainEvents.OfType<CourseArchivedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.CourseId.Should().Be(course.Id.Value);
        domainEvent.ArchivedBy.Should().Be(archivedBy);
    }

    [Fact]
    public void Archive_WithEmptyArchivedBy_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var result = course.Archive(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidArchiver");
    }

    [Fact]
    public void Archive_CompletedCourse_ShouldSucceed()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Complete();

        // Act
        var result = course.Archive(UserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.Status.Should().Be(CourseStatus.Archived);
    }

    [Fact]
    public void Archive_AlreadyArchivedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Archive(UserId);

        // Act
        var result = course.Archive(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.AlreadyArchived");
    }

    [Fact]
    public void Archive_DeletedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Delete(UserId);

        // Act
        var result = course.Archive(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.Deleted");
    }

    [Fact]
    public void Activate_ArchivedCourse_ShouldActivateAndRaiseEvent()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Archive(UserId);
        var activatedBy = Guid.NewGuid();

        // Act
        var result = course.Activate(activatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.Status.Should().Be(CourseStatus.Active);
        course.IsActive().Should().BeTrue();

        var domainEvent = course.DomainEvents.OfType<CourseActivatedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.CourseId.Should().Be(course.Id.Value);
        domainEvent.ActivatedBy.Should().Be(activatedBy);
    }

    [Fact]
    public void Activate_WithEmptyActivatedBy_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Archive(UserId);

        // Act
        var result = course.Activate(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidActivator");
    }

    [Fact]
    public void Activate_CompletedCourse_ShouldSucceed()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Complete();

        // Act
        var result = course.Activate(UserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.Status.Should().Be(CourseStatus.Active);
    }

    [Fact]
    public void Activate_AlreadyActiveCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var result = course.Activate(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.AlreadyActive");
    }

    [Fact]
    public void Activate_DeletedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Delete(UserId);

        // Act
        var result = course.Activate(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.Deleted");
    }

    [Fact]
    public void Complete_ActiveCourse_ShouldCompleteSuccessfully()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var result = course.Complete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.Status.Should().Be(CourseStatus.Completed);
        course.IsCompleted().Should().BeTrue();
    }



    [Fact]
    public void Complete_DeletedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Delete(UserId);

        // Act
        var result = course.Complete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.Deleted");
    }

    [Fact]
    public void Delete_ActiveCourse_ShouldDeleteAndRaiseEvent()
    {
        // Arrange
        var course = CreateValidCourse();
        var deletedBy = Guid.NewGuid();

        // Act
        var result = course.Delete(deletedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        course.Status.Should().Be(CourseStatus.Deleted);
        course.IsDeleted().Should().BeTrue();

        var domainEvent = course.DomainEvents.OfType<CourseDeletedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.CourseId.Should().Be(course.Id.Value);
        domainEvent.DeletedBy.Should().Be(deletedBy);
    }

    [Fact]
    public void Delete_WithEmptyDeletedBy_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        var result = course.Delete(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.InvalidDeleter");
    }

    [Fact]
    public void Delete_AlreadyDeletedCourse_ShouldReturnFailure()
    {
        // Arrange
        var course = CreateValidCourse();
        course.Delete(UserId);

        // Act
        var result = course.Delete(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Course.AlreadyDeleted");
    }

    #endregion

    #region Counter Tests

    [Fact]
    public void IncrementDocumentCount_ShouldIncreaseCount()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        course.IncrementDocumentCount();
        course.IncrementDocumentCount();
        course.IncrementDocumentCount();

        // Assert
        course.DocumentCount.Should().Be(3);
    }

    [Fact]
    public void DecrementDocumentCount_ShouldDecreaseCount()
    {
        // Arrange
        var course = CreateValidCourse();
        course.IncrementDocumentCount();
        course.IncrementDocumentCount();

        // Act
        course.DecrementDocumentCount();

        // Assert
        course.DocumentCount.Should().Be(1);
    }

    [Fact]
    public void DecrementDocumentCount_FromZero_ShouldStayAtZero()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        course.DecrementDocumentCount();

        // Assert
        course.DocumentCount.Should().Be(0);
    }

    [Fact]
    public void IncrementEnrollmentCount_ShouldIncreaseCount()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        course.IncrementEnrollmentCount();
        course.IncrementEnrollmentCount();

        // Assert
        course.EnrollmentCount.Should().Be(2);
    }

    [Fact]
    public void DecrementEnrollmentCount_ShouldDecreaseCount()
    {
        // Arrange
        var course = CreateValidCourse();
        course.IncrementEnrollmentCount();
        course.IncrementEnrollmentCount();
        course.IncrementEnrollmentCount();

        // Act
        course.DecrementEnrollmentCount();

        // Assert
        course.EnrollmentCount.Should().Be(2);
    }

    [Fact]
    public void DecrementEnrollmentCount_FromZero_ShouldStayAtZero()
    {
        // Arrange
        var course = CreateValidCourse();

        // Act
        course.DecrementEnrollmentCount();

        // Assert
        course.EnrollmentCount.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static Course CreateValidCourse()
    {
        var code = CourseCode.Create("CS101").Value;
        var name = CourseName.Create("Introduction to CS").Value;
        var description = CourseDescription.Create("Course description").Value;
        var semester = Semester.Create("2024-2025 HK1").Value;
        
        var course = Course.Create(code, name, description, semester, 3, UserId, FacultyId).Value;
        course.ClearDomainEvents(); // Clear creation events for cleaner test assertions
        return course;
    }

    #endregion
}
