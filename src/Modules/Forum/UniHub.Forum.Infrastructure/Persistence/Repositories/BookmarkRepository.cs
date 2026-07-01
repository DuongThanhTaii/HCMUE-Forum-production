using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Queries.GetBookmarkedPosts;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.Posts;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of bookmark repository for Forum module.
/// </summary>
public sealed class BookmarkRepository : IBookmarkRepository
{
    private readonly ApplicationDbContext _context;

    public BookmarkRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Bookmark?> GetByUserAndPostAsync(
        Guid userId,
        PostId postId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Bookmarks
            .FirstOrDefaultAsync(b => b.UserId == userId && b.PostId == postId, cancellationToken);
    }

    public async Task<GetBookmarkedPostsResult> GetBookmarkedPostsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = from bookmark in _context.Bookmarks
                    join post in _context.Posts on bookmark.PostId equals post.Id
                    where bookmark.UserId == userId
                    select new { bookmark, post };

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var items = await query
            .OrderByDescending(x => x.bookmark.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(x => new BookmarkedPostDto
            {
                Id = x.post.Id.Value,
                BookmarkedAt = x.bookmark.CreatedAt,
                Title = x.post.Title.Value,
                Slug = x.post.Slug.Value,
                PostType = (int)x.post.Type,
                Status = (int)x.post.Status,
                AuthorId = x.post.AuthorId,
                VoteScore = x.post.VoteScore,
                CommentCount = 0, // Will be filled below if we implement batch count, but for now 0
                ViewCount = x.post.ViewCount,
                LikeCount = x.post.VoteScore,
                BookmarkCount = 0, 
                ReplyCount = 0,
                CategorySlug = null,
                AuthorAvatar = null,
                LastActivity = x.post.UpdatedAt ?? x.post.CreatedAt,
                Preview = x.post.Content.Value.Length > 200 ? x.post.Content.Value.Substring(0, 200) : x.post.Content.Value,
                IsLocked = x.post.IsLocked,
                IsSolved = false,
                CurrentUserVote = null, // Can map similarly if needed
                IsPinned = x.post.IsPinned,
                CreatedAt = x.post.CreatedAt,
                UpdatedAt = x.post.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        var postIdGuids = items.Select(p => p.Id).ToList();
        var commentCounts = new Dictionary<Guid, int>();
        var bookmarkCounts = new Dictionary<Guid, int>();
        var currentUserVotes = new Dictionary<Guid, int>();

        if (postIdGuids.Count > 0)
        {
            var guidArray = postIdGuids.ToArray();
            var rows = await _context.Database
                .SqlQuery<CommentCountRow>($"""
                    SELECT post_id AS "PostId", COUNT(*)::int AS "Count"
                    FROM forum.comments
                    WHERE post_id = ANY({guidArray})
                    GROUP BY post_id
                    """)
                .ToListAsync(cancellationToken);
            commentCounts = rows.ToDictionary(r => r.PostId, r => r.Count);

            var bmRows = await _context.Database
                .SqlQuery<CommentCountRow>($"""
                    SELECT post_id AS "PostId", COUNT(*)::int AS "Count"
                    FROM forum.bookmarks
                    WHERE post_id = ANY({guidArray})
                    GROUP BY post_id
                    """)
                .ToListAsync(cancellationToken);
            bookmarkCounts = bmRows.ToDictionary(r => r.PostId, r => r.Count);

            var voteRows = await _context.Database
                .SqlQuery<VoteTypeRow>($"""
                    SELECT post_id AS "PostId", type AS "VoteType"
                    FROM forum.post_votes
                    WHERE post_id = ANY({guidArray}) AND user_id = {userId}
                    """)
                .ToListAsync(cancellationToken);
            currentUserVotes = voteRows.ToDictionary(r => r.PostId, r => r.VoteType);
        }

        items = items
            .Select(p => p with 
            { 
                CommentCount = commentCounts.GetValueOrDefault(p.Id, 0),
                ReplyCount = commentCounts.GetValueOrDefault(p.Id, 0),
                BookmarkCount = bookmarkCounts.GetValueOrDefault(p.Id, 0),
                CurrentUserVote = currentUserVotes.TryGetValue(p.Id, out var vote) ? vote : null
            })
            .ToList();

        return new GetBookmarkedPostsResult
        {
            Posts = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task AddAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        await _context.Bookmarks.AddAsync(bookmark, cancellationToken);
    }

    public Task RemoveAsync(Bookmark bookmark, CancellationToken cancellationToken = default)
    {
        _context.Bookmarks.Remove(bookmark);
        return Task.CompletedTask;
    }

    private sealed record CommentCountRow(Guid PostId, int Count);
    private sealed record VoteTypeRow(Guid PostId, int VoteType);
}
