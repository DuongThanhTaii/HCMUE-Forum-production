using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Tags;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public class PostTagConfiguration : IEntityTypeConfiguration<PostTag>
{
    public void Configure(EntityTypeBuilder<PostTag> builder)
    {
        builder.ToTable("post_tags", "forum");

        // Composite primary key
        builder.HasKey(pt => new { pt.PostId, pt.TagId });

        // Strongly-typed foreign key: PostId (Guid)
        builder.Property(pt => pt.PostId)
            .HasConversion(
                id => id.Value,
                value => PostId.Create(value))
            .HasColumnName("post_id")
            .IsRequired();

        // Strongly-typed foreign key: TagId (int)
        builder.Property(pt => pt.TagId)
            .HasConversion(
                id => id.Value,
                value => TagId.Create(value))
            .HasColumnName("tag_id")
            .IsRequired();

        // Timestamp
        builder.Property(pt => pt.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        // Indexes
        builder.HasIndex(pt => pt.PostId);
        builder.HasIndex(pt => pt.TagId);
    }
}
