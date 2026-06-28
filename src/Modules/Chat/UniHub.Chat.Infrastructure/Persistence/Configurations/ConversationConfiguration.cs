using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Chat.Domain.Conversations;

namespace UniHub.Chat.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations", "chat");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => ConversationId.Create(value))
            .HasColumnName("id");

        // Enum: ConversationType (0=Direct, 1=Group)
        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        // Title (nullable, required for Group conversations)
        builder.Property(c => c.Title)
            .HasColumnName("title")
            .HasMaxLength(100);

        // Simple properties
        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.LastMessageAt)
            .HasColumnName("last_message_at");

        builder.Property(c => c.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false);

        // Primitive collection: Participants (List<Guid>) - stored as JSON
        builder.Property("_participants")
            .HasColumnName("participants")
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(c => c.CreatedBy);
        builder.HasIndex(c => c.Type);
        builder.HasIndex(c => c.IsArchived);
        builder.HasIndex(c => c.LastMessageAt);

        builder.Ignore(c => c.DomainEvents);
    }
}
