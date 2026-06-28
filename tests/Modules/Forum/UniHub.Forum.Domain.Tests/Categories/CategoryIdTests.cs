using UniHub.Forum.Domain.Categories;

namespace UniHub.Forum.Domain.Tests.Categories;

public class CategoryIdTests
{
    [Fact]
    public void CreateUnique_ShouldGenerateUniqueId()
    {
        // Act
        var categoryId1 = CategoryId.CreateUnique();
        var categoryId2 = CategoryId.CreateUnique();

        // Assert
        categoryId1.Should().NotBe(categoryId2);
        categoryId1.Value.Should().NotBeEmpty();
        categoryId2.Value.Should().NotBeEmpty();
    }

    [Fact]
    public void Create_WithValidGuid_ShouldCreateCategoryId()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var categoryId = CategoryId.Create(guid);

        // Assert
        categoryId.Value.Should().Be(guid);
    }

    [Fact]
    public void ToString_ShouldReturnGuidString()
    {
        // Arrange
        var guid = Guid.NewGuid();
        var categoryId = CategoryId.Create(guid);

        // Act
        var result = categoryId.ToString();

        // Assert
        result.Should().Be(guid.ToString());
    }

    [Fact]
    public void TwoCategoryIdsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var guid = Guid.NewGuid();

        // Act
        var categoryId1 = CategoryId.Create(guid);
        var categoryId2 = CategoryId.Create(guid);

        // Assert
        categoryId1.Should().Be(categoryId2);
    }

    [Fact]
    public void TwoCategoryIdsWithDifferentValues_ShouldNotBeEqual()
    {
        // Act
        var categoryId1 = CategoryId.CreateUnique();
        var categoryId2 = CategoryId.CreateUnique();

        // Assert
        categoryId1.Should().NotBe(categoryId2);
    }
}
