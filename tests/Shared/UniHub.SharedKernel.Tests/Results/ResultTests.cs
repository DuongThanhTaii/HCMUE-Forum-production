using FluentAssertions;
using UniHub.SharedKernel.Results;

namespace UniHub.SharedKernel.Tests.Results;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_WithValue_ShouldCreateFailedResult()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_OnFailedResult_ShouldThrowException()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");
        var result = Result.Failure<string>(error);

        // Act
        var act = () => result.Value;

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access value of a failed result");
    }

    [Fact]
    public void Constructor_WithSuccessAndError_ShouldThrowException()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error message");

        // Act
        var act = () => new TestResult(true, error);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Success result cannot have an error");
    }

    [Fact]
    public void Constructor_WithFailureAndNoError_ShouldThrowException()
    {
        // Act
        var act = () => new TestResult(false, Error.None);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Failure result must have an error");
    }

    // Helper class to test Result constructor
    private class TestResult : Result
    {
        public TestResult(bool isSuccess, Error error) : base(isSuccess, error)
        {
        }
    }
}
