using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Learning.Domain.Documents;

namespace UniHub.Learning.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents", "learning");

        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasConversion(
                id => id.Value,
                value => DocumentId.Create(value))
            .HasColumnName("id");

        // Owned: DocumentTitle (sealed record)
        builder.OwnsOne(d => d.Title, title =>
        {
            title.Property(x => x.Value)
                .HasColumnName("title")
                .HasMaxLength(200)
                .IsRequired();
        });

        // Owned: DocumentDescription (sealed record)
        builder.OwnsOne(d => d.Description, description =>
        {
            description.Property(x => x.Value)
                .HasColumnName("description")
                .HasMaxLength(1000)
                .IsRequired();
        });

        // Owned: DocumentFile (sealed record with multiple properties)
        builder.OwnsOne(d => d.File, file =>
        {
            file.Property(f => f.FileName)
                .HasColumnName("file_name")
                .HasMaxLength(255)
                .IsRequired();

            file.Property(f => f.FilePath)
                .HasColumnName("file_path")
                .HasMaxLength(500)
                .IsRequired();

            file.Property(f => f.FileSize)
                .HasColumnName("file_size")
                .IsRequired();

            file.Property(f => f.ContentType)
                .HasColumnName("content_type")
                .HasMaxLength(100)
                .IsRequired();

            file.Property(f => f.FileExtension)
                .HasColumnName("file_extension")
                .HasMaxLength(10);
        });

        // Enums
        builder.Property(d => d.Type)
            .HasColumnName("type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(d => d.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Simple properties
        builder.Property(d => d.UploaderId)
            .HasColumnName("uploader_id")
            .IsRequired();

        builder.Property(d => d.CourseId)
            .HasColumnName("course_id");

        builder.Property(d => d.ReviewerId)
            .HasColumnName("reviewer_id");

        builder.Property(d => d.ReviewComment)
            .HasColumnName("review_comment")
            .HasMaxLength(1000);

        builder.Property(d => d.RejectionReason)
            .HasColumnName("rejection_reason")
            .HasMaxLength(500);

        builder.Property(d => d.DownloadCount)
            .HasColumnName("download_count")
            .HasDefaultValue(0);

        builder.Property(d => d.ViewCount)
            .HasColumnName("view_count")
            .HasDefaultValue(0);

        builder.Property(d => d.AverageRating)
            .HasColumnName("average_rating")
            .HasPrecision(3, 2)
            .HasDefaultValue(0.0);

        builder.Property(d => d.RatingCount)
            .HasColumnName("rating_count")
            .HasDefaultValue(0);

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(d => d.SubmittedAt)
            .HasColumnName("submitted_at");

        builder.Property(d => d.ReviewedAt)
            .HasColumnName("reviewed_at");

        // Indexes
        builder.HasIndex(d => d.UploaderId);
        builder.HasIndex(d => d.CourseId);
        builder.HasIndex(d => d.Status);
        builder.HasIndex(d => d.Type);
        builder.HasIndex(d => d.CreatedAt);

        builder.Ignore(d => d.DomainEvents);
    }
}
