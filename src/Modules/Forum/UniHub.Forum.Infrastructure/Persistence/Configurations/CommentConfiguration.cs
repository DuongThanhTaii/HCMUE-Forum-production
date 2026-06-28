using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.Comments;
using UniHub.Forum.Domain.Comments.ValueObjects;
using UniHub.Forum.Domain.Posts;
using UniHub.Forum.Domain.Votes;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public class CommentConfiguration : IEntityTypeConfiguration<Comment>
{
    public void Configure(EntityTypeBuilder<Comment> builder)
    {
        builder.ToTable("comments", "forum");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CommentId.Create(value))
            .HasColumnName("id");

        // Strongly-typed foreign key
        builder.Property(c => c.PostId)
            .HasConversion(
                id => id.Value,
                value => PostId.Create(value))
            .HasColumnName("post_id")
            .IsRequired();

        // Self-referencing foreign key (nullable)
        builder.Property(c => c.ParentCommentId)
            .HasConversion(
                id => id!.Value,
                value => CommentId.Create(value))
            .HasColumnName("parent_comment_id");

        // Owned: CommentContent
        builder.OwnsOne(c => c.Content, content =>
        {
            content.Property(cc => cc.Value)
                .HasColumnName("content")
                .HasMaxLength(10000)
                .IsRequired();
        });

        // Simple properties
        builder.Property(c => c.AuthorId)
            .HasColumnName("author_id")
            .IsRequired();

        builder.Property(c => c.IsAcceptedAnswer)
            .HasColumnName("is_accepted_answer")
            .HasDefaultValue(false);

        builder.Property(c => c.IsPinned)
            .HasColumnName("is_pinned")
            .HasDefaultValue(false);

        builder.Property(c => c.VoteScore)
            .HasColumnName("vote_score")
            .HasDefaultValue(0);

        builder.Property(c => c.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Owned collection: Votes
        builder.OwnsMany(c => c.Votes, vote =>
        {
            vote.ToTable("comment_votes", "forum");

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
            vote.WithOwner().HasForeignKey("comment_id");
            vote.HasKey("comment_id", nameof(Vote.UserId));
        });

        // Indexes
        builder.HasIndex(c => c.PostId);
        builder.HasIndex(c => c.AuthorId);
        builder.HasIndex(c => c.ParentCommentId);
        builder.HasIndex(c => c.CreatedAt);
    }
}
