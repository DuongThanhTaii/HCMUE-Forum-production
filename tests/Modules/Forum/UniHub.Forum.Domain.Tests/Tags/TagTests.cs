using FluentAssertions;
using UniHub.Forum.Domain.Tags;
using UniHub.Forum.Domain.Tags.ValueObjects;
using Xunit;

namespace UniHub.Forum.Domain.Tests.Tags;

public sealed class TagTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        var tagId = new TagId(1);
        var name = "csharp";
        var description = "C# programming language";

        // Act
        var result = Tag.Create(tagId, name, description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(tagId);
        result.Value.Name.Value.Should().Be(name);
        result.Value.Description.Value.Should().Be(description);
        result.Value.UsageCount.Should().Be(0);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithNullDescription_ShouldSucceed()
    {
        // Arrange
        var tagId = new TagId(1);
        var name = "dotnet";

        // Act
        var result = Tag.Create(tagId, name, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Description.Value.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithInvalidName_ShouldFail(string invalidName)
    {
        // Arrange
        var tagId = new TagId(1);

        // Act
        var result = Tag.Create(tagId, invalidName, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.Empty");
    }

    [Fact]
    public void Create_WithTooShortName_ShouldFail()
    {
        // Arrange
        var tagId = new TagId(1);
        var name = "c";

        // Act
        var result = Tag.Create(tagId, name, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.TooShort");
    }

    [Fact]
    public void Create_WithTooLongName_ShouldFail()
    {
        // Arrange
        var tagId = new TagId(1);
        var name = new string('a', TagName.MaxLength + 1);

        // Act
        var result = Tag.Create(tagId, name, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.TooLong");
    }

    [Theory]
    [InlineData("c#")]
    [InlineData("c++")]
    [InlineData("tag with spaces")]
    [InlineData("tag@special")]
    public void Create_WithInvalidNameFormat_ShouldFail(string invalidName)
    {
        // Arrange
        var tagId = new TagId(1);

        // Act
        var result = Tag.Create(tagId, invalidName, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.InvalidFormat");
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldFail()
    {
        // Arrange
        var tagId = new TagId(1);
        var name = "valid-name";
        var description = new string('a', TagDescription.MaxLength + 1);

        // Act
        var result = Tag.Create(tagId, name, description);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagDescription.TooLong");
    }

    [Fact]
    public void Update_WithValidData_ShouldSucceed()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "oldname", "Old description").Value;
        var newName = "newname";
        var newDescription = "New description";

        // Act
        var result = tag.Update(newName, newDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tag.Name.Value.Should().Be(newName);
        tag.Description.Value.Should().Be(newDescription);
        tag.UpdatedAt.Should().NotBeNull();
        tag.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_WithInvalidName_ShouldFail(string invalidName)
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "validname", null).Value;

        // Act
        var result = tag.Update(invalidName, null);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.Empty");
    }

    [Fact]
    public void IncrementUsageCount_ShouldIncreaseCount()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "testTag", null).Value;
        var initialCount = tag.UsageCount;

        // Act
        tag.IncrementUsageCount();

        // Assert
        tag.UsageCount.Should().Be(initialCount + 1);
        tag.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void DecrementUsageCount_WhenCountIsPositive_ShouldDecreaseCount()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "testTag", null).Value;
        tag.IncrementUsageCount();
        var currentCount = tag.UsageCount;

        // Act
        tag.DecrementUsageCount();

        // Assert
        tag.UsageCount.Should().Be(currentCount - 1);
        tag.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void DecrementUsageCount_WhenCountIsZero_ShouldNotDecrease()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "testTag", null).Value;

        // Act
        tag.DecrementUsageCount();

        // Assert
        tag.UsageCount.Should().Be(0);
    }

    [Theory]
    [InlineData("csharp", "csharp")]
    [InlineData("dotnet-core", "dotnet-core")]
    [InlineData("web_dev", "webdev")] // Slug removes underscores
    [InlineData("Test123", "test123")]
    public void Create_ShouldGenerateCorrectSlug(string name, string expectedSlugValue)
    {
        // Arrange
        var tagId = new TagId(1);

        // Act
        var result = Tag.Create(tagId, name, null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Slug.Value.Should().Be(expectedSlugValue);
    }

    [Fact]
    public void IncrementUsageCount_MultipleTimes_ShouldAccumulate()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "popular", null).Value;

        // Act
        tag.IncrementUsageCount();
        tag.IncrementUsageCount();
        tag.IncrementUsageCount();

        // Assert
        tag.UsageCount.Should().Be(3);
    }
}
