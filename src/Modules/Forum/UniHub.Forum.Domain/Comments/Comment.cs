using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Events;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Comments;

public sealed class Comment : Entity<CommentId>, IHasDomainEvents
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly List<Vote> _votes = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void ClearDomainEvents() => _domainEvents.Clear();
    public PostId PostId { get; private set; }
    public Guid AuthorId { get; private set; }
    public CommentContent Content { get; private set; }
    public CommentId? ParentCommentId { get; private set; }
    public bool IsAcceptedAnswer { get; private set; }
    public bool IsPinned { get; private set; }
    public int VoteScore { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public IReadOnlyList<Vote> Votes => _votes.AsReadOnly();

    // EF Core constructor
    private Comment()
    {
        PostId = null!;
        Content = null!;
    }

    private Comment(
        CommentId id,
        PostId postId,
        Guid authorId,
        CommentContent content,
        CommentId? parentCommentId)
    {
        Id = id;
        PostId = postId;
        AuthorId = authorId;
        Content = content;
        ParentCommentId = parentCommentId;
        IsAcceptedAnswer = false;
        IsPinned = false;
        VoteScore = 0;
        IsDeleted = false;
        CreatedAt = DateTime.UtcNow;
    }

    public static Result<Comment> Create(
        PostId postId,
        Guid authorId,
        CommentContent content,
        CommentId? parentCommentId = null)
    {
        var comment = new Comment(
            CommentId.CreateUnique(),
            postId,
            authorId,
            content,
            parentCommentId);

        comment.RaiseDomainEvent(new CommentAddedEvent(comment.Id, comment.PostId, comment.AuthorId, comment.ParentCommentId));
        return Result.Success(comment);
    }

    public Result Update(CommentContent newContent)
    {
        if (IsDeleted)
        {
            return Result.Failure(new Error("Comment.Deleted", "Cannot update a deleted comment"));
        }

        Content = newContent;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CommentUpdatedEvent(Id, PostId));
        return Result.Success();
    }

    public Result Delete()
    {
        if (IsDeleted)
        {
            return Result.Failure(new Error("Comment.AlreadyDeleted", "Comment is already deleted"));
        }

        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CommentDeletedEvent(Id, PostId));
        return Result.Success();
    }

    public Result Pin()
    {
        if (IsDeleted)
        {
            return Result.Failure(new Error("Comment.Deleted", "Cannot pin a deleted comment"));
        }

        if (IsPinned)
        {
            return Result.Success();
        }

        IsPinned = true;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result Unpin()
    {
        if (!IsPinned)
        {
            return Result.Success();
        }

        IsPinned = false;
        UpdatedAt = DateTime.UtcNow;
        return Result.Success();
    }

    public Result AcceptAsAnswer()
    {
        if (IsDeleted)
        {
            return Result.Failure(new Error("Comment.Deleted", "Cannot accept a deleted comment as answer"));
        }

        if (IsAcceptedAnswer)
        {
            return Result.Failure(new Error("Comment.AlreadyAccepted", "Comment is already accepted as answer"));
        }

        if (ParentCommentId is not null)
        {
            return Result.Failure(new Error("Comment.NestedComment", "Nested comments cannot be accepted as answers"));
        }

        IsAcceptedAnswer = true;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CommentAcceptedAsAnswerEvent(Id, PostId, AuthorId));
        return Result.Success();
    }

    public Result UnacceptAsAnswer()
    {
        if (!IsAcceptedAnswer)
        {
            return Result.Failure(new Error("Comment.NotAccepted", "Comment is not accepted as answer"));
        }

        IsAcceptedAnswer = false;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new CommentUnacceptedAsAnswerEvent(Id, PostId));
        return Result.Success();
    }

    public Result AddVote(Guid userId, VoteType voteType)
    {
        if (IsDeleted)
        {
            return Result.Failure(new Error("Comment.Deleted", "Cannot vote on a deleted comment"));
        }

        var existingVote = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is not null)
        {
            return Result.Failure(new Error("Comment.VoteAlreadyExists", "User has already voted on this comment"));
        }

        var voteResult = Vote.Create(userId, voteType);
        if (voteResult.IsFailure)
        {
            return Result.Failure(voteResult.Error);
        }

        _votes.Add(voteResult.Value);
        VoteScore += voteResult.Value.GetScoreValue();

        RaiseDomainEvent(new CommentVoteAddedEvent(Id, userId, voteType));
        return Result.Success();
    }

    public Result ChangeVote(Guid userId, VoteType newVoteType)
    {
        if (IsDeleted)
        {
            return Result.Failure(new Error("Comment.Deleted", "Cannot change vote on a deleted comment"));
        }

        var existingVote = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is null)
        {
            return Result.Failure(new Error("Comment.VoteNotFound", "User has not voted on this comment"));
        }

        var oldVoteType = existingVote.Type;
        var changedVoteResult = existingVote.Change(newVoteType);
        if (changedVoteResult.IsFailure)
        {
            return Result.Failure(changedVoteResult.Error);
        }

        // Remove old vote score and add new vote score
        VoteScore -= existingVote.GetScoreValue();
        VoteScore += changedVoteResult.Value.GetScoreValue();

        // Replace the vote in the collection
        _votes.Remove(existingVote);
        _votes.Add(changedVoteResult.Value);

        RaiseDomainEvent(new CommentVoteChangedEvent(Id, userId, oldVoteType, newVoteType));
        return Result.Success();
    }

    public Result RemoveVote(Guid userId)
    {
        if (IsDeleted)
        {
            return Result.Failure(new Error("Comment.Deleted", "Cannot remove vote from a deleted comment"));
        }

        var existingVote = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is null)
        {
            return Result.Failure(new Error("Comment.VoteNotFound", "User has not voted on this comment"));
        }

        VoteScore -= existingVote.GetScoreValue();
        _votes.Remove(existingVote);

        RaiseDomainEvent(new CommentVoteRemovedEvent(Id, userId, existingVote.Type));
        return Result.Success();
    }

    private void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);
}
