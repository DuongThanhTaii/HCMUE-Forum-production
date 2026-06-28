using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Learning.Domain.EventSourcing;

namespace UniHub.Learning.Infrastructure.Persistence.Configurations;

internal sealed class StoredEventConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("LearningStoredEvents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.AggregateId)
            .IsRequired();

        builder.Property(x => x.AggregateType)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.EventType)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.EventData)
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.OccurredOn)
            .IsRequired();

        builder.Property(x => x.StoredOn)
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.HasIndex(x => x.AggregateId);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.OccurredOn);
    }
}
