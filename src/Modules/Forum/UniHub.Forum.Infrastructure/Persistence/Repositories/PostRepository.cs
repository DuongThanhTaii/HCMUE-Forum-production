using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Queries;
using UniHub.Forum.Application.Queries.GetPostById;
using UniHub.Forum.Application.Queries.GetPosts;
using UniHub.Forum.Application.Queries.SearchPosts;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;
using UniHub.Forum.Infrastructure.Persistence;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of post repository for Forum module.
/// </summary>
public sealed class PostRepository : IPostRepository
{
    private readonly ApplicationDbContext _context;

    public PostRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Post>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var posts = await _context.Posts
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return posts.AsReadOnly();
    }

    public async Task<Post?> GetByIdAsync(PostId postId, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .Include(p => p.Votes)
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
    }

    public async Task<Post?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(p => p.Slug.Value == slug, cancellationToken);
    }

    public async Task<bool> IsSlugUniqueAsync(string slug, CancellationToken cancellationToken = default)
    {
        var exists = await _context.Posts
            .AnyAsync(p => p.Slug.Value == slug, cancellationToken);
        return !exists;
    }

    public async Task AddAsync(Post post, CancellationToken cancellationToken = default)
    {
        await _context.Posts.AddAsync(post, cancellationToken);
    }

    public async Task UpdateAsync(Post post, CancellationToken cancellationToken = default)
    {
        // Aggregate mutations are expected to happen on tracked entities loaded in this UoW.
        // Avoid DbSet.Update() because it can force owned Vote rows into Modified state.
        await EnsureNewPostVotesAreInsertedAsync(post, cancellationToken);
    }

    private async Task EnsureNewPostVotesAreInsertedAsync(Post post, CancellationToken cancellationToken)
    {
        var voteEntries = _context.ChangeTracker.Entries<Vote>()
            .Where(e => e.State == EntityState.Modified)
            .Where(e => e.Metadata.GetTableName() == "post_votes")
            .ToList();

        if (voteEntries.Count == 0)
        {
            return;
        }

        var existingUserIds = await _context.Posts
            .AsNoTracking()
            .Where(p => p.Id == post.Id)
            .SelectMany(p => p.Votes.Select(v => v.UserId))
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

    public Task DeleteAsync(Post post, CancellationToken cancellationToken = default)
    {
        _context.Posts.Remove(post);
        return Task.CompletedTask;
    }

    public async Task<SearchPostsResult> SearchAsync(
        string searchTerm,
        Guid? categoryId = null,
        int? postType = null,
        IEnumerable<string>? tags = null,
        int pageNumber = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Posts.AsQueryable();

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(p =>
                p.Title.Value.ToLower().Contains(lowerSearchTerm) ||
                p.Content.Value.ToLower().Contains(lowerSearchTerm));
        }

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        // Filter by post type
        if (postType.HasValue)
        {
            query = query.Where(p => (int)p.Type == postType.Value);
        }

        // Filter by tags
        if (tags != null && tags.Any())
        {
            var tagList = tags.ToList();
            query = query.Where(p => p.Tags.Any(t => tagList.Contains(t)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(p => new PostSearchResult
            {
                Id = p.Id.Value,
                Title = p.Title.Value,
                Content = p.Content.Value,
                Slug = p.Slug.Value,
                Type = (int)p.Type,
                AuthorId = p.AuthorId,
                VoteScore = p.VoteScore,
                CategoryId = p.CategoryId.GetValueOrDefault(),
                Tags = p.Tags.ToList(),
                CreatedAt = p.CreatedAt,
                CommentCount = 0, // TODO: Calculate from Comments table
                IsPinned = p.IsPinned,
                SearchRank = 1.0 // TODO: Implement full-text search ranking
            })
            .ToListAsync(cancellationToken);

        return new SearchPostsResult
        {
            Posts = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<GetPostsResult> GetPostsAsync(
        GetPostsQuery q,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Posts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(q.SearchTerm))
        {
            var lowerSearchTerm = q.SearchTerm.ToLower();
            query = query.Where(p =>
                p.Title.Value.ToLower().Contains(lowerSearchTerm) ||
                p.Content.Value.ToLower().Contains(lowerSearchTerm));
        }

        // Filter by category
        if (q.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == q.CategoryId);
        }

        if (q.ThreadChannelId.HasValue)
        {
            query = query.Where(p => p.ThreadChannelId == q.ThreadChannelId);
        }

        if (q.CategoryIds is not null)
        {
            if (q.CategoryIds.Count == 0)
            {
                query = query.Where(p => false);
            }
            else
            {
                query = query.Where(p => p.CategoryId != null && q.CategoryIds.Contains(p.CategoryId.Value));
            }
        }

        // Filter by type
        if (q.Type.HasValue)
        {
            query = query.Where(p => (int)p.Type == q.Type.Value);
        }

        // Filter by status
        if (q.Status.HasValue)
        {
            query = query.Where(p => (int)p.Status == q.Status.Value);
        }

        if (q.IsPinned.HasValue)
        {
            query = query.Where(p => p.IsPinned == q.IsPinned.Value);
        }

        if (q.IsUnanswered.HasValue && q.IsUnanswered.Value)
        {
            query = query.Where(p => !_context.Comments.Any(c => c.PostId == p.Id && !c.IsDeleted));
        }

        if (q.IsSolved.HasValue && q.IsSolved.Value)
        {
            query = query.Where(p => _context.Comments.Any(c => c.PostId == p.Id && c.IsAcceptedAnswer && !c.IsDeleted));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination and ordering
        var orderedQuery = q.SortBy switch
        {
            1 => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.VoteScore).ThenByDescending(p => p.CreatedAt), // Trending
            2 => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.UpdatedAt ?? p.CreatedAt), // Recently Active
            3 => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.ViewCount).ThenByDescending(p => p.CreatedAt), // Most Viewed
            4 => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.VoteScore).ThenByDescending(p => p.CreatedAt), // Most Liked
            _ => query.OrderByDescending(p => p.IsPinned).ThenByDescending(p => p.CreatedAt) // Newest (0)
        };

        var items = await orderedQuery
            .Skip((q.PageNumber - 1) * q.PageSize)
            .Take(q.PageSize)
            .AsNoTracking()
            .Select(p => new PostItem
            {
                Id = p.Id.Value,
                Title = p.Title.Value,
                Content = p.Content.Value,
                Slug = p.Slug.Value,
                Type = (int)p.Type,
                Status = (int)p.Status,
                AuthorId = p.AuthorId,
                CategoryId = p.CategoryId,
                ThreadChannelId = p.ThreadChannelId,
                Tags = p.Tags.ToList(),
                VoteScore = p.VoteScore,
                LikeCount = p.VoteScore,
                CommentCount = 0,
                ReplyCount = 0,
                IsPinned = p.IsPinned,
                IsLocked = p.IsLocked,
                ViewCount = p.ViewCount,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                LastActivity = p.UpdatedAt ?? p.CreatedAt,
                IsSolved = _context.Comments.Any(c => c.PostId == p.Id && c.IsAcceptedAnswer && !c.IsDeleted),
                BookmarkCount = 0, // Bookmarks not natively tracked on Post yet
                Preview = p.Content.Value.Length > 200 ? p.Content.Value.Substring(0, 200) : p.Content.Value
            })
            .ToListAsync(cancellationToken);

        // Batch-load comment counts via raw SQL.
        // EF Core 10 cannot translate Contains/GroupBy on value-object-converted FK columns (PostId).
        // EF.Property<Guid> also fails because CLR type is PostId, not Guid.
        // Raw SQL with PostgreSQL ANY(@array) is the reliable alternative.
        var postIdGuids = items.Select(p => p.Id).ToList();
        var commentCounts = new Dictionary<Guid, int>();
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
        }

        items = items
            .Select(p => p with 
            { 
                CommentCount = commentCounts.GetValueOrDefault(p.Id, 0),
                ReplyCount = commentCounts.GetValueOrDefault(p.Id, 0)
            })
            .ToList();

        await EnrichPostItemsAsync(items, cancellationToken);

        return new GetPostsResult
        {
            Posts = items,
            TotalCount = totalCount,
            PageNumber = q.PageNumber,
            PageSize = q.PageSize
        };
    }

    public async Task<PostDetailResult?> GetPostDetailsAsync(
        PostId postId,
        CancellationToken cancellationToken = default)
    {
        var post = await _context.Posts
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == postId, cancellationToken);
        
        if (post == null)
        {
            return null;
        }

        var commentCount = await _context.Comments
            .AsNoTracking()
            .CountAsync(c => c.PostId == postId, cancellationToken);

        string? categoryName = null;
        string? threadChannelName = null;
        string? threadChannelCode = null;
        if (post.CategoryId.HasValue)
        {
            categoryName = await _context.Categories
                .AsNoTracking()
                .Where(c => c.Id == CategoryId.Create(post.CategoryId.Value))
                .Select(c => c.Name.Value)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (post.ThreadChannelId.HasValue)
        {
            var threadChannel = await _context.ThreadChannels
                .AsNoTracking()
                .Where(c => c.Id == post.ThreadChannelId.Value)
                .Select(c => new { c.Name, c.Code })
                .FirstOrDefaultAsync(cancellationToken);
            threadChannelName = threadChannel?.Name;
            threadChannelCode = threadChannel?.Code;
        }

        var authorMap = await DisplayNameLookup.LoadAuthorNamesAsync(
            _context,
            new[] { post.AuthorId },
            cancellationToken);
        authorMap.TryGetValue(post.AuthorId, out var authorName);

        var result = new PostDetailResult
        {
            Id = post.Id.Value,
            Title = post.Title.Value,
            Content = post.Content.Value,
            Slug = post.Slug.Value,
            Type = (int)post.Type,
            Status = (int)post.Status,
            AuthorId = post.AuthorId,
            CategoryId = post.CategoryId,
            ThreadChannelId = post.ThreadChannelId,
            ThreadChannelCode = threadChannelCode,
            ThreadChannelName = threadChannelName,
            CategoryName = categoryName,
            AuthorName = authorName,
            Tags = post.Tags.ToList(),
            VoteScore = post.VoteScore,
            CommentCount = commentCount,
            IsPinned = post.IsPinned,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt
        };

        return result;
    }

    private async Task EnrichPostItemsAsync(List<PostItem> items, CancellationToken cancellationToken)
    {
        if (items.Count == 0)
        {
            return;
        }

        var catIds = items
            .Select(i => i.CategoryId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();
        var authorIds = items.Select(i => i.AuthorId).Distinct().ToList();
        var threadChannelIds = items
            .Select(i => i.ThreadChannelId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        var catMap = await DisplayNameLookup.LoadCategoryNamesAsync(_context, catIds, cancellationToken);
        var authorMap = await DisplayNameLookup.LoadAuthorNamesAsync(_context, authorIds, cancellationToken);
        var threadChannelMap = await _context.ThreadChannels
            .AsNoTracking()
            .Where(x => threadChannelIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, x => (x.Code, x.Name), cancellationToken);

        for (var i = 0; i < items.Count; i++)
        {
            var p = items[i];
            var cn = p.CategoryId.HasValue && catMap.TryGetValue(p.CategoryId.Value, out var cname)
                ? cname
                : null;
            var an = authorMap.TryGetValue(p.AuthorId, out var aname) ? aname : null;
            var threadInfo = p.ThreadChannelId.HasValue && threadChannelMap.TryGetValue(p.ThreadChannelId.Value, out var tc)
                ? tc
                : ((string Code, string Name)?)null;
            var threadCode = threadInfo?.Code;
            var threadName = threadInfo?.Name;
            var catSlug = catMap.TryGetValue(p.CategoryId ?? Guid.Empty, out _) ? null : null; // We might need to map category slug if needed, but for now we only have CategoryName in display lookup. 
            // We'll leave CategorySlug null if DisplayNameLookup doesn't provide it, or we can fetch it if necessary.
            var authorAvatar = (string?)null; // Can map if DisplayNameLookup provides it.
            items[i] = p with { 
                CategoryName = cn, 
                AuthorName = an, 
                ThreadChannelCode = threadCode, 
                ThreadChannelName = threadName,
                AuthorAvatar = authorAvatar
            };
        }
    }

    private sealed record CommentCountRow(Guid PostId, int Count);
}
