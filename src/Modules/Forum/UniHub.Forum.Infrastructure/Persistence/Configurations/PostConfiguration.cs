using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Posts.ValueObjects;
using UniHub.Forum.Domain.Votes;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public class PostConfiguration : IEntityTypeConfiguration<Post>
{
    public void Configure(EntityTypeBuilder<Post> builder)
    {
        builder.ToTable("posts", "forum");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(
                id => id.Value,
                value => PostId.Create(value))
            .HasColumnName("id");

        // Owned: PostTitle
        builder.OwnsOne(p => p.Title, title =>
        {
            title.Property(t => t.Value)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();
        });

        // Owned: PostContent
        builder.OwnsOne(p => p.Content, content =>
        {
            content.Property(c => c.Value)
                .HasColumnName("content")
                .HasMaxLength(50000)
                .IsRequired();
        });

        // Owned: Slug
        builder.OwnsOne(p => p.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .HasMaxLength(250)
                .IsRequired();

            slug.HasIndex(s => s.Value).IsUnique();
        });

        // Enums
        builder.Property(p => p.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(p => p.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Simple properties
        builder.Property(p => p.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(p => p.CategoryId)
            .HasColumnName("category_id");

        builder.Property(p => p.ThreadChannelId)
            .HasColumnName("thread_channel_id");

        builder.Property(p => p.ViewCount)
            .HasColumnName("view_count")
            .HasDefaultValue(0);

        builder.Property(p => p.VoteScore)
            .HasColumnName("vote_score")
            .HasDefaultValue(0);

        builder.Property(p => p.CommentCount)
            .HasColumnName("comment_count")
            .HasDefaultValue(0);

        builder.Property(p => p.IsPinned)
            .HasColumnName("is_pinned")
            .HasDefaultValue(false);

        builder.Property(p => p.IsLocked)
            .HasColumnName("is_locked")
            .HasDefaultValue(false);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(p => p.PublishedAt)
            .HasColumnName("published_at");

        builder.HasOne<UniHub.Forum.Domain.ThreadChannels.ThreadChannel>()
            .WithMany()
            .HasForeignKey(p => p.ThreadChannelId)
            .OnDelete(DeleteBehavior.SetNull);

        // Primitive collection: Tags (List<string>) - stored as JSON
        builder.Property("_tags")
            .HasColumnName("tags")
            .HasColumnType("jsonb");

        // Owned collection: Votes
        builder.OwnsMany(p => p.Votes, vote =>
        {
            vote.ToTable("post_votes", "forum");

            vote.Property(v => v.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            vote.Property(v => v.Type)
                .HasColumnName("vote_type")
                .HasConversion<int>()
                .IsRequired();

            vote.Property(v => v.CreatedAt)
                .HasColumnName("created_at")
                .IsRequired();

            vote.Property(v => v.UpdatedAt)
                .HasColumnName("updated_at");

            // Configure composite primary key and foreign key
            vote.WithOwner().HasForeignKey("post_id");
            vote.HasKey("post_id", nameof(Vote.UserId));
        });

        // Indexes
        builder.HasIndex(p => p.AuthorId);
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.ThreadChannelId);
        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.CreatedAt);

        builder.Ignore(p => p.DomainEvents);
    }
}
