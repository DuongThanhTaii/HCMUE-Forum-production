using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.Forum.Application.Commands.UpdateTag;
using UniHub.Forum.Domain.Tags;
using Xunit;

namespace UniHub.Forum.Application.Tests.Commands.UpdateTag;

public sealed class UpdateTagCommandHandlerTests
{
    private readonly ITagRepository _tagRepository;
    private readonly UpdateTagCommandHandler _handler;

    public UpdateTagCommandHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new UpdateTagCommandHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldUpdateTag()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "oldname", "Old description").Value;
        var command = new UpdateTagCommand(1, "newname", "New description");
        
        _tagRepository.GetByIdAsync(new TagId(command.TagId), Arg.Any<CancellationToken>())
            .Returns(tag);
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tag.Name.Value.Should().Be("newname");
        tag.Description.Value.Should().Be("New description");
        await _tagRepository.Received(1).UpdateAsync(tag, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentTag_ShouldReturnFailure()
    {
        // Arrange
        var command = new UpdateTagCommand(999, "newname", null);
        _tagRepository.GetByIdAsync(new TagId(command.TagId), Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagErrors.TagNotFound);
        await _tagRepository.DidNotReceive().UpdateAsync(
            Arg.Any<Tag>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateName_ShouldReturnFailure()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "oldname", null).Value;
        var otherTag = Tag.Create(new TagId(2), "existingname", null).Value;
        var command = new UpdateTagCommand(1, "existingname", null);
        
        _tagRepository.GetByIdAsync(new TagId(1), Arg.Any<CancellationToken>())
            .Returns(tag);
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(otherTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagErrors.TagAlreadyExists);
        await _tagRepository.DidNotReceive().UpdateAsync(
            Arg.Any<Tag>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSameName_ShouldSucceed()
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "samename", "Old description").Value;
        var command = new UpdateTagCommand(1, "samename", "New description");
        
        _tagRepository.GetByIdAsync(new TagId(1), Arg.Any<CancellationToken>())
            .Returns(tag);
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(tag); // Same tag

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        tag.Description.Value.Should().Be("New description");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handle_WithInvalidName_ShouldReturnFailure(string invalidName)
    {
        // Arrange
        var tag = Tag.Create(new TagId(1), "validname", null).Value;
        var command = new UpdateTagCommand(1, invalidName, null);
        _tagRepository.GetByIdAsync(new TagId(1), Arg.Any<CancellationToken>())
            .Returns(tag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.Empty");
    }
}
