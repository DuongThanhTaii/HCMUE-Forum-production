using FluentAssertions;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Domain.Tests.Users.ValueObjects;

public class UserProfileTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        // Arrange
        const string firstName = "John";
        const string lastName = "Doe";

        // Act
        var result = UserProfile.Create(firstName, lastName);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be(firstName);
        result.Value.LastName.Should().Be(lastName);
        result.Value.FullName.Should().Be("John Doe");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyFirstName_ShouldFail(string? firstName)
    {
        // Act
        var result = UserProfile.Create(firstName!, "Doe");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserProfile.FirstName.Empty");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Create_WithEmptyLastName_ShouldFail(string? lastName)
    {
        // Act
        var result = UserProfile.Create("John", lastName!);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserProfile.LastName.Empty");
    }

    [Fact]
    public void Create_WithTooLongFirstName_ShouldFail()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var result = UserProfile.Create(longName, "Doe");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserProfile.FirstName.TooLong");
    }

    [Fact]
    public void Create_WithTooLongLastName_ShouldFail()
    {
        // Arrange
        var longName = new string('A', 101);

        // Act
        var result = UserProfile.Create("John", longName);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserProfile.LastName.TooLong");
    }

    [Fact]
    public void Create_WithTooLongBio_ShouldFail()
    {
        // Arrange
        var longBio = new string('A', 501);

        // Act
        var result = UserProfile.Create("John", "Doe", bio: longBio);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserProfile.Bio.TooLong");
    }

    [Theory]
    [InlineData("123")]
    [InlineData("abc")]
    [InlineData("123-abc")]
    public void Create_WithInvalidPhone_ShouldFail(string phone)
    {
        // Act
        var result = UserProfile.Create("John", "Doe", phone: phone);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserProfile.Phone.Invalid");
    }

    [Theory]
    [InlineData("12345678")]
    [InlineData("123456789012345")]
    [InlineData("(123) 456-7890")]
    [InlineData("123-456-7890")]
    public void Create_WithValidPhone_ShouldSucceed(string phone)
    {
        // Act
        var result = UserProfile.Create("John", "Doe", phone: phone);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_WithTooYoungAge_ShouldFail()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-12);

        // Act
        var result = UserProfile.Create("John", "Doe", dateOfBirth: dateOfBirth);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UserProfile.DateOfBirth.TooYoung");
    }

    [Fact]
    public void Create_WithValidAge_ShouldSucceed()
    {
        // Arrange
        var dateOfBirth = DateTime.UtcNow.AddYears(-20);

        // Act
        var result = UserProfile.Create("John", "Doe", dateOfBirth: dateOfBirth);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DateOfBirth.Should().Be(dateOfBirth);
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        // Act
        var result = UserProfile.Create("  John  ", "  Doe  ", bio: "  Bio  ");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.FirstName.Should().Be("John");
        result.Value.LastName.Should().Be("Doe");
        result.Value.Bio.Should().Be("Bio");
    }

    [Fact]
    public void TwoProfilesWithSameData_ShouldBeEqual()
    {
        // Arrange
        var profile1 = UserProfile.Create("John", "Doe").Value;
        var profile2 = UserProfile.Create("John", "Doe").Value;

        // Act & Assert
        profile1.Should().Be(profile2);
    }
}