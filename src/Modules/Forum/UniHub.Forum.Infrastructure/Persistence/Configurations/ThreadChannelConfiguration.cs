using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.ThreadChannels;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public sealed class ThreadChannelConfiguration : IEntityTypeConfiguration<ThreadChannel>
{
    public void Configure(EntityTypeBuilder<ThreadChannel> builder)
    {
        builder.ToTable("thread_channels", "forum");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasMaxLength(120)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(x => x.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(x => x.AllowPinnedComments)
            .HasColumnName("allow_pinned_comments")
            .HasDefaultValue(true);

        builder.Property(x => x.AllowAcceptedAnswers)
            .HasColumnName("allow_accepted_answers")
            .HasDefaultValue(true);

        builder.Property(x => x.AllowModeratorActions)
            .HasColumnName("allow_moderator_actions")
            .HasDefaultValue(true);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => new { x.IsActive, x.DisplayOrder });

        builder.Ignore(x => x.DomainEvents);
    }
}
