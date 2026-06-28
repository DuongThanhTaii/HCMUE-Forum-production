using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Application.Abstractions;
using UniHub.Forum.Application.Queries.GetPopularTags;
using UniHub.Forum.Application.Queries.GetTags;
using UniHub.Forum.Domain.Tags;
using UniHub.Infrastructure.Persistence;

namespace UniHub.Forum.Infrastructure.Persistence.Repositories;

/// <summary>
/// EF Core implementation of tag repository for Forum module.
/// </summary>
public sealed class TagRepository : ITagRepository
{
    private readonly ApplicationDbContext _context;

    public TagRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(TagId id, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<Tag?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Name.Value.ToLower() == name.ToLower(), cancellationToken);
    }

    public async Task<Tag?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Tags
            .FirstOrDefaultAsync(t => t.Slug.Value.ToLower() == slug.ToLower(), cancellationToken);
    }

    public async Task<GetTagsResult> GetTagsAsync(
        int pageNumber,
        int pageSize,
        string? searchTerm,
        bool orderByUsage,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tags.AsQueryable();

        // Filter by search term
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(t =>
                t.Name.Value.ToLower().Contains(lowerSearchTerm) ||
                t.Description.Value.ToLower().Contains(lowerSearchTerm));
        }

        // Order by usage or name
        query = orderByUsage
            ? query.OrderByDescending(t => t.UsageCount)
            : query.OrderBy(t => t.Name.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply pagination
        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(t => new TagDto
            {
                Id = (int)t.Id.Value,
                Name = t.Name.Value,
                Slug = t.Slug.Value,
                Description = t.Description.Value,
                UsageCount = t.UsageCount,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync(cancellationToken);

        return new GetTagsResult
        {
            Tags = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<PopularTagDto>> GetPopularTagsAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        var popularTags = await _context.Tags
            .OrderByDescending(t => t.UsageCount)
            .Take(count)
            .AsNoTracking()
            .Select(t => new PopularTagDto
            {
                Id = (int)t.Id.Value,
                Name = t.Name.Value,
                Slug = t.Slug.Value,
                UsageCount = t.UsageCount
            })
            .ToListAsync(cancellationToken);

        return popularTags;
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await _context.Tags.AddAsync(tag, cancellationToken);
    }

    public Task UpdateAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _context.Tags.Update(tag);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        _context.Tags.Remove(tag);
        return Task.CompletedTask;
    }
}
