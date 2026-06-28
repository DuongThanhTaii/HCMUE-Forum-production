using FluentAssertions;
using UniHub.Career.Domain.Applications;

namespace UniHub.Career.Domain.Tests.Applications;

public class CoverLetterTests
{
    [Fact]
    public void Create_WithValidContent_ShouldReturnSuccess()
    {
        var content = "I am writing to express my strong interest in this position. " +
                      "With my background in software development and passion for technology, " +
                      "I believe I would be a great fit for your team.";

        var result = CoverLetter.Create(content);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().Be(content);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    [InlineData(null)]
    public void Create_WithEmptyContent_ShouldReturnFailure(string? content)
    {
        var result = CoverLetter.Create(content!);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CoverLetter.Empty");
    }

    [Fact]
    public void Create_WithContentTooShort_ShouldReturnFailure()
    {
        var shortContent = "Too short";

        var result = CoverLetter.Create(shortContent);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CoverLetter.TooShort");
    }

    [Fact]
    public void Create_WithContentTooLong_ShouldReturnFailure()
    {
        var longContent = new string('A', CoverLetter.MaxContentLength + 1);

        var result = CoverLetter.Create(longContent);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("CoverLetter.TooLong");
    }

    [Fact]
    public void Create_TrimsWhitespace()
    {
        var content = "  " + new string('A', CoverLetter.MinContentLength) + "  ";

        var result = CoverLetter.Create(content);

        result.IsSuccess.Should().BeTrue();
        result.Value.Content.Should().NotStartWith(" ");
        result.Value.Content.Should().NotEndWith(" ");
    }

    [Fact]
    public void CreateOptional_WithNullContent_ShouldReturnNull()
    {
        var result = CoverLetter.CreateOptional(null);

        result.Should().BeNull();
    }

    [Fact]
    public void CreateOptional_WithValidContent_ShouldReturnCoverLetter()
    {
        var content = new string('A', CoverLetter.MinContentLength);

        var result = CoverLetter.CreateOptional(content);

        result.Should().NotBeNull();
        result!.Content.Should().Be(content);
    }

    [Fact]
    public void CreateOptional_WithInvalidContent_ShouldReturnNull()
    {
        var invalidContent = "Too short";

        var result = CoverLetter.CreateOptional(invalidContent);

        result.Should().BeNull();
    }

    [Fact]
    public void Equality_WithSameContent_ShouldBeEqual()
    {
        var content = new string('A', CoverLetter.MinContentLength);
        var letter1 = CoverLetter.Create(content).Value;
        var letter2 = CoverLetter.Create(content).Value;

        letter1.Should().Be(letter2);
    }

    [Fact]
    public void Equality_WithDifferentContent_ShouldNotBeEqual()
    {
        var letter1 = CoverLetter.Create(new string('A', CoverLetter.MinContentLength)).Value;
        var letter2 = CoverLetter.Create(new string('B', CoverLetter.MinContentLength)).Value;

        letter1.Should().NotBe(letter2);
    }
}
