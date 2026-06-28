using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Persistence.Configurations;

internal sealed class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("AIConversations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.SessionId)
            .HasMaxLength(200);

        builder.Property(x => x.Title)
            .HasMaxLength(500);

        builder.Property(x => x.HandoffReason)
            .HasMaxLength(1000);

        builder.Property(x => x.StartedAt)
            .IsRequired();

        builder.Property(x => x.LastActiveAt)
            .IsRequired();

        builder.Property(x => x.IsClosed)
            .IsRequired();

        builder.HasMany(x => x.Messages)
            .WithOne()
            .HasForeignKey(x => x.ConversationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.SessionId);
        builder.HasIndex(x => x.LastActiveAt);
        builder.HasIndex(x => x.IsClosed);
    }
}
