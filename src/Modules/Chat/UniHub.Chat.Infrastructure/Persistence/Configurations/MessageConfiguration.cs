using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Chat.Domain.Conversations;
using UniHub.Chat.Domain.Messages;

namespace UniHub.Chat.Infrastructure.Persistence.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("messages", "chat");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .HasConversion(
                id => id.Value,
                value => MessageId.Create(value))
            .HasColumnName("id");

        // Strongly-typed foreign key: ConversationId
        builder.Property(m => m.ConversationId)
            .HasConversion(
                id => id.Value,
                value => ConversationId.Create(value))
            .HasColumnName("conversation_id")
            .IsRequired();

        // Nullable self-referencing foreign key: ReplyToMessageId
        builder.Property(m => m.ReplyToMessageId)
            .HasConversion(
                id => id!.Value,
                value => MessageId.Create(value))
            .HasColumnName("reply_to_message_id");

        // Simple properties
        builder.Property(m => m.SenderId)
            .HasColumnName("sender_id")
            .IsRequired();

        builder.Property(m => m.Content)
            .HasColumnName("content")
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(m => m.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(m => m.SentAt)
            .HasColumnName("sent_at")
            .IsRequired();

        builder.Property(m => m.EditedAt)
            .HasColumnName("edited_at");

        builder.Property(m => m.IsDeleted)
            .HasColumnName("is_deleted")
            .HasDefaultValue(false);

        builder.Property(m => m.DeletedAt)
            .HasColumnName("deleted_at");

        // Owned collection: Attachments
        builder.OwnsMany(m => m.Attachments, attachment =>
        {
            attachment.ToTable("message_attachments", "chat");

            attachment.Property(a => a.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();

            attachment.Property(a => a.FileUrl)
                .HasColumnName("file_url")
                .HasMaxLength(2000)
                .IsRequired();

            attachment.Property(a => a.FileSizeBytes)
                .HasColumnName("file_size_bytes")
                .IsRequired();

            attachment.Property(a => a.MimeType)
                .HasColumnName("mime_type")
                .HasMaxLength(100)
                .IsRequired();

            attachment.Property(a => a.ThumbnailUrl)
                .HasColumnName("thumbnail_url")
                .HasMaxLength(2000);

            attachment.WithOwner().HasForeignKey("message_id");
            attachment.HasKey("message_id", nameof(Attachment.FileName));
        });

        // Owned collection: Reactions
        builder.OwnsMany(m => m.Reactions, reaction =>
        {
            reaction.ToTable("message_reactions", "chat");

            reaction.Property(r => r.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            reaction.Property(r => r.Emoji)
                .HasColumnName("emoji")
                .HasMaxLength(10)
                .IsRequired();

            reaction.Property(r => r.ReactedAt)
                .HasColumnName("reacted_at")
                .IsRequired();

            reaction.WithOwner().HasForeignKey("message_id");
            reaction.HasKey("message_id", nameof(Reaction.UserId), nameof(Reaction.Emoji));
        });

        // Owned collection: ReadReceipts
        builder.OwnsMany(m => m.ReadReceipts, receipt =>
        {
            receipt.ToTable("message_read_receipts", "chat");

            receipt.Property(r => r.UserId)
                .HasColumnName("user_id")
                .IsRequired();

            receipt.Property(r => r.ReadAt)
                .HasColumnName("read_at")
                .IsRequired();

            receipt.WithOwner().HasForeignKey("message_id");
            receipt.HasKey("message_id", nameof(ReadReceipt.UserId));
        });

        // Indexes
        builder.HasIndex(m => m.ConversationId);
        builder.HasIndex(m => m.SenderId);
        builder.HasIndex(m => m.SentAt);
        builder.HasIndex(m => m.IsDeleted);
        builder.HasIndex(m => m.ReplyToMessageId);
    }
}
