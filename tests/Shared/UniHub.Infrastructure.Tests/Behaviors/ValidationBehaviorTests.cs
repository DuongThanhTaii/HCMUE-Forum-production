using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Moq;
using UniHub.Infrastructure.Behaviors;

namespace UniHub.Infrastructure.Tests.Behaviors;

public class ValidationBehaviorTests
{
    private readonly Mock<RequestHandlerDelegate<string>> _nextMock;

    public ValidationBehaviorTests()
    {
        _nextMock = new Mock<RequestHandlerDelegate<string>>();
        _nextMock.Setup(x => x()).ReturnsAsync("Response");
    }

    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Response");
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        // Arrange
        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest { Value = "test" };

        // Act
        var result = await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        result.Should().Be("Response");
        _nextMock.Verify(x => x(), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldThrowValidationException()
    {
        // Arrange
        var validationFailure = new ValidationFailure("Value", "Value is required");
        var validationResult = new ValidationResult(new[] { validationFailure });

        var validatorMock = new Mock<IValidator<TestRequest>>();
        validatorMock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        var validators = new[] { validatorMock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest { Value = null };

        // Act
        var act = async () => await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UniHub.SharedKernel.Exceptions.ValidationException>();
        _nextMock.Verify(x => x(), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldRunAllValidators()
    {
        // Arrange
        var validator1Mock = new Mock<IValidator<TestRequest>>();
        validator1Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validator2Mock = new Mock<IValidator<TestRequest>>();
        validator2Mock
            .Setup(x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validator1Mock.Object, validator2Mock.Object };
        var behavior = new ValidationBehavior<TestRequest, string>(validators);
        var request = new TestRequest { Value = "test" };

        // Act
        await behavior.Handle(request, _nextMock.Object, CancellationToken.None);

        // Assert
        validator1Mock.Verify(
            x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once);
        validator2Mock.Verify(
            x => x.ValidateAsync(It.IsAny<ValidationContext<TestRequest>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    public class TestRequest
    {
        public string? Value { get; set; }
    }
}
