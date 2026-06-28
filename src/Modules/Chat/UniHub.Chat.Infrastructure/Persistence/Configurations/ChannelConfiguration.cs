using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Chat.Domain.Channels;

namespace UniHub.Chat.Infrastructure.Persistence.Configurations;

public class ChannelConfiguration : IEntityTypeConfiguration<Channel>
{
    public void Configure(EntityTypeBuilder<Channel> builder)
    {
        builder.ToTable("channels", "chat");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => ChannelId.Create(value))
            .HasColumnName("id");

        // Simple properties
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(c => c.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.IsArchived)
            .HasColumnName("is_archived")
            .HasDefaultValue(false);

        builder.Property(c => c.ArchivedAt)
            .HasColumnName("archived_at");

        //Primitive collection: Members (List<Guid>) - stored as JSON
        builder.Property("_members")
            .HasColumnName("members")
            .HasColumnType("jsonb");

        // Primitive collection: Moderators (List<Guid>) - stored as JSON
        builder.Property("_moderators")
            .HasColumnName("moderators")
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Type);
        builder.HasIndex(c => c.OwnerId);
        builder.HasIndex(c => c.IsArchived);

        builder.Ignore(c => c.DomainEvents);
    }
}
