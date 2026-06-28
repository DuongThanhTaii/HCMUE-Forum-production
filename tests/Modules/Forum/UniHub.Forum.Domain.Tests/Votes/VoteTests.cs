using FluentAssertions;
using UniHub.Forum.Domain.Votes;
using Xunit;

namespace UniHub.Forum.Domain.Tests.Votes;

public class VoteTests
{
    [Fact]
    public void Create_Should_Create_Valid_Upvote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var voteType = VoteType.Upvote;

        // Act
        var result = Vote.Create(userId, voteType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Type.Should().Be(voteType);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Create_Valid_Downvote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var voteType = VoteType.Downvote;

        // Act
        var result = Vote.Create(userId, voteType);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.UserId.Should().Be(userId);
        result.Value.Type.Should().Be(voteType);
        result.Value.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Value.UpdatedAt.Should().BeNull();
    }

    [Fact]
    public void Create_Should_Fail_When_UserId_Is_Empty()
    {
        // Arrange
        var userId = Guid.Empty;
        var voteType = VoteType.Upvote;

        // Act
        var result = Vote.Create(userId, voteType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vote.InvalidUserId");
    }

    [Fact]
    public void Create_Should_Fail_When_VoteType_Is_Invalid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var invalidVoteType = (VoteType)999;

        // Act
        var result = Vote.Create(userId, invalidVoteType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vote.InvalidVoteType");
    }

    [Fact]
    public void GetScoreValue_Should_Return_One_For_Upvote()
    {
        // Arrange
        var vote = Vote.Create(Guid.NewGuid(), VoteType.Upvote).Value;

        // Act
        var scoreValue = vote.GetScoreValue();

        // Assert
        scoreValue.Should().Be(1);
    }

    [Fact]
    public void GetScoreValue_Should_Return_MinusOne_For_Downvote()
    {
        // Arrange
        var vote = Vote.Create(Guid.NewGuid(), VoteType.Downvote).Value;

        // Act
        var scoreValue = vote.GetScoreValue();

        // Assert
        scoreValue.Should().Be(-1);
    }

    [Fact]
    public void Change_Should_Change_Vote_From_Upvote_To_Downvote()
    {
        // Arrange
        var vote = Vote.Create(Guid.NewGuid(), VoteType.Upvote).Value;

        // Act
        var result = vote.Change(VoteType.Downvote);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(VoteType.Downvote);
        result.Value.UpdatedAt.Should().NotBeNull();
        result.Value.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        result.Value.CreatedAt.Should().Be(vote.CreatedAt);
    }

    [Fact]
    public void Change_Should_Change_Vote_From_Downvote_To_Upvote()
    {
        // Arrange
        var vote = Vote.Create(Guid.NewGuid(), VoteType.Downvote).Value;

        // Act
        var result = vote.Change(VoteType.Upvote);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be(VoteType.Upvote);
        result.Value.UpdatedAt.Should().NotBeNull();
        result.Value.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Change_Should_Fail_When_New_VoteType_Is_Same_As_Current()
    {
        // Arrange
        var vote = Vote.Create(Guid.NewGuid(), VoteType.Upvote).Value;

        // Act
        var result = vote.Change(VoteType.Upvote);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vote.SameVoteType");
    }

    [Fact]
    public void Change_Should_Fail_When_VoteType_Is_Invalid()
    {
        // Arrange
        var vote = Vote.Create(Guid.NewGuid(), VoteType.Upvote).Value;
        var invalidVoteType = (VoteType)999;

        // Act
        var result = vote.Change(invalidVoteType);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vote.InvalidVoteType");
    }

    [Fact]
    public void Votes_With_Same_UserId_And_Type_Should_Be_Equal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vote1 = Vote.Create(userId, VoteType.Upvote).Value;
        var vote2 = Vote.Create(userId, VoteType.Upvote).Value;

        // Act & Assert
        vote1.Should().Be(vote2);
    }

    [Fact]
    public void Votes_With_Different_UserId_Should_Not_Be_Equal()
    {
        // Arrange
        var vote1 = Vote.Create(Guid.NewGuid(), VoteType.Upvote).Value;
        var vote2 = Vote.Create(Guid.NewGuid(), VoteType.Upvote).Value;

        // Act & Assert
        vote1.Should().NotBe(vote2);
    }

    [Fact]
    public void Votes_With_Different_Type_Should_Not_Be_Equal()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var vote1 = Vote.Create(userId, VoteType.Upvote).Value;
        var vote2 = Vote.Create(userId, VoteType.Downvote).Value;

        // Act & Assert
        vote1.Should().NotBe(vote2);
    }
}
