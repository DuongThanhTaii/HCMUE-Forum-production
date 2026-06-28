using Microsoft.EntityFrameworkCore;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Reports;
using UniHub.Forum.Domain.Tags;
using UniHub.Forum.Domain.ThreadChannels;

namespace UniHub.Forum.Infrastructure.Persistence;

public sealed class ForumDbContext : DbContext
{
    public ForumDbContext(DbContextOptions<ForumDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Bookmark> Bookmarks => Set<Bookmark>();
    public DbSet<PostTag> PostTags => Set<PostTag>();
    public DbSet<Report> Reports => Set<Report>();
    public DbSet<ThreadChannel> ThreadChannels => Set<ThreadChannel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ForumDbContext).Assembly);
    }
}
