using UniHub.Learning.Domain.Documents.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Documents.ValueObjects;

public class DocumentTitleTests
{
    [Fact]
    public void Create_WithValidTitle_ShouldReturnSuccess()
    {
        // Arrange
        var title = "Valid Document Title";

        // Act
        var result = DocumentTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnFailure(string? invalidTitle)
    {
        // Act
        var result = DocumentTitle.Create(invalidTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentTitle.Empty");
    }

    [Fact]
    public void Create_WithTooShortTitle_ShouldReturnFailure()
    {
        // Arrange
        var shortTitle = "Hi";

        // Act
        var result = DocumentTitle.Create(shortTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentTitle.TooShort");
    }

    [Fact]
    public void Create_WithTooLongTitle_ShouldReturnFailure()
    {
        // Arrange
        var longTitle = new string('a', DocumentTitle.MaxLength + 1);

        // Act
        var result = DocumentTitle.Create(longTitle);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentTitle.TooLong");
    }

    [Fact]
    public void Create_WithMaxLengthTitle_ShouldReturnSuccess()
    {
        // Arrange
        var maxTitle = new string('a', DocumentTitle.MaxLength);

        // Act
        var result = DocumentTitle.Create(maxTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithMinLengthTitle_ShouldReturnSuccess()
    {
        // Arrange
        var minTitle = new string('a', DocumentTitle.MinLength);

        // Act
        var result = DocumentTitle.Create(minTitle);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var title = "  Valid Title  ";

        // Act
        var result = DocumentTitle.Create(title);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid Title");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var title = "Test Title";
        var documentTitle = DocumentTitle.Create(title).Value;

        // Act
        var result = documentTitle.ToString();

        // Assert
        result.Should().Be(title);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var title = "Test Title";
        var documentTitle = DocumentTitle.Create(title).Value;

        // Act
        string result = documentTitle;

        // Assert
        result.Should().Be(title);
    }
}
