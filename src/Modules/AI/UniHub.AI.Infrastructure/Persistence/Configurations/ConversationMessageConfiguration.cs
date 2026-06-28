using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Persistence.Configurations;

internal sealed class ConversationMessageConfiguration : IEntityTypeConfiguration<ConversationMessage>
{
    public void Configure(EntityTypeBuilder<ConversationMessage> builder)
    {
        builder.ToTable("AIConversationMessages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.ConversationId)
            .IsRequired();

        builder.Property(x => x.Role)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Content)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(x => x.ConfidenceScore);

        builder.Property(x => x.SentAt)
            .IsRequired();

        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => x.Role);
        builder.HasIndex(x => x.SentAt);
        builder.HasIndex(x => x.SourceFAQId);
    }
}
