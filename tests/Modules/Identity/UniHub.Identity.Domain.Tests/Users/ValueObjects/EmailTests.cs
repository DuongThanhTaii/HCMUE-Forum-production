using FluentAssertions;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Domain.Tests.Users.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        // Arrange
        const string emailAddress = "test@example.com";

        // Act
        var result = Email.Create(emailAddress);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be(emailAddress);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyEmail_ShouldFail(string? email)
    {
        // Act
        var result = Email.Create(email!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Empty");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("test@")]
    [InlineData("@example.com")]
    [InlineData("test.example.com")]
    public void Create_WithInvalidFormat_ShouldFail(string email)
    {
        // Act
        var result = Email.Create(email);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.Invalid");
    }

    [Fact]
    public void Create_WithTooLongEmail_ShouldFail()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com";

        // Act
        var result = Email.Create(longEmail);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Email.TooLong");
    }

    [Fact]
    public void Create_ShouldNormalizeEmail()
    {
        // Arrange
        const string email = "  Test@EXAMPLE.COM  ";

        // Act
        var result = Email.Create(email);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Value.Should().Be("test@example.com");
    }

    [Fact]
    public void ImplicitOperator_ShouldConvertToString()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;

        // Act
        string emailString = email;

        // Assert
        emailString.Should().Be("test@example.com");
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = Email.Create("test@example.com").Value;

        // Act
        var result = email.ToString();

        // Assert
        result.Should().Be("test@example.com");
    }

    [Fact]
    public void TwoEmailsWithSameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = Email.Create("test@example.com").Value;
        var email2 = Email.Create("test@example.com").Value;

        // Act & Assert
        email1.Should().Be(email2);
        (email1 == email2).Should().BeTrue();
    }
}