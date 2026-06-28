using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Chat.Domain.Safety;

namespace UniHub.Chat.Infrastructure.Persistence.Configurations;

public sealed class ConversationMuteConfiguration : IEntityTypeConfiguration<ConversationMute>
{
    public void Configure(EntityTypeBuilder<ConversationMute> builder)
    {
        builder.ToTable("conversation_mutes", "chat");

        builder.HasKey(m => new { m.UserId, m.ConversationId });

        builder.Property(m => m.UserId).HasColumnName("user_id");
        builder.Property(m => m.ConversationId).HasColumnName("conversation_id");
        builder.Property(m => m.IsMuted).HasColumnName("is_muted");
        builder.Property(m => m.UpdatedAt).HasColumnName("updated_at");
    }
}
