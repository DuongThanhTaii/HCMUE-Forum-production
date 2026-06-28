using UniHub.Learning.Domain.Documents.ValueObjects;

namespace UniHub.Learning.Domain.Tests.Documents.ValueObjects;

public class DocumentDescriptionTests
{
    [Fact]
    public void Create_WithValidDescription_ShouldReturnSuccess()
    {
        // Arrange
        var description = "This is a valid document description.";

        // Act
        var result = DocumentDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespace_ShouldReturnEmptyDescription(string? emptyDescription)
    {
        // Act
        var result = DocumentDescription.Create(emptyDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().BeEmpty();
    }

    [Fact]
    public void Create_WithTooLongDescription_ShouldReturnFailure()
    {
        // Arrange
        var longDescription = new string('a', DocumentDescription.MaxLength + 1);

        // Act
        var result = DocumentDescription.Create(longDescription);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("DocumentDescription.TooLong");
    }

    [Fact]
    public void Create_WithMaxLengthDescription_ShouldReturnSuccess()
    {
        // Arrange
        var maxDescription = new string('a', DocumentDescription.MaxLength);

        // Act
        var result = DocumentDescription.Create(maxDescription);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithLeadingAndTrailingSpaces_ShouldTrimSpaces()
    {
        // Arrange
        var description = "  Valid Description  ";

        // Act
        var result = DocumentDescription.Create(description);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("Valid Description");
    }

    [Fact]
    public void ToString_ShouldReturnValue()
    {
        // Arrange
        var description = "Test Description";
        var documentDescription = DocumentDescription.Create(description).Value;

        // Act
        var result = documentDescription.ToString();

        // Assert
        result.Should().Be(description);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldWork()
    {
        // Arrange
        var description = "Test Description";
        var documentDescription = DocumentDescription.Create(description).Value;

        // Act
        string result = documentDescription;

        // Assert
        result.Should().Be(description);
    }
}
