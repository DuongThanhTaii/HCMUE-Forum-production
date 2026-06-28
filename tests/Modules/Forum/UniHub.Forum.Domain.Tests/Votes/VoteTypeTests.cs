using FluentAssertions;
using UniHub.Forum.Domain.Votes;
using Xunit;

namespace UniHub.Forum.Domain.Tests.Votes;

public class VoteTypeTests
{
    [Fact]
    public void Upvote_Should_Have_Value_Of_One()
    {
        // Act
        var upvoteValue = (int)VoteType.Upvote;

        // Assert
        upvoteValue.Should().Be(1);
    }

    [Fact]
    public void Downvote_Should_Have_Value_Of_MinusOne()
    {
        // Act
        var downvoteValue = (int)VoteType.Downvote;

        // Assert
        downvoteValue.Should().Be(-1);
    }

    [Fact]
    public void VoteType_Should_Have_Exactly_Two_Values()
    {
        // Act
        var voteTypes = Enum.GetValues<VoteType>();

        // Assert
        voteTypes.Should().HaveCount(2);
    }

    [Fact]
    public void VoteType_Should_Contain_Upvote_And_Downvote()
    {
        // Act
        var voteTypes = Enum.GetValues<VoteType>();

        // Assert
        voteTypes.Should().Contain(VoteType.Upvote);
        voteTypes.Should().Contain(VoteType.Downvote);
    }
}
