using FluentAssertions;
using MediatR;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.SharedKernel.Tests.CQRS;

public class CommandTests
{
    [Fact]
    public void ICommand_ShouldExtendIRequest()
    {
        // Assert
        typeof(ICommand).Should().BeAssignableTo<IRequest<Result>>();
    }

    [Fact]
    public void ICommandWithResponse_ShouldExtendIRequest()
    {
        // Assert
        typeof(ICommand<string>).Should().BeAssignableTo<IRequest<Result<string>>>();
    }

    [Fact]
    public async Task CommandHandler_ShouldHandleCommand()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand { Value = 42 };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task CommandHandlerWithResponse_ShouldReturnValue()
    {
        // Arrange
        var handler = new TestCommandWithResponseHandler();
        var command = new TestCommandWithResponse { Value = 42 };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("42");
    }

    [Fact]
    public async Task CommandHandler_OnFailure_ShouldReturnError()
    {
        // Arrange
        var handler = new TestCommandHandler();
        var command = new TestCommand { Value = -1 }; // Invalid value

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_VALUE");
    }

    // Test command without response
    private record TestCommand : ICommand
    {
        public int Value { get; init; }
    }

    // Test command handler without response
    private class TestCommandHandler : ICommandHandler<TestCommand>
    {
        public Task<Result> Handle(TestCommand request, CancellationToken cancellationToken)
        {
            if (request.Value < 0)
            {
                return Task.FromResult(Result.Failure(
                    new Error("INVALID_VALUE", "Value must be non-negative")));
            }

            return Task.FromResult(Result.Success());
        }
    }

    // Test command with response
    private record TestCommandWithResponse : ICommand<string>
    {
        public int Value { get; init; }
    }

    // Test command handler with response
    private class TestCommandWithResponseHandler : ICommandHandler<TestCommandWithResponse, string>
    {
        public Task<Result<string>> Handle(TestCommandWithResponse request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success(request.Value.ToString()));
        }
    }
}
