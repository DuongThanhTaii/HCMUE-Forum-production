using FluentAssertions;
using NSubstitute;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Commands.CreatePost;
using UniHub.Forum.Application.Commands.VotePost;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.Forum.Domain.Votes;

namespace UniHub.Forum.Application.Tests.Commands.VotePost;

public class VotePostCommandHandlerTests
{
    private readonly IPostRepository _postRepository;
    private readonly VotePostCommandHandler _handler;

    public VotePostCommandHandlerTests()
    {
        _postRepository = Substitute.For<IPostRepository>();
        _handler = new VotePostCommandHandler(_postRepository);
    }

    [Fact]
    public async Task Handle_WithNoExistingVote_ShouldAddVote()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var voteType = VoteType.Upvote;

        var post = CreateTestPost(postId);
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new VotePostCommand(postId, userId, voteType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Votes.Should().ContainSingle();
        post.Votes.First().UserId.Should().Be(userId);
        post.Votes.First().Type.Should().Be(voteType);
        post.VoteScore.Should().Be(1);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSameVoteType_ShouldRemoveVote()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var voteType = VoteType.Upvote;

        var post = CreateTestPost(postId);
        post.AddVote(userId, voteType); // Add initial vote

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new VotePostCommand(postId, userId, voteType);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Votes.Should().BeEmpty();
        post.VoteScore.Should().Be(0);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDifferentVoteType_ShouldChangeVote()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var post = CreateTestPost(postId);
        post.AddVote(userId, VoteType.Upvote); // Add initial upvote

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new VotePostCommand(postId, userId, VoteType.Downvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Votes.Should().ContainSingle();
        post.Votes.First().Type.Should().Be(VoteType.Downvote);
        post.VoteScore.Should().Be(-1);
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentPost_ShouldReturnFailure()
    {
        // Arrange
        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns((Post?)null);

        var command = new VotePostCommand(Guid.NewGuid(), Guid.NewGuid(), VoteType.Upvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(PostErrors.PostNotFound);
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDeletedPost_ShouldReturnFailure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var post = CreateTestPost(postId);
        post.Delete(); // Mark as deleted

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new VotePostCommand(postId, userId, VoteType.Upvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Post.Deleted");
        await _postRepository.DidNotReceive().UpdateAsync(Arg.Any<Post>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithMultipleVotes_ShouldCalculateScoreCorrectly()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user3Id = Guid.NewGuid();

        var post = CreateTestPost(postId);
        post.AddVote(user1Id, VoteType.Upvote);
        post.AddVote(user2Id, VoteType.Upvote);

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new VotePostCommand(postId, user3Id, VoteType.Downvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.Votes.Should().HaveCount(3);
        post.VoteScore.Should().Be(1); // 2 upvotes + 1 downvote = 2 - 1 = 1
        await _postRepository.Received(1).UpdateAsync(post, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithUpvoteDownvoteToggle_ShouldUpdateScoreCorrectly()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var post = CreateTestPost(postId);
        post.AddVote(userId, VoteType.Downvote); // Initial downvote (score: -1)

        _postRepository.GetByIdAsync(Arg.Any<PostId>(), Arg.Any<CancellationToken>())
            .Returns(post);

        var command = new VotePostCommand(postId, userId, VoteType.Upvote);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        post.VoteScore.Should().Be(1); // Changed from -1 to +1
        post.Votes.First().Type.Should().Be(VoteType.Upvote);
    }

    private static Post CreateTestPost(Guid postId)
    {
        var title = PostTitle.Create("Test Post Title").Value;
        var content = PostContent.Create("This is test content for the post.").Value;
        var post = Post.Create(title, content, PostType.Discussion, Guid.NewGuid(), null, null).Value;
        typeof(Post).GetProperty(nameof(Post.Id))!.SetValue(post, new PostId(postId));
        return post;
    }
}
