using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.AddComment;
using UniHub.Forum.Application.Commands.VoteComment;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;

namespace UniHub.Forum.Application.Tests.Commands.VoteComment;

public class VoteCommentCommandHandlerTests
{
    private readonly ICommentRepository _commentRepository;
    private readonly VoteCommentCommandHandler _handler;

    public VoteCommentCommandHandlerTests()
    {
        _commentRepository = Substitute.For<ICommentRepository>();
        _handler = new VoteCommentCommandHandler(_commentRepository);
    }

    [Fact]
    public async Task Handle_WithNoExistingVote_ShouldAddVote()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var voteType = VoteType.Upvote;

        var comment = CreateTestComment(commentId);
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new VoteCommentCommand(commentId, userId, voteType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Votes.Should().ContainSingle();
        comment.Votes.First().UserId.Should().Be(userId);
        comment.Votes.First().Type.Should().Be(voteType);
        comment.VoteScore.Should().Be(1);
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSameVoteType_ShouldRemoveVote()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var voteType = VoteType.Upvote;

        var comment = CreateTestComment(commentId);
        comment.AddVote(userId, voteType); // Add initial vote

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new VoteCommentCommand(commentId, userId, voteType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Votes.Should().BeEmpty();
        comment.VoteScore.Should().Be(0);
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDifferentVoteType_ShouldChangeVote()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = CreateTestComment(commentId);
        comment.AddVote(userId, VoteType.Upvote); // Add initial upvote

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new VoteCommentCommand(commentId, userId, VoteType.Downvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Votes.Should().ContainSingle();
        comment.Votes.First().Type.Should().Be(VoteType.Downvote);
        comment.VoteScore.Should().Be(-1);
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentComment_ShouldReturnFailure()
    {
        // Arrange
        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns((Comment?)null);

        var command = new VoteCommentCommand(Guid.NewGuid(), Guid.NewGuid(), VoteType.Upvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(CommentErrors.CommentNotFound);
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDeletedComment_ShouldReturnFailure()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = CreateTestComment(commentId);
        comment.Delete(); // Mark as deleted

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new VoteCommentCommand(commentId, userId, VoteType.Upvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
        await _commentRepository.DidNotReceive().UpdateAsync(Arg.Any<Comment>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleVotes_ShouldCalculateScoreCorrectly()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user3Id = Guid.NewGuid();

        var comment = CreateTestComment(commentId);
        comment.AddVote(user1Id, VoteType.Upvote);
        comment.AddVote(user2Id, VoteType.Upvote);

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new VoteCommentCommand(commentId, user3Id, VoteType.Downvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Votes.Should().HaveCount(3);
        comment.VoteScore.Should().Be(1); // 2 upvotes + 1 downvote = 2 - 1 = 1
        await _commentRepository.Received(1).UpdateAsync(comment, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUpvoteDownvoteToggle_ShouldUpdateScoreCorrectly()
    {
        // Arrange
        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = CreateTestComment(commentId);
        comment.AddVote(userId, VoteType.Downvote); // Initial downvote (score: -1)

        _commentRepository.GetByIdAsync(Arg.Any<CommentId>(), Arg.Any<CancellationToken>())
            .Returns(comment);

        var command = new VoteCommentCommand(commentId, userId, VoteType.Upvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.VoteScore.Should().Be(1); // Changed from -1 to +1
        comment.Votes.First().Type.Should().Be(VoteType.Upvote);
    }

    private static Comment CreateTestComment(Guid commentId)
    {
        var content = CommentContent.Create("Test comment content").Value;
        var comment = Comment.Create(new PostId(Guid.NewGuid()), Guid.NewGuid(), content, null).Value;
        typeof(Comment).GetProperty(nameof(Comment.Id))!.SetValue(comment, new CommentId(commentId));
        return comment;
    }
}
