using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Queries.GetComments;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;
using UniHub.Forum.Infrastructure.Persistence;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of comment repository for Forum module.
/// </summary>
public sealed class CommentRepository : ICommentRepository
{
    private readonly ApplicationDbContext _context;

    public CommentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(CommentId commentId, CancellationToken cancellationToken = default)
    {
        return await _context.Comments
            .Include(c => c.Votes)
            .FirstOrDefaultAsync(c => c.Id == commentId, cancellationToken);
    }

    public async Task<IReadOnlyList<Comment>> GetByPostIdAsync(PostId postId, CancellationToken cancellationToken = default)
    {
        var comments = await _context.Comments
            .Where(c => c.PostId == postId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return comments.AsReadOnly();
    }

    public async Task AddAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        await _context.Comments.AddAsync(comment, cancellationToken);
    }

    public async Task UpdateAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        // Keep tracked aggregate state; do not force Update() for owned Vote rows.
        await EnsureNewCommentVotesAreInsertedAsync(comment, cancellationToken);
    }

    private async Task EnsureNewCommentVotesAreInsertedAsync(Comment comment, CancellationToken cancellationToken)
    {
        var voteEntries = _context.ChangeTracker.Entries<Vote>()
            .Where(e => e.State == EntityState.Modified)
            .Where(e => e.Metadata.GetTableName() == "comment_votes")
            .ToList();

        if (voteEntries.Count == 0)
        {
            return;
        }

        var existingUserIds = await _context.Comments
            .AsNoTracking()
            .Where(c => c.Id == comment.Id)
            .SelectMany(c => c.Votes.Select(v => v.UserId))
            .ToListAsync(cancellationToken);

        var existingSet = existingUserIds.ToHashSet();
        foreach (var entry in voteEntries)
        {
            var userId = entry.Entity.UserId;
            if (!existingSet.Contains(userId))
            {
                entry.State = EntityState.Added;
            }
        }
    }

    public Task DeleteAsync(Comment comment, CancellationToken cancellationToken = default)
    {
        _context.Comments.Remove(comment);
        return Task.CompletedTask;
    }

    public async Task<GetCommentsResult> GetCommentsByPostIdAsync(
        PostId postId,
        Guid? currentUserId = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Comments.Where(c => c.PostId == postId);

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var items = await query
            .OrderByDescending(c => c.IsPinned)
            .ThenBy(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(c => new CommentItem
            {
                Id = c.Id.Value,
                PostId = c.PostId.Value,
                AuthorId = c.AuthorId,
                AuthorName = string.Empty,
                Content = c.Content.Value,
                ParentCommentId = c.ParentCommentId != null ? c.ParentCommentId.Value : null,
                VoteScore = c.VoteScore,
                CurrentUserVote = currentUserId.HasValue
                    ? c.Votes
                        .Where(v => v.UserId == currentUserId.Value)
                        .Select(v => (int?)v.Type)
                        .FirstOrDefault()
                    : null,
                IsAcceptedAnswer = c.IsAcceptedAnswer,
                IsPinned = c.IsPinned,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var authorIds = items.Select(i => i.AuthorId).Distinct().ToList();
        var authorMap = await DisplayNameLookup.LoadAuthorNamesAsync(_context, authorIds, cancellationToken);
        for (var i = 0; i < items.Count; i++)
        {
            var c = items[i];
            var name = authorMap.TryGetValue(c.AuthorId, out var n) ? n : string.Empty;
            items[i] = c with { AuthorName = name };
        }

        return new GetCommentsResult
        {
            Comments = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
