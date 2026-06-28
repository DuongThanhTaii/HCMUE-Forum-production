using FluentAssertions;
using UniHub.SharedKernel.Results;

namespace UniHub.SharedKernel.Tests.Results;

public class ResultExtensionsTests
{
    [Fact]
    public void Map_OnSuccessResult_ShouldMapValue()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var mappedResult = result.Map(x => x * 2);

        // Assert
        mappedResult.IsSuccess.Should().BeTrue();
        mappedResult.Value.Should().Be(10);
    }

    [Fact]
    public void Map_OnFailureResult_ShouldReturnFailure()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error");
        var result = Result.Failure<int>(error);

        // Act
        var mappedResult = result.Map(x => x * 2);

        // Assert
        mappedResult.IsFailure.Should().BeTrue();
        mappedResult.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_OnSuccessResult_ShouldBindValue()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var boundResult = result.Bind(x => Result.Success(x.ToString()));

        // Assert
        boundResult.IsSuccess.Should().BeTrue();
        boundResult.Value.Should().Be("5");
    }

    [Fact]
    public void Bind_OnFailureResult_ShouldReturnFailure()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error");
        var result = Result.Failure<int>(error);

        // Act
        var boundResult = result.Bind(x => Result.Success(x.ToString()));

        // Assert
        boundResult.IsFailure.Should().BeTrue();
        boundResult.Error.Should().Be(error);
    }

    [Fact]
    public void Bind_OnSuccessResultReturningFailure_ShouldReturnFailure()
    {
        // Arrange
        var result = Result.Success(5);
        var error = new Error("BIND_ERROR", "Bind error");

        // Act
        var boundResult = result.Bind<int, string>(_ => Result.Failure<string>(error));

        // Assert
        boundResult.IsFailure.Should().BeTrue();
        boundResult.Error.Should().Be(error);
    }

    [Fact]
    public void Match_OnSuccessResult_ShouldCallSuccessHandler()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var output = result.Match(
            onSuccess: x => $"Success: {x}",
            onFailure: error => $"Failure: {error.Message}");

        // Assert
        output.Should().Be("Success: 5");
    }

    [Fact]
    public void Match_OnFailureResult_ShouldCallFailureHandler()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error");
        var result = Result.Failure<int>(error);

        // Act
        var output = result.Match(
            onSuccess: x => $"Success: {x}",
            onFailure: error => $"Failure: {error.Message}");

        // Assert
        output.Should().Be("Failure: Test error");
    }

    [Fact]
    public void GetValueOrDefault_OnSuccessResult_ShouldReturnValue()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var value = result.GetValueOrDefault(10);

        // Assert
        value.Should().Be(5);
    }

    [Fact]
    public void GetValueOrDefault_OnFailureResult_ShouldReturnDefaultValue()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error");
        var result = Result.Failure<int>(error);

        // Act
        var value = result.GetValueOrDefault(10);

        // Assert
        value.Should().Be(10);
    }

    [Fact]
    public void GetValueOrDefault_OnFailureResultWithoutDefault_ShouldReturnTypeDefault()
    {
        // Arrange
        var error = new Error("TEST_ERROR", "Test error");
        var result = Result.Failure<int>(error);

        // Act
        var value = result.GetValueOrDefault();

        // Assert
        value.Should().Be(0);
    }

    [Fact]
    public void ChainedOperations_ShouldWorkCorrectly()
    {
        // Arrange
        var result = Result.Success(5);

        // Act
        var finalResult = result
            .Map(x => x * 2)
            .Bind(x => Result.Success(x + 10))
            .Map(x => x.ToString());

        // Assert
        finalResult.IsSuccess.Should().BeTrue();
        finalResult.Value.Should().Be("20");
    }
}
