using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.Reports;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.ToTable("reports", "forum");

        // NOTE: ReportId uses int, not Guid
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => new ReportId(value))
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Polymorphic foreign key (can be Post or Comment)
        builder.Property(r => r.ReportedItemId)
            .HasColumnName("reported_item_id")
            .IsRequired();

        builder.Property(r => r.ReportedItemType)
            .HasColumnName("reported_item_type")
            .HasConversion<int>()
            .IsRequired();

        // Reporter
        builder.Property(r => r.ReporterId)
            .HasColumnName("reporter_id")
            .IsRequired();

        // Enums
        builder.Property(r => r.Reason)
            .HasColumnName("reason")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.ResolutionDecision)
            .HasColumnName("resolution_decision")
            .HasConversion<int?>();

        // Description (nullable)
        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        // Timestamps
        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.ReviewedAt)
            .HasColumnName("reviewed_at");

        // Reviewer (nullable)
        builder.Property(r => r.ReviewedBy)
            .HasColumnName("reviewed_by");

        // Indexes
        builder.HasIndex(r => r.ReportedItemId);
        builder.HasIndex(r => r.ReportedItemType);
        builder.HasIndex(r => r.ReporterId);
        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.CreatedAt);

        // Composite indexes for polymorphic queries
        builder.HasIndex(r => new { r.ReportedItemId, r.ReportedItemType });
    }
}
