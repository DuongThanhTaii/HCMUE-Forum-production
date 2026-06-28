using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.ReportComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Application.Tests.Commands.ReportComment;

public class ReportCommentCommandHandlerTests
{
    private readonly IReportRepository _reportRepository;
    private readonly ICommentRepository _commentRepository;
    private readonly ReportCommentCommandHandler _handler;

    public ReportCommentCommandHandlerTests()
    {
        _reportRepository = Substitute.For<IReportRepository>();
        _commentRepository = Substitute.For<ICommentRepository>();
        _handler = new ReportCommentCommandHandler(_reportRepository, _commentRepository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateReport()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var command = new ReportCommentCommand(
            CommentId: commentId,
            ReporterId: reporterId,
            Reason: ReportReason.Harassment,
            Description: "Harassing comment");

        var comment = CreateValidComment();
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

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
                r.ReportedItemId == command.CommentId &&
                r.ReportedItemType == ReportedItemType.Comment &&
                r.ReporterId == command.ReporterId &&
                r.Reason == command.Reason &&
                r.Description == command.Description),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentComment_ShouldFail()
    {
        // Arrange
        var command = new ReportCommentCommand(
            CommentId: Guid.NewGuid(),
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.Spam,
            Description: null);

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReportErrors.CommentNotFound);
        await _reportRepository.DidNotReceive().AddAsync(
            Arg.Any<Report>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDuplicateReport_ShouldFail()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var reporterId = Guid.NewGuid();
        var command = new ReportCommentCommand(
            CommentId: commentId,
            ReporterId: reporterId,
            Reason: ReportReason.Spam,
            Description: null);

        var comment = CreateValidComment();
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var existingReport = Report.Create(
            new ReportId(1),
            command.CommentId,
            ReportedItemType.Comment,
            command.ReporterId,
            command.Reason,
            null).Value;

        _reportRepository.GetByReporterAndItemAsync(
            command.ReporterId,
            command.CommentId,
            ReportedItemType.Comment,
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
    public async Task Handle_WithEmptyCommentId_ShouldFail()
    {
        // Arrange
        var command = new ReportCommentCommand(
            CommentId: Guid.Empty,
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.Spam,
            Description: null);

        var comment = CreateValidComment();
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

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
        var command = new ReportCommentCommand(
            CommentId: Guid.NewGuid(),
            ReporterId: Guid.Empty,
            Reason: ReportReason.Spam,
            Description: null);

        var comment = CreateValidComment();
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

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
    public async Task Handle_WithMultipleUsersReportingSameComment_ShouldSucceed()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var command1 = new ReportCommentCommand(
            CommentId: commentId,
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.Harassment,
            Description: "Harassment");

        var command2 = new ReportCommentCommand(
            CommentId: commentId,
            ReporterId: Guid.NewGuid(),
            Reason: ReportReason.InappropriateContent,
            Description: "Inappropriate");

        var comment = CreateValidComment();
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

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

    private static Comment CreateValidComment()
    {
        var result = Comment.Create(
            new PostId(Guid.NewGuid()),
            Guid.NewGuid(),
            CommentContent.Create("This is a test comment").Value,
            null);

        return result.Value;
    }
}
