using UniHub.Forum.Domain.Posts.ValueObjects;

namespace UniHub.Forum.Domain.Tests.Posts.ValueObjects;

public class SlugTests
{
    [Fact]
    public void Create_WithValidSlug_ShouldReturnSuccess()
    {
        // Arrange
        var slugValue = "valid-slug";

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyOrWhitespaceSlug_ShouldReturnFailure(string? slugValue)
    {
        // Act
        var result = Slug.Create(slugValue!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Slug.Empty");
    }

    [Fact]
    public void Create_ShouldConvertToLowercase()
    {
        // Arrange
        var slugValue = "UPPERCASE SLUG";

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("uppercase-slug");
    }

    [Fact]
    public void Create_ShouldReplaceSpacesWithHyphens()
    {
        // Arrange
        var slugValue = "slug with spaces";

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("slug-with-spaces");
    }

    [Fact]
    public void Create_ShouldRemoveSpecialCharacters()
    {
        // Arrange
        var slugValue = "slug!@#$%^&*()with[]{}special|\\characters";

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("slugwithspecialcharacters");
    }

    [Fact]
    public void Create_ShouldRemoveMultipleHyphens()
    {
        // Arrange
        var slugValue = "slug---with---multiple---hyphens";

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("slug-with-multiple-hyphens");
    }

    [Fact]
    public void Create_ShouldTrimHyphens()
    {
        // Arrange
        var slugValue = "-slug-with-hyphens-";

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("slug-with-hyphens");
    }

    [Fact]
    public void Create_WithSlugTooLong_ShouldReturnFailure()
    {
        // Arrange
        var slugValue = new string('a', Slug.MaxLength + 1);

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Slug.TooLong");
    }

    [Fact]
    public void CreateFromTitle_ShouldGenerateValidSlug()
    {
        // Arrange
        var title = PostTitle.Create("This Is A Valid Post Title!").Value;

        // Act
        var result = Slug.CreateFromTitle(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("this-is-a-valid-post-title");
    }

    [Fact]
    public void Create_WithVietnameseCharacters_ShouldRemoveAccents()
    {
        // Arrange
        var slugValue = "tiếng việt có dấu";

        // Act
        var result = Slug.Create(slugValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("tieng-viet-co-dau");
    }
}
