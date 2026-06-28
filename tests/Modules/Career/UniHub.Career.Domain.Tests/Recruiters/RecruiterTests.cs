using FluentAssertions;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;
using UniHub.Career.Domain.Recruiters.Events;

namespace UniHub.Career.Domain.Tests.Recruiters;

public class RecruiterTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly CompanyId _companyId = CompanyId.CreateUnique();
    private readonly Guid _addedBy = Guid.NewGuid();

    #region Add Factory Method Tests

    [Fact]
    public void Add_WithValidData_ShouldCreateRecruiter()
    {
        // Arrange
        var permissions = RecruiterPermissions.Default();

        // Act
        var result = Recruiter.Add(_userId, _companyId, permissions, _addedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var recruiter = result.Value;
        recruiter.UserId.Should().Be(_userId);
        recruiter.CompanyId.Should().Be(_companyId);
        recruiter.Permissions.Should().Be(permissions);
        recruiter.Status.Should().Be(RecruiterStatus.Active);
        recruiter.AddedBy.Should().Be(_addedBy);
        recruiter.AddedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        recruiter.IsActive().Should().BeTrue();
    }

    [Fact]
    public void Add_ShouldRaiseRecruiterAddedEvent()
    {
        // Arrange
        var permissions = RecruiterPermissions.Default();

        // Act
        var result = Recruiter.Add(_userId, _companyId, permissions, _addedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var recruiter = result.Value;
        var domainEvent = recruiter.DomainEvents.OfType<RecruiterAddedEvent>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.RecruiterId.Should().Be(recruiter.Id);
        domainEvent.UserId.Should().Be(_userId);
        domainEvent.CompanyId.Should().Be(_companyId);
        domainEvent.AddedBy.Should().Be(_addedBy);
        domainEvent.Permissions.Should().Be(permissions);
    }

    [Fact]
    public void Add_WithEmptyUserId_ShouldReturnFailure()
    {
        // Arrange
        var permissions = RecruiterPermissions.Default();

        // Act
        var result = Recruiter.Add(Guid.Empty, _companyId, permissions, _addedBy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Recruiter.InvalidUserId");
    }

    [Fact]
    public void Add_WithNullCompanyId_ShouldReturnFailure()
    {
        // Arrange
        var permissions = RecruiterPermissions.Default();

        // Act
        var result = Recruiter.Add(_userId, null!, permissions, _addedBy);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Recruiter.InvalidCompanyId");
    }

    [Fact]
    public void Add_WithEmptyAddedBy_ShouldReturnFailure()
    {
        // Arrange
        var permissions = RecruiterPermissions.Default();

        // Act
        var result = Recruiter.Add(_userId, _companyId, permissions, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Recruiter.InvalidAddedBy");
    }

    #endregion

    #region UpdatePermissions Tests

    [Fact]
    public void UpdatePermissions_WithValidData_ShouldUpdatePermissions()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        var newPermissions = RecruiterPermissions.Admin();
        var updatedBy = Guid.NewGuid();

        // Act
        var result = recruiter.UpdatePermissions(newPermissions, updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        recruiter.Permissions.Should().Be(newPermissions);
        recruiter.LastModifiedAt.Should().NotBeNull();
        recruiter.LastModifiedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void UpdatePermissions_ShouldRaisePermissionsUpdatedEvent()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.ClearDomainEvents();
        var newPermissions = RecruiterPermissions.Admin();
        var updatedBy = Guid.NewGuid();

        // Act
        var result = recruiter.UpdatePermissions(newPermissions, updatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var domainEvent = recruiter.DomainEvents.OfType<RecruiterPermissionsUpdatedEvent>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.RecruiterId.Should().Be(recruiter.Id);
        domainEvent.UpdatedBy.Should().Be(updatedBy);
        domainEvent.NewPermissions.Should().Be(newPermissions);
    }

    [Fact]
    public void UpdatePermissions_WhenInactive_ShouldReturnFailure()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.Deactivate(Guid.NewGuid());
        var newPermissions = RecruiterPermissions.Admin();

        // Act
        var result = recruiter.UpdatePermissions(newPermissions, Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RecruiterErrors.InactiveRecruiter);
    }

    [Fact]
    public void UpdatePermissions_WithEmptyUpdatedBy_ShouldReturnFailure()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        var newPermissions = RecruiterPermissions.Admin();

        // Act
        var result = recruiter.UpdatePermissions(newPermissions, Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Recruiter.InvalidUpdatedBy");
    }

    #endregion

    #region Deactivate Tests

    [Fact]
    public void Deactivate_WithValidData_ShouldDeactivateRecruiter()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        var deactivatedBy = Guid.NewGuid();

        // Act
        var result = recruiter.Deactivate(deactivatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        recruiter.Status.Should().Be(RecruiterStatus.Inactive);
        recruiter.IsActive().Should().BeFalse();
        recruiter.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_ShouldRaiseDeactivatedEvent()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.ClearDomainEvents();
        var deactivatedBy = Guid.NewGuid();

        // Act
        var result = recruiter.Deactivate(deactivatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var domainEvent = recruiter.DomainEvents.OfType<RecruiterDeactivatedEvent>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.RecruiterId.Should().Be(recruiter.Id);
        domainEvent.DeactivatedBy.Should().Be(deactivatedBy);
    }

    [Fact]
    public void Deactivate_WhenRecruiterTryingToDeactivateSelf_ShouldReturnFailure()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;

        // Act
        var result = recruiter.Deactivate(_userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RecruiterErrors.CannotDeactivateSelf);
        recruiter.Status.Should().Be(RecruiterStatus.Active);
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldReturnFailure()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.Deactivate(Guid.NewGuid());

        // Act
        var result = recruiter.Deactivate(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RecruiterErrors.AlreadyInactive);
    }

    [Fact]
    public void Deactivate_WithEmptyDeactivatedBy_ShouldReturnFailure()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;

        // Act
        var result = recruiter.Deactivate(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Recruiter.InvalidDeactivatedBy");
    }

    #endregion

    #region Reactivate Tests

    [Fact]
    public void Reactivate_WithValidData_ShouldReactivateRecruiter()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.Deactivate(Guid.NewGuid());
        var reactivatedBy = Guid.NewGuid();

        // Act
        var result = recruiter.Reactivate(reactivatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        recruiter.Status.Should().Be(RecruiterStatus.Active);
        recruiter.IsActive().Should().BeTrue();
        recruiter.LastModifiedAt.Should().NotBeNull();
    }

    [Fact]
    public void Reactivate_ShouldRaiseReactivatedEvent()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.Deactivate(Guid.NewGuid());
        recruiter.ClearDomainEvents();
        var reactivatedBy = Guid.NewGuid();

        // Act
        var result = recruiter.Reactivate(reactivatedBy);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var domainEvent = recruiter.DomainEvents.OfType<RecruiterReactivatedEvent>().FirstOrDefault();
        domainEvent.Should().NotBeNull();
        domainEvent!.RecruiterId.Should().Be(recruiter.Id);
        domainEvent.ReactivatedBy.Should().Be(reactivatedBy);
    }

    [Fact]
    public void Reactivate_WhenAlreadyActive_ShouldReturnFailure()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;

        // Act
        var result = recruiter.Reactivate(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(RecruiterErrors.AlreadyActive);
    }

    [Fact]
    public void Reactivate_WithEmptyReactivatedBy_ShouldReturnFailure()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.Deactivate(Guid.NewGuid());

        // Act
        var result = recruiter.Reactivate(Guid.Empty);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Recruiter.InvalidReactivatedBy");
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActive_WhenActive_ShouldReturnTrue()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;

        // Act & Assert
        recruiter.IsActive().Should().BeTrue();
    }

    [Fact]
    public void IsActive_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;
        recruiter.Deactivate(Guid.NewGuid());

        // Act & Assert
        recruiter.IsActive().Should().BeFalse();
    }

    #endregion

    #region HasPermission Tests

    [Fact]
    public void HasPermission_WhenActiveAndHasPermission_ShouldReturnTrue()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Admin(), _addedBy).Value;

        // Act & Assert
        recruiter.HasPermission(p => p.CanInviteRecruiters).Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WhenActiveButLacksPermission_ShouldReturnFalse()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Default(), _addedBy).Value;

        // Act & Assert
        recruiter.HasPermission(p => p.CanInviteRecruiters).Should().BeFalse();
    }

    [Fact]
    public void HasPermission_WhenInactive_ShouldReturnFalse()
    {
        // Arrange
        var recruiter = Recruiter.Add(_userId, _companyId, RecruiterPermissions.Admin(), _addedBy).Value;
        recruiter.Deactivate(Guid.NewGuid());

        // Act & Assert
        recruiter.HasPermission(p => p.CanInviteRecruiters).Should().BeFalse();
    }

    #endregion
}
