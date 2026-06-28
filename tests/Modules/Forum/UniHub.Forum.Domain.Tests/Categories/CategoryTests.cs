using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Categories.ValueObjects;

namespace UniHub.Forum.Domain.Tests.Categories;

public class CategoryTests
{
    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;

        // Act
        var result = Category.Create(name, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be(name);
        result.Value.Description.Should().Be(description);
        result.Value.Slug.Value.Should().Be("technology");
        result.Value.ParentCategoryId.Should().BeNull();
        result.Value.PostCount.Should().Be(0);
        result.Value.IsActive.Should().BeTrue();
        result.Value.ModeratorIds.Should().BeEmpty();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithParentCategory_ShouldSetParentCategoryId()
    {
        // Arrange
        var name = CategoryName.Create("Programming").Value;
        var description = CategoryDescription.Create("Programming topics").Value;
        var parentCategoryId = CategoryId.CreateUnique();

        // Act
        var result = Category.Create(name, description, parentCategoryId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentCategoryId.Should().Be(parentCategoryId);
    }

    [Fact]
    public void Create_WithDisplayOrder_ShouldSetDisplayOrder()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var displayOrder = 5;

        // Act
        var result = Category.Create(name, description, displayOrder: displayOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DisplayOrder.Should().Be(displayOrder);
    }

    [Fact]
    public void Update_WithValidData_ShouldUpdateCategory()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;

        var newName = CategoryName.Create("Software").Value;
        var newDescription = CategoryDescription.Create("Software development").Value;
        var newDisplayOrder = 10;

        // Act
        var result = category.Update(newName, newDescription, newDisplayOrder);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.Name.Should().Be(newName);
        category.Description.Should().Be(newDescription);
        category.Slug.Value.Should().Be("software");
        category.DisplayOrder.Should().Be(newDisplayOrder);
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_OnInactiveCategory_ShouldActivateCategory()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        category.Deactivate();

        // Act
        var result = category.Activate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeTrue();
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Activate_OnAlreadyActiveCategory_ShouldReturnFailure()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;

        // Act
        var result = category.Activate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.AlreadyActive");
    }

    [Fact]
    public void Deactivate_OnActiveCategory_ShouldDeactivateCategory()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;

        // Act
        var result = category.Deactivate();

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.IsActive.Should().BeFalse();
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Deactivate_OnAlreadyInactiveCategory_ShouldReturnFailure()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        category.Deactivate();

        // Act
        var result = category.Deactivate();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.AlreadyInactive");
    }

    [Fact]
    public void AssignModerator_WithNewModerator_ShouldAddModerator()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        var moderatorId = Guid.NewGuid();

        // Act
        var result = category.AssignModerator(moderatorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.ModeratorIds.Should().Contain(moderatorId);
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AssignModerator_WithExistingModerator_ShouldReturnFailure()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        var moderatorId = Guid.NewGuid();
        category.AssignModerator(moderatorId);

        // Act
        var result = category.AssignModerator(moderatorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.ModeratorAlreadyAssigned");
    }

    [Fact]
    public void RemoveModerator_WithExistingModerator_ShouldRemoveModerator()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        var moderatorId = Guid.NewGuid();
        category.AssignModerator(moderatorId);

        // Act
        var result = category.RemoveModerator(moderatorId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        category.ModeratorIds.Should().NotContain(moderatorId);
        category.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void RemoveModerator_WithNonExistingModerator_ShouldReturnFailure()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        var moderatorId = Guid.NewGuid();

        // Act
        var result = category.RemoveModerator(moderatorId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Category.ModeratorNotAssigned");
    }

    [Fact]
    public void IncrementPostCount_ShouldIncreasePostCount()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;

        // Act
        category.IncrementPostCount();
        category.IncrementPostCount();

        // Assert
        category.PostCount.Should().Be(2);
    }

    [Fact]
    public void DecrementPostCount_ShouldDecreasePostCount()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        category.IncrementPostCount();
        category.IncrementPostCount();

        // Act
        category.DecrementPostCount();

        // Assert
        category.PostCount.Should().Be(1);
    }

    [Fact]
    public void DecrementPostCount_WhenZero_ShouldStayAtZero()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;

        // Act
        category.DecrementPostCount();

        // Assert
        category.PostCount.Should().Be(0);
    }

    [Fact]
    public void AssignMultipleModerators_ShouldAddAllModerators()
    {
        // Arrange
        var name = CategoryName.Create("Technology").Value;
        var description = CategoryDescription.Create("Tech discussions").Value;
        var category = Category.Create(name, description).Value;
        var moderatorId1 = Guid.NewGuid();
        var moderatorId2 = Guid.NewGuid();
        var moderatorId3 = Guid.NewGuid();

        // Act
        category.AssignModerator(moderatorId1);
        category.AssignModerator(moderatorId2);
        category.AssignModerator(moderatorId3);

        // Assert
        category.ModeratorIds.Should().HaveCount(3);
        category.ModeratorIds.Should().Contain(new[] { moderatorId1, moderatorId2, moderatorId3 });
    }
}
