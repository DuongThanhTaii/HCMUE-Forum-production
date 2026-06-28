using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Chat.Domain.Safety;

namespace UniHub.Chat.Infrastructure.Persistence.Configurations;

public sealed class ChatMessageReportConfiguration : IEntityTypeConfiguration<ChatMessageReport>
{
    public void Configure(EntityTypeBuilder<ChatMessageReport> builder)
    {
        builder.ToTable("message_reports", "chat");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).HasColumnName("id").ValueGeneratedOnAdd();

        builder.Property(r => r.MessageId).HasColumnName("message_id");
        builder.Property(r => r.ConversationId).HasColumnName("conversation_id");
        builder.Property(r => r.ReporterId).HasColumnName("reporter_id");
        builder.Property(r => r.Reason).HasColumnName("reason").HasConversion<int>();
        builder.Property(r => r.Description).HasColumnName("description").HasMaxLength(2000);
        builder.Property(r => r.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(r => r.MessageId);
        builder.HasIndex(r => r.ReporterId);
        builder.HasIndex(r => new { r.ReporterId, r.MessageId }).IsUnique();
    }
}
