using UniHub.Forum.Domain.Events;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.Forum.Domain.Votes;
using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Forum.Domain.Posts;

public sealed class Post : AggregateRoot<PostId>
{
    private readonly List<string> _tags = new();
    private readonly List<Vote> _votes = new();

    public PostTitle Title { get; private set; }
    public PostContent Content { get; private set; }
    public Slug Slug { get; private set; }
    public PostType Type { get; private set; }
    public PostStatus Status { get; private set; }
    public Guid AuthorId { get; private set; }
    public Guid? CategoryId { get; private set; }
    public Guid? ThreadChannelId { get; private set; }
    public int ViewCount { get; private set; }
    public int VoteScore { get; private set; }
    public int CommentCount { get; private set; }
    public bool IsPinned { get; private set; }
    public bool IsLocked { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();
    public IReadOnlyList<Vote> Votes => _votes.AsReadOnly();

    // EF Core constructor
    private Post()
    {
        Title = null!;
        Content = null!;
        Slug = null!;
    }

    private Post(
        PostId id,
        PostTitle title,
        PostContent content,
        Slug slug,
        PostType type,
        Guid authorId,
        Guid? categoryId,
        Guid? threadChannelId,
        IEnumerable<string> tags)
    {
        Id = id;
        Title = title;
        Content = content;
        Slug = slug;
        Type = type;
        Status = PostStatus.Draft;
        AuthorId = authorId;
        CategoryId = categoryId;
        ThreadChannelId = threadChannelId;
        ViewCount = 0;
        VoteScore = 0;
        CommentCount = 0;
        IsPinned = false;
        IsLocked = false;
        CreatedAt = DateTime.UtcNow;
        _tags.AddRange(tags);
    }

    public static Result<Post> Create(
        PostTitle title,
        PostContent content,
        PostType type,
        Guid authorId,
        Guid? categoryId = null,
        IEnumerable<string>? tags = null,
        Guid? threadChannelId = null)
    {
        var slugResult = Slug.CreateFromTitle(title);
        if (slugResult.IsFailure)
        {
            return Result.Failure<Post>(slugResult.Error);
        }

        var post = new Post(
            PostId.CreateUnique(),
            title,
            content,
            slugResult.Value,
            type,
            authorId,
            categoryId,
            threadChannelId,
            tags ?? Enumerable.Empty<string>());

        post.AddDomainEvent(new PostCreatedEvent(post.Id, post.AuthorId, post.Type));
        return Result.Success(post);
    }

    public Result Update(PostTitle title, PostContent content, Guid? categoryId, Guid? threadChannelId, IEnumerable<string>? tags)
    {
        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(new Error("Post.Deleted", "Cannot update a deleted post"));
        }

        var slugResult = Slug.CreateFromTitle(title);
        if (slugResult.IsFailure)
        {
            return Result.Failure(slugResult.Error);
        }

        Title = title;
        Content = content;
        Slug = slugResult.Value;
        CategoryId = categoryId;
        ThreadChannelId = threadChannelId;
        UpdatedAt = DateTime.UtcNow;

        if (tags is not null)
        {
            _tags.Clear();
            _tags.AddRange(tags);
        }

        AddDomainEvent(new PostUpdatedEvent(Id, AuthorId));
        return Result.Success();
    }

    public Result Publish()
    {
        if (Status == PostStatus.Published)
        {
            return Result.Failure(new Error("Post.AlreadyPublished", "Post is already published"));
        }

        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(new Error("Post.Deleted", "Cannot publish a deleted post"));
        }

        Status = PostStatus.Published;
        PublishedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PostPublishedEvent(Id, AuthorId));
        return Result.Success();
    }

    public Result Archive()
    {
        if (Status == PostStatus.Archived)
        {
            return Result.Failure(new Error("Post.AlreadyArchived", "Post is already archived"));
        }

        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(new Error("Post.Deleted", "Cannot archive a deleted post"));
        }

        Status = PostStatus.Archived;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PostArchivedEvent(Id));
        return Result.Success();
    }

    public Result Delete()
    {
        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(new Error("Post.AlreadyDeleted", "Post is already deleted"));
        }

        Status = PostStatus.Deleted;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PostDeletedEvent(Id, AuthorId));
        return Result.Success();
    }

    public Result Pin()
    {
        if (IsPinned)
        {
            return Result.Failure(new Error("Post.AlreadyPinned", "Post is already pinned"));
        }

        IsPinned = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PostPinnedEvent(Id));
        return Result.Success();
    }

    public Result Unpin()
    {
        if (!IsPinned)
        {
            return Result.Failure(new Error("Post.NotPinned", "Post is not pinned"));
        }

        IsPinned = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PostUnpinnedEvent(Id));
        return Result.Success();
    }

    public Result Lock()
    {
        if (IsLocked)
        {
            return Result.Failure(new Error("Post.AlreadyLocked", "Post is already locked"));
        }

        IsLocked = true;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PostLockedEvent(Id));
        return Result.Success();
    }

    public Result Unlock()
    {
        if (!IsLocked)
        {
            return Result.Failure(new Error("Post.NotLocked", "Post is not locked"));
        }

        IsLocked = false;
        UpdatedAt = DateTime.UtcNow;

        AddDomainEvent(new PostUnlockedEvent(Id));
        return Result.Success();
    }

    public void IncrementViewCount()
    {
        ViewCount++;
    }

    public Result AddVote(Guid userId, VoteType voteType)
    {
        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(new Error("Post.Deleted", "Cannot vote on a deleted post"));
        }

        var existingVote = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is not null)
        {
            return Result.Failure(new Error("Post.VoteAlreadyExists", "User has already voted on this post"));
        }

        var voteResult = Vote.Create(userId, voteType);
        if (voteResult.IsFailure)
        {
            return Result.Failure(voteResult.Error);
        }

        _votes.Add(voteResult.Value);
        VoteScore += voteResult.Value.GetScoreValue();

        AddDomainEvent(new PostVoteAddedEvent(Id, userId, voteType));
        return Result.Success();
    }

    public Result ChangeVote(Guid userId, VoteType newVoteType)
    {
        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(new Error("Post.Deleted", "Cannot change vote on a deleted post"));
        }

        var existingVote = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is null)
        {
            return Result.Failure(new Error("Post.VoteNotFound", "User has not voted on this post"));
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

        AddDomainEvent(new PostVoteChangedEvent(Id, userId, oldVoteType, newVoteType));
        return Result.Success();
    }

    public Result RemoveVote(Guid userId)
    {
        if (Status == PostStatus.Deleted)
        {
            return Result.Failure(new Error("Post.Deleted", "Cannot remove vote from a deleted post"));
        }

        var existingVote = _votes.FirstOrDefault(v => v.UserId == userId);
        if (existingVote is null)
        {
            return Result.Failure(new Error("Post.VoteNotFound", "User has not voted on this post"));
        }

        VoteScore -= existingVote.GetScoreValue();
        _votes.Remove(existingVote);

        AddDomainEvent(new PostVoteRemovedEvent(Id, userId, existingVote.Type));
        return Result.Success();
    }

    public void IncrementCommentCount()
    {
        CommentCount++;
    }

    public void DecrementCommentCount()
    {
        if (CommentCount > 0)
        {
            CommentCount--;
        }
    }
}
