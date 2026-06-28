using FluentAssertions;
using MediatR;
using UniHub.SharedKernel.CQRS;
using UniHub.SharedKernel.Results;

namespace UniHub.SharedKernel.Tests.CQRS;

public class QueryTests
{
    [Fact]
    public void IQuery_ShouldExtendIRequest()
    {
        // Assert
        typeof(IQuery<string>).Should().BeAssignableTo<IRequest<Result<string>>>();
    }

    [Fact]
    public async Task QueryHandler_ShouldReturnValue()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Id = 123 };

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("Entity_123");
    }

    [Fact]
    public async Task QueryHandler_OnNotFound_ShouldReturnError()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Id = -1 }; // Not found

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("NOT_FOUND");
    }

    [Fact]
    public async Task QueryHandler_WithCancellation_ShouldRespectToken()
    {
        // Arrange
        var handler = new TestQueryHandler();
        var query = new TestQuery { Id = 123 };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act
        var act = async () => await handler.Handle(query, cts.Token);

        // Assert
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // Test query
    private record TestQuery : IQuery<string>
    {
        public int Id { get; init; }
    }

    // Test query handler
    private class TestQueryHandler : IQueryHandler<TestQuery, string>
    {
        public Task<Result<string>> Handle(TestQuery request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (request.Id < 0)
            {
                return Task.FromResult(Result.Failure<string>(
                    new Error("NOT_FOUND", "Entity not found")));
            }

            return Task.FromResult(Result.Success($"Entity_{request.Id}"));
        }
    }
}
