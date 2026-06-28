using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;

namespace UniHub.Forum.Domain.Tests.Comments;

public class CommentTests
{
    private readonly PostId _postId = PostId.CreateUnique();
    private readonly Guid _authorId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldReturnSuccess()
    {
        // Arrange
        var content = CommentContent.Create("This is a valid comment").Value;

        // Act
        var result = Comment.Create(_postId, _authorId, content);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.PostId.Should().Be(_postId);
        result.Value.AuthorId.Should().Be(_authorId);
        result.Value.Content.Should().Be(content);
        result.Value.ParentCommentId.Should().BeNull();
        result.Value.IsAcceptedAnswer.Should().BeFalse();
        result.Value.VoteScore.Should().Be(0);
        result.Value.IsDeleted.Should().BeFalse();
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Create_WithParentComment_ShouldSetParentCommentId()
    {
        // Arrange
        var content = CommentContent.Create("This is a reply").Value;
        var parentCommentId = CommentId.CreateUnique();

        // Act
        var result = Comment.Create(_postId, _authorId, content, parentCommentId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.ParentCommentId.Should().Be(parentCommentId);
    }

    [Fact]
    public void Update_WithValidContent_ShouldUpdateComment()
    {
        // Arrange
        var originalContent = CommentContent.Create("Original comment").Value;
        var comment = Comment.Create(_postId, _authorId, originalContent).Value;
        var newContent = CommentContent.Create("Updated comment").Value;

        // Act
        var result = comment.Update(newContent);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.Content.Should().Be(newContent);
        comment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Update_OnDeletedComment_ShouldReturnFailure()
    {
        // Arrange
        var content = CommentContent.Create("Original comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        comment.Delete();
        var newContent = CommentContent.Create("Updated comment").Value;

        // Act
        var result = comment.Update(newContent);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
    }

    [Fact]
    public void Delete_OnExistingComment_ShouldDeleteComment()
    {
        // Arrange
        var content = CommentContent.Create("Comment to delete").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;

        // Act
        var result = comment.Delete();

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.IsDeleted.Should().BeTrue();
        comment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void Delete_OnAlreadyDeletedComment_ShouldReturnFailure()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        comment.Delete();

        // Act
        var result = comment.Delete();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.AlreadyDeleted");
    }

    [Fact]
    public void AcceptAsAnswer_OnValidComment_ShouldAcceptComment()
    {
        // Arrange
        var content = CommentContent.Create("Answer comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;

        // Act
        var result = comment.AcceptAsAnswer();

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.IsAcceptedAnswer.Should().BeTrue();
        comment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void AcceptAsAnswer_OnDeletedComment_ShouldReturnFailure()
    {
        // Arrange
        var content = CommentContent.Create("Answer comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        comment.Delete();

        // Act
        var result = comment.AcceptAsAnswer();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
    }

    [Fact]
    public void AcceptAsAnswer_OnAlreadyAcceptedComment_ShouldReturnFailure()
    {
        // Arrange
        var content = CommentContent.Create("Answer comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        comment.AcceptAsAnswer();

        // Act
        var result = comment.AcceptAsAnswer();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.AlreadyAccepted");
    }

    [Fact]
    public void AcceptAsAnswer_OnNestedComment_ShouldReturnFailure()
    {
        // Arrange
        var content = CommentContent.Create("Nested answer").Value;
        var parentCommentId = CommentId.CreateUnique();
        var comment = Comment.Create(_postId, _authorId, content, parentCommentId).Value;

        // Act
        var result = comment.AcceptAsAnswer();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NestedComment");
    }

    [Fact]
    public void UnacceptAsAnswer_OnAcceptedComment_ShouldUnacceptComment()
    {
        // Arrange
        var content = CommentContent.Create("Answer comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        comment.AcceptAsAnswer();

        // Act
        var result = comment.UnacceptAsAnswer();

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.IsAcceptedAnswer.Should().BeFalse();
        comment.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public void UnacceptAsAnswer_OnNotAcceptedComment_ShouldReturnFailure()
    {
        // Arrange
        var content = CommentContent.Create("Answer comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;

        // Act
        var result = comment.UnacceptAsAnswer();

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.NotAccepted");
    }

    #region Voting Tests

    [Fact]
    public void AddVote_WithUpvote_ShouldIncreaseVoteScore()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();

        // Act
        var result = comment.AddVote(userId, VoteType.Upvote);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.VoteScore.Should().Be(1);
        comment.Votes.Should().HaveCount(1);
        comment.Votes.First().UserId.Should().Be(userId);
        comment.Votes.First().Type.Should().Be(VoteType.Upvote);
    }

    [Fact]
    public void AddVote_WithDownvote_ShouldDecreaseVoteScore()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();

        // Act
        var result = comment.AddVote(userId, VoteType.Downvote);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.VoteScore.Should().Be(-1);
        comment.Votes.Should().HaveCount(1);
        comment.Votes.First().Type.Should().Be(VoteType.Downvote);
    }

    [Fact]
    public void AddVote_MultipleUsers_ShouldAccumulateVoteScore()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;

        // Act
        comment.AddVote(Guid.NewGuid(), VoteType.Upvote);
        comment.AddVote(Guid.NewGuid(), VoteType.Upvote);
        comment.AddVote(Guid.NewGuid(), VoteType.Downvote);

        // Assert
        comment.VoteScore.Should().Be(1); // 2 upvotes - 1 downvote = 1
        comment.Votes.Should().HaveCount(3);
    }

    [Fact]
    public void AddVote_WhenUserAlreadyVoted_ShouldFail()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();
        comment.AddVote(userId, VoteType.Upvote);

        // Act
        var result = comment.AddVote(userId, VoteType.Downvote);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.VoteAlreadyExists");
        comment.VoteScore.Should().Be(1);
        comment.Votes.Should().HaveCount(1);
    }

    [Fact]
    public void AddVote_OnDeletedComment_ShouldFail()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        comment.Delete();

        // Act
        var result = comment.AddVote(Guid.NewGuid(), VoteType.Upvote);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
    }

    [Fact]
    public void ChangeVote_FromUpvoteToDownvote_ShouldUpdateVoteScore()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();
        comment.AddVote(userId, VoteType.Upvote);

        // Act
        var result = comment.ChangeVote(userId, VoteType.Downvote);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.VoteScore.Should().Be(-1); // Changed from +1 to -1
        comment.Votes.Should().HaveCount(1);
        comment.Votes.First().Type.Should().Be(VoteType.Downvote);
    }

    [Fact]
    public void ChangeVote_FromDownvoteToUpvote_ShouldUpdateVoteScore()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();
        comment.AddVote(userId, VoteType.Downvote);

        // Act
        var result = comment.ChangeVote(userId, VoteType.Upvote);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.VoteScore.Should().Be(1); // Changed from -1 to +1
        comment.Votes.First().Type.Should().Be(VoteType.Upvote);
    }

    [Fact]
    public void ChangeVote_WhenUserHasNotVoted_ShouldFail()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;

        // Act
        var result = comment.ChangeVote(Guid.NewGuid(), VoteType.Downvote);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.VoteNotFound");
    }

    [Fact]
    public void ChangeVote_OnDeletedComment_ShouldFail()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();
        comment.AddVote(userId, VoteType.Upvote);
        comment.Delete();

        // Act
        var result = comment.ChangeVote(userId, VoteType.Downvote);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
    }

    [Fact]
    public void RemoveVote_ShouldDecreaseVoteScoreAndRemoveVote()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();
        comment.AddVote(userId, VoteType.Upvote);

        // Act
        var result = comment.RemoveVote(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.VoteScore.Should().Be(0);
        comment.Votes.Should().BeEmpty();
    }

    [Fact]
    public void RemoveVote_WithDownvote_ShouldIncreaseVoteScore()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();
        comment.AddVote(userId, VoteType.Downvote);

        // Act
        var result = comment.RemoveVote(userId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        comment.VoteScore.Should().Be(0); // Removed -1, so back to 0
        comment.Votes.Should().BeEmpty();
    }

    [Fact]
    public void RemoveVote_WhenUserHasNotVoted_ShouldFail()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;

        // Act
        var result = comment.RemoveVote(Guid.NewGuid());

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.VoteNotFound");
    }

    [Fact]
    public void RemoveVote_OnDeletedComment_ShouldFail()
    {
        // Arrange
        var content = CommentContent.Create("Comment").Value;
        var comment = Comment.Create(_postId, _authorId, content).Value;
        var userId = Guid.NewGuid();
        comment.AddVote(userId, VoteType.Upvote);
        comment.Delete();

        // Act
        var result = comment.RemoveVote(userId);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Comment.Deleted");
    }

    #endregion
}
