using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.Bookmarks;
using UniHub.Forum.Domain.Posts;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public class BookmarkConfiguration : IEntityTypeConfiguration<Bookmark>
{
    public void Configure(EntityTypeBuilder<Bookmark> builder)
    {
        builder.ToTable("bookmarks", "forum");

        // Composite primary key
        builder.HasKey(b => new { b.PostId, b.UserId });

        // Strongly-typed foreign key: PostId (Guid)
        builder.Property(b => b.PostId)
            .HasConversion(
                id => id.Value,
                value => PostId.Create(value))
            .HasColumnName("post_id")
            .IsRequired();

        // User foreign key
        builder.Property(b => b.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Timestamp
        builder.Property(b => b.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(b => b.PostId);
        builder.HasIndex(b => b.UserId);
        builder.HasIndex(b => b.CreatedAt);
    }
}
