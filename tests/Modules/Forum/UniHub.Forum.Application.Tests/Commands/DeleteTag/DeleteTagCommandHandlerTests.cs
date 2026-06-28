using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.Forum.Application.Commands.DeleteTag;
using UniHub.Forum.Domain.Tags;
using Xunit;

namespace UniHub.Forum.Application.Tests.Commands.DeleteTag;

public sealed class DeleteTagCommandHandlerTests
{
    private readonly ITagRepository _tagRepository;
    private readonly DeleteTagCommandHandler _handler;

    public DeleteTagCommandHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new DeleteTagCommandHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldDeleteTag()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "unused-tag", null).Value;
        var command = new DeleteTagCommand(1);
        
        _tagRepository.GetByIdAsync(new TagId(1), Arg.Any<CancellationToken>())
            .Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tagRepository.Received(1).DeleteAsync(tag, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentTag_ShouldReturnFailure()
    {
        // Arrange
        var command = new DeleteTagCommand(999);
        _tagRepository.GetByIdAsync(new TagId(999), Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagErrors.TagNotFound);
        await _tagRepository.DidNotReceive().DeleteAsync(
            Arg.Any<Tag>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTagInUse_ShouldReturnFailure()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "popular-tag", null).Value;
        tag.IncrementUsageCount();
        tag.IncrementUsageCount();
        tag.IncrementUsageCount();
        
        var command = new DeleteTagCommand(1);
        _tagRepository.GetByIdAsync(new TagId(1), Arg.Any<CancellationToken>())
            .Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be(TagErrors.TagInUse.Code);
        result.Error.Message.Should().Contain("3 posts");
        await _tagRepository.DidNotReceive().DeleteAsync(
            Arg.Any<Tag>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTagUsageCountZero_ShouldDeleteTag()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "test-tag", null).Value;
        tag.IncrementUsageCount();
        tag.DecrementUsageCount(); // Back to 0
        
        var command = new DeleteTagCommand(1);
        _tagRepository.GetByIdAsync(new TagId(1), Arg.Any<CancellationToken>())
            .Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tagRepository.Received(1).DeleteAsync(tag, Arg.Any<CancellationToken>());
    }
}
