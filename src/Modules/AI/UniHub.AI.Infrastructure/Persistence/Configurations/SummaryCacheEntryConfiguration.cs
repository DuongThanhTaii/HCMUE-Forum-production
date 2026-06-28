using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Persistence.Configurations;

internal sealed class SummaryCacheEntryConfiguration : IEntityTypeConfiguration<SummaryCacheEntry>
{
    public void Configure(EntityTypeBuilder<SummaryCacheEntry> builder)
    {
        builder.ToTable("AISummaryCacheEntries");

        builder.HasKey(x => x.CacheKey);

        builder.Property(x => x.CacheKey)
            .HasMaxLength(256)
            .ValueGeneratedNever();

        builder.Property(x => x.Summary)
            .HasMaxLength(16000)
            .IsRequired();

        builder.Property(x => x.KeyPoints)
            .HasConversion(
                v => AIModelConversion.ToStringListDb(v),
                v => AIModelConversion.ToStringListDomain(v))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.DetectedLanguage)
            .HasMaxLength(16);

        builder.Property(x => x.OriginalLength)
            .IsRequired();

        builder.Property(x => x.SummaryLength)
            .IsRequired();

        builder.Property(x => x.CompressionRatio)
            .IsRequired();

        builder.Property(x => x.TokensUsed);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.ExpiresAt);

        builder.Property(x => x.AccessCount)
            .IsRequired();

        builder.Property(x => x.LastAccessedAt);

        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.ExpiresAt);
    }
}
