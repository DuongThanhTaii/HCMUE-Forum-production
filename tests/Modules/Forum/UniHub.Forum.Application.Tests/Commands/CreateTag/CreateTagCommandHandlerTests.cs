using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreateTag;
using UniHub.Forum.Domain.Tags;
using UniHub.SharedKernel.Results;
using Xunit;

namespace UniHub.Forum.Application.Tests.Commands.CreateTag;

public sealed class CreateTagCommandHandlerTests
{
    private readonly ITagRepository _tagRepository;
    private readonly CreateTagCommandHandler _handler;

    public CreateTagCommandHandlerTests()
    {
        _tagRepository = Substitute.For<ITagRepository>();
        _handler = new CreateTagCommandHandler(_tagRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateTag()
    {
        // Arrange
        var command = new CreateTagCommand("csharp", "C# programming language");
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tagRepository.Received(1).AddAsync(
            Arg.Is<Tag>(t => t.Name.Value == command.Name),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithExistingTagName_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateTagCommand("csharp", "C# programming language");
        var existingTag = Tag.Create(new TagId(1), "csharp", null).Value;
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns(existingTag);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(TagErrors.TagAlreadyExists);
        await _tagRepository.DidNotReceive().AddAsync(
            Arg.Any<Tag>(),
            Arg.Any<CancellationToken>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public async Task Handle_WithEmptyName_ShouldReturnFailure(string invalidName)
    {
        // Arrange
        var command = new CreateTagCommand(invalidName, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.Empty");
    }

    [Fact]
    public async Task Handle_WithTooLongName_ShouldReturnFailure()
    {
        // Arrange
        var longName = new string('a', 51);
        var command = new CreateTagCommand(longName, null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.TooLong");
    }

    [Fact]
    public async Task Handle_WithInvalidNameFormat_ShouldReturnFailure()
    {
        // Arrange
        var command = new CreateTagCommand("invalid name with spaces", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagName.InvalidFormat");
    }

    [Fact]
    public async Task Handle_WithNullDescription_ShouldSucceed()
    {
        // Arrange
        var command = new CreateTagCommand("validname", null);
        _tagRepository.GetByNameAsync(command.Name, Arg.Any<CancellationToken>())
            .Returns((Tag?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _tagRepository.Received(1).AddAsync(
            Arg.Is<Tag>(t => t.Description.Value == string.Empty),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithTooLongDescription_ShouldReturnFailure()
    {
        // Arrange
        var longDescription = new string('a', 501);
        var command = new CreateTagCommand("validname", longDescription);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("TagDescription.TooLong");
    }
}
