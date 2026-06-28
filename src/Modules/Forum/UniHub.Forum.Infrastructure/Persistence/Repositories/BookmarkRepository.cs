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
                CommentCount = 0, // TODO: Calculate from Comments table
                ViewCount = x.post.ViewCount,
                IsPinned = x.post.IsPinned,
                CreatedAt = x.post.CreatedAt
            })
            .ToListAsync(cancellationToken);

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
}
