using UniHub.Learning.Domain.Faculties;
using UniHub.Learning.Domain.Faculties.Events;
using UniHub.Learning.Domain.Faculties.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Faculties;

public class FacultyTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid ManagerId = Guid.NewGuid();

    #region Creation Tests

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var code = FacultyCode.Create("CNTT").Value;
        var name = FacultyName.Create("Khoa Công nghệ Thông tin").Value;
        var description = FacultyDescription.Create("Faculty description").Value;

        // Act
        var result = Faculty.Create(code, name, description, UserId, ManagerId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var faculty = result.Value;
        faculty.Code.Should().Be(code);
        faculty.Name.Should().Be(name);
        faculty.Description.Should().Be(description);
        faculty.CreatedBy.Should().Be(UserId);
        faculty.ManagerId.Should().Be(ManagerId);
        faculty.Status.Should().Be(FacultyStatus.Active);
        faculty.CourseCount.Should().Be(0);
    }

    [Fact]
    public void Create_WithoutManager_ShouldReturnSuccess()
    {
        // Arrange
        var code = FacultyCode.Create("TOAN").Value;
        var name = FacultyName.Create("Khoa Toán").Value;
        var description = FacultyDescription.Create("Faculty description").Value;

        // Act
        var result = Faculty.Create(code, name, description, UserId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ManagerId.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldRaiseFacultyCreatedEvent()
    {
        // Arrange
        var code = FacultyCode.Create("LY").Value;
        var name = FacultyName.Create("Khoa Vật Lý").Value;
        var description = FacultyDescription.Create("Faculty description").Value;

        // Act
        var faculty = Faculty.Create(code, name, description, UserId).Value;

        // Assert
        var domainEvent = faculty.DomainEvents.OfType<FacultyCreatedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FacultyId.Should().Be(faculty.Id.Value);
        domainEvent.FacultyCode.Should().Be(code);
        domainEvent.FacultyName.Should().Be(name);
        domainEvent.CreatedBy.Should().Be(UserId);
    }

    [Fact]
    public void Create_WithEmptyCreatedBy_ShouldReturnFailure()
    {
        // Arrange
        var code = FacultyCode.Create("HOA").Value;
        var name = FacultyName.Create("Khoa Hóa học").Value;
        var description = FacultyDescription.Create("Faculty description").Value;

        // Act
        var result = Faculty.Create(code, name, description, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidCreator");
    }

    [Fact]
    public void Create_WithEmptyManagerId_ShouldReturnFailure()
    {
        // Arrange
        var code = FacultyCode.Create("SINH").Value;
        var name = FacultyName.Create("Khoa Sinh học").Value;
        var description = FacultyDescription.Create("Faculty description").Value;

        // Act
        var result = Faculty.Create(code, name, description, UserId, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidManager");
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithValidData_ShouldUpdateAndRaiseEvent()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var newName = FacultyName.Create("Khoa CNTT Mới").Value;
        var newDescription = FacultyDescription.Create("New description").Value;

        // Act
        var result = faculty.Update(newName, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        faculty.Name.Should().Be(newName);
        faculty.Description.Should().Be(newDescription);

        var domainEvent = faculty.DomainEvents.OfType<FacultyUpdatedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FacultyId.Should().Be(faculty.Id.Value);
    }

    [Fact]
    public void Update_DeletedFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Delete(UserId);
        var newName = FacultyName.Create("New Name").Value;
        var newDescription = FacultyDescription.Create("New description").Value;

        // Act
        var result = faculty.Update(newName, newDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.Deleted");
    }

    #endregion

    #region Manager Management Tests

    [Fact]
    public void AssignManager_WithValidData_ShouldAssignManagerAndRaiseEvent()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var managerId = Guid.NewGuid();
        var assignedBy = Guid.NewGuid();

        // Act
        var result = faculty.AssignManager(managerId, assignedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        faculty.ManagerId.Should().Be(managerId);
        faculty.IsManager(managerId).Should().BeTrue();

        var domainEvent = faculty.DomainEvents.OfType<ManagerAssignedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FacultyId.Should().Be(faculty.Id.Value);
        domainEvent.ManagerId.Should().Be(managerId);
        domainEvent.AssignedBy.Should().Be(assignedBy);
    }

    [Fact]
    public void AssignManager_WithEmptyManagerId_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        var result = faculty.AssignManager(Guid.Empty, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidManager");
    }

    [Fact]
    public void AssignManager_WithEmptyAssignedBy_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var managerId = Guid.NewGuid();

        // Act
        var result = faculty.AssignManager(managerId, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidAssigner");
    }

    [Fact]
    public void AssignManager_ToDeletedFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Delete(UserId);
        var managerId = Guid.NewGuid();

        // Act
        var result = faculty.AssignManager(managerId, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.Deleted");
    }

    [Fact]
    public void AssignManager_SameManagerTwice_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var managerId = Guid.NewGuid();
        faculty.AssignManager(managerId, UserId);

        // Act
        var result = faculty.AssignManager(managerId, UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.ManagerAlreadyAssigned");
    }

    [Fact]
    public void RemoveManager_WithValidData_ShouldRemoveManagerAndRaiseEvent()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var managerId = Guid.NewGuid();
        var removedBy = Guid.NewGuid();
        faculty.AssignManager(managerId, UserId);

        // Act
        var result = faculty.RemoveManager(removedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        faculty.ManagerId.Should().BeNull();
        faculty.IsManager(managerId).Should().BeFalse();

        var domainEvent = faculty.DomainEvents.OfType<ManagerRemovedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FacultyId.Should().Be(faculty.Id.Value);
        domainEvent.ManagerId.Should().Be(managerId);
        domainEvent.RemovedBy.Should().Be(removedBy);
    }

    [Fact]
    public void RemoveManager_WithEmptyRemovedBy_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        var result = faculty.RemoveManager(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidRemover");
    }

    [Fact]
    public void RemoveManager_FromDeletedFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var managerId = Guid.NewGuid();
        faculty.AssignManager(managerId, UserId);
        faculty.Delete(UserId);

        // Act
        var result = faculty.RemoveManager(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.Deleted");
    }

    [Fact]
    public void RemoveManager_WhenNoManager_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        var result = faculty.RemoveManager(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.NoManager");
    }

    [Fact]
    public void IsManager_WithNonManagerUser_ShouldReturnFalse()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var userId = Guid.NewGuid();

        // Act
        var result = faculty.IsManager(userId);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public void Deactivate_ActiveFaculty_ShouldDeactivateAndRaiseEvent()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var deactivatedBy = Guid.NewGuid();

        // Act
        var result = faculty.Deactivate(deactivatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        faculty.Status.Should().Be(FacultyStatus.Inactive);
        faculty.IsInactive().Should().BeTrue();

        var domainEvent = faculty.DomainEvents.OfType<FacultyDeactivatedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FacultyId.Should().Be(faculty.Id.Value);
        domainEvent.DeactivatedBy.Should().Be(deactivatedBy);
    }

    [Fact]
    public void Deactivate_WithEmptyDeactivatedBy_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        var result = faculty.Deactivate(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidDeactivator");
    }

    [Fact]
    public void Deactivate_DeletedFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Delete(UserId);

        // Act
        var result = faculty.Deactivate(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.Deleted");
    }

    [Fact]
    public void Deactivate_AlreadyInactiveFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Deactivate(UserId);

        // Act
        var result = faculty.Deactivate(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.AlreadyInactive");
    }

    [Fact]
    public void Activate_InactiveFaculty_ShouldActivateAndRaiseEvent()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Deactivate(UserId);
        var activatedBy = Guid.NewGuid();

        // Act
        var result = faculty.Activate(activatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        faculty.Status.Should().Be(FacultyStatus.Active);
        faculty.IsActive().Should().BeTrue();

        var domainEvent = faculty.DomainEvents.OfType<FacultyActivatedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FacultyId.Should().Be(faculty.Id.Value);
        domainEvent.ActivatedBy.Should().Be(activatedBy);
    }

    [Fact]
    public void Activate_WithEmptyActivatedBy_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Deactivate(UserId);

        // Act
        var result = faculty.Activate(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidActivator");
    }

    [Fact]
    public void Activate_DeletedFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Delete(UserId);

        // Act
        var result = faculty.Activate(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.Deleted");
    }

    [Fact]
    public void Activate_AlreadyActiveFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        var result = faculty.Activate(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.AlreadyActive");
    }

    [Fact]
    public void Delete_ActiveFaculty_ShouldDeleteAndRaiseEvent()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        var deletedBy = Guid.NewGuid();

        // Act
        var result = faculty.Delete(deletedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        faculty.Status.Should().Be(FacultyStatus.Deleted);
        faculty.IsDeleted().Should().BeTrue();

        var domainEvent = faculty.DomainEvents.OfType<FacultyDeletedEvent>().SingleOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.FacultyId.Should().Be(faculty.Id.Value);
        domainEvent.DeletedBy.Should().Be(deletedBy);
    }

    [Fact]
    public void Delete_WithEmptyDeletedBy_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        var result = faculty.Delete(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.InvalidDeleter");
    }

    [Fact]
    public void Delete_AlreadyDeletedFaculty_ShouldReturnFailure()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.Delete(UserId);

        // Act
        var result = faculty.Delete(UserId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Faculty.AlreadyDeleted");
    }

    #endregion

    #region Course Counter Tests

    [Fact]
    public void IncrementCourseCount_ShouldIncreaseCount()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        faculty.IncrementCourseCount();
        faculty.IncrementCourseCount();
        faculty.IncrementCourseCount();

        // Assert
        faculty.CourseCount.Should().Be(3);
    }

    [Fact]
    public void DecrementCourseCount_ShouldDecreaseCount()
    {
        // Arrange
        var faculty = CreateValidFaculty();
        faculty.IncrementCourseCount();
        faculty.IncrementCourseCount();

        // Act
        faculty.DecrementCourseCount();

        // Assert
        faculty.CourseCount.Should().Be(1);
    }

    [Fact]
    public void DecrementCourseCount_FromZero_ShouldStayAtZero()
    {
        // Arrange
        var faculty = CreateValidFaculty();

        // Act
        faculty.DecrementCourseCount();

        // Assert
        faculty.CourseCount.Should().Be(0);
    }

    #endregion

    #region Helper Methods

    private static Faculty CreateValidFaculty()
    {
        var code = FacultyCode.Create("CNTT").Value;
        var name = FacultyName.Create("Khoa Công nghệ Thông tin").Value;
        var description = FacultyDescription.Create("Faculty description").Value;
        
        var faculty = Faculty.Create(code, name, description, UserId).Value;
        faculty.ClearDomainEvents(); // Clear creation events for cleaner test assertions
        return faculty;
    }

    #endregion
}
