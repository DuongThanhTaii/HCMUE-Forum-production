using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Persistence.Configurations;

internal sealed class SearchHistoryConfiguration : IEntityTypeConfiguration<SearchHistory>
{
    public void Configure(EntityTypeBuilder<SearchHistory> builder)
    {
        builder.ToTable("AISearchHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasMaxLength(64)
            .ValueGeneratedNever();

        builder.Property(x => x.UserId)
            .HasMaxLength(64);

        builder.Property(x => x.Query)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.NormalizedQuery)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.SearchType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ResultCount)
            .IsRequired();

        builder.Property(x => x.HadClickthrough)
            .IsRequired();

        builder.Property(x => x.ClickedResultId)
            .HasMaxLength(200);

        builder.Property(x => x.ProcessingTimeMs)
            .IsRequired();

        builder.Property(x => x.SearchedAt)
            .IsRequired();

        builder.Property(x => x.SessionId)
            .HasMaxLength(200);

        builder.Property(x => x.Language)
            .HasMaxLength(16)
            .IsRequired();

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.NormalizedQuery);
        builder.HasIndex(x => x.SearchedAt);
        builder.HasIndex(x => x.SearchType);
    }
}
