using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.ReportPost;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Tests.Commands.ReportPost;

public class ReportPostCommandHandlerTests
{
    private readonly IReportRepository _reportRepository;
    private readonly IPostRepository _postRepository;
    private readonly ReportPostCommandHandler _handler;

    public ReportPostCommandHandlerTests()
    {
        _reportRepository = Substitute.For<IReportRepository>();
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new ReportPostCommandHandler(_reportRepository, _postRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateReport()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var command = new ReportPostCommand(
            PostId: postId,
            ReporterId: reporterId,
            Reason: ReportReason.Spam,
            Description: "This is spam");

        var post = CreateValidPost();
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _reportRepository.GetByReporterAndItemAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<ReportedItemType>(),
            Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _reportRepository.Received(1).AddAsync(
            Arg.Is<Report>(r =>
                r.ReportedItemId == command.PostId &&
                r.ReportedItemType == ReportedItemType.Post &&
                r.ReporterId == command.ReporterId &&
                r.Reason == command.Reason &&
                r.Description == command.Description),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldFail()
    {
        // Arrange
        var command = new ReportPostCommand(
            PostId: Guid.NewGuid(),
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.Spam,
            Description: null);

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.PostNotFound);
        await _reportRepository.DidNotReceive().AddAsync(
            Arg.Any<Report>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateReport_ShouldFail()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var command = new ReportPostCommand(
            PostId: postId,
            ReporterId: reporterId,
            Reason: ReportReason.Spam,
            Description: null);

        var post = CreateValidPost();
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var existingReport = Report.Create(
            new ReportId(1),
            command.PostId,
            ReportedItemType.Post,
            command.ReporterId,
            command.Reason,
            null).Value;

        _reportRepository.GetByReporterAndItemAsync(
            command.ReporterId,
            command.PostId,
            ReportedItemType.Post,
            Arg.Any<CancellationToken>())
            .Returns(existingReport);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.DuplicateReport);
        await _reportRepository.DidNotReceive().AddAsync(
            Arg.Any<Report>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithEmptyPostId_ShouldFail()
    {
        // Arrange
        var command = new ReportPostCommand(
            PostId: Guid.Empty,
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.Spam,
            Description: null);

        var post = CreateValidPost();
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _reportRepository.GetByReporterAndItemAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<ReportedItemType>(),
            Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReportedItemId);
    }

    [Fact]
    public async Task Handle_WithEmptyReporterId_ShouldFail()
    {
        // Arrange
        var command = new ReportPostCommand(
            PostId: Guid.NewGuid(),
            ReporterId: Guid.Empty,
            Reason: ReportReason.Spam,
            Description: null);

        var post = CreateValidPost();
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _reportRepository.GetByReporterAndItemAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<ReportedItemType>(),
            Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.InvalidReporterId);
    }

    [Fact]
    public async Task Handle_WithMultipleUsersReportingSamePost_ShouldSucceed()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var command1 = new ReportPostCommand(
            PostId: postId,
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.Spam,
            Description: "Spam content");

        var command2 = new ReportPostCommand(
            PostId: postId,
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.InappropriateContent,
            Description: "Inappropriate");

        var post = CreateValidPost();
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _reportRepository.GetByReporterAndItemAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<ReportedItemType>(),
            Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.IsSuccess.Should().BeTrue();
        result2.IsSuccess.Should().BeTrue();
        await _reportRepository.Received(2).AddAsync(
            Arg.Any<Report>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDifferentReasons_ShouldCreateReportWithCorrectReason()
    {
        // Arrange
        var reasons = new[]
        {
            ReportReason.Spam,
            ReportReason.Harassment,
            ReportReason.InappropriateContent,
            ReportReason.Misinformation,
            ReportReason.OffTopic,
            ReportReason.CopyrightViolation,
            ReportReason.Other
        };

        var post = CreateValidPost();
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        _reportRepository.GetByReporterAndItemAsync(
            Arg.Any<Guid>(),
            Arg.Any<Guid>(),
            Arg.Any<ReportedItemType>(),
            Arg.Any<CancellationToken>())
            .Returns((Report?)null);

        // Act & Assert
        foreach (var reason in reasons)
        {
            var command = new ReportPostCommand(
                PostId: Guid.NewGuid(),
                ReporterId: Guid.NewGuid(),
                Reason: reason,
                Description: null);

            var result = await _handler.Handle(command, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            await _reportRepository.Received().AddAsync(
                Arg.Is<Report>(r => r.Reason == reason),
                Arg.Any<CancellationToken>());

            _reportRepository.ClearReceivedCalls();
        }
    }

    private static Post CreateValidPost()
    {
        var result = Post.Create(
            PostTitle.Create("Test Post").Value,
            PostContent.Create("Test content for the post").Value,
            PostType.Discussion,
            Guid.NewGuid(),
            Guid.NewGuid());

        return result.Value;
    }
}
