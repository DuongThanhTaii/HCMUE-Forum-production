using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Persistence.Configurations;

internal sealed class FAQItemConfiguration : IEntityTypeConfiguration<FAQItem>
{
    public void Configure(EntityTypeBuilder<FAQItem> builder)
    {
        builder.ToTable("AIFAQItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Question)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(x => x.Answer)
            .HasMaxLength(12000)
            .IsRequired();

        builder.Property(x => x.Category)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Tags)
            .HasConversion(
                v => AIModelConversion.ToStringListDb(v),
                v => AIModelConversion.ToStringListDomain(v))
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(x => x.Priority)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.UsageCount)
            .IsRequired();

        builder.Property(x => x.AverageRating);

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();

        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.IsActive);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.CreatedAt);
    }
}
