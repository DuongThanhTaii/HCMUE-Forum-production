using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Career.Domain.Applications;
using UniHub.Career.Domain.JobPostings;
using ApplicationEntity = UniHub.Career.Domain.Applications.Application;

namespace UniHub.Career.Infrastructure.Persistence.Configurations;

public class ApplicationConfiguration : IEntityTypeConfiguration<ApplicationEntity>
{
    public void Configure(EntityTypeBuilder<ApplicationEntity> builder)
    {
        builder.ToTable("applications", "career");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(
                id => id.Value,
                value => UniHub.Career.Domain.Applications.ApplicationId.Create(value))
            .HasColumnName("id");

        // Strongly-typed foreign key: JobPostingId
        builder.Property(a => a.JobPostingId)
            .HasConversion(
                id => id.Value,
                value => JobPostingId.Create(value))
            .HasColumnName("job_posting_id")
            .IsRequired();

        // Simple properties
        builder.Property(a => a.ApplicantId)
            .HasColumnName("applicant_id")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(a => a.SubmittedAt)
            .HasColumnName("submitted_at")
            .IsRequired();

        builder.Property(a => a.LastStatusChangedAt)
            .HasColumnName("last_status_changed_at");

        builder.Property(a => a.LastStatusChangedBy)
            .HasColumnName("last_status_changed_by");

        builder.Property(a => a.ReviewNotes)
            .HasColumnName("review_notes")
            .HasMaxLength(2000);

        // Owned: Resume (non-nullable)
        builder.OwnsOne(a => a.Resume, resume =>
        {
            resume.Property(r => r.FileName)
                .HasColumnName("resume_file_name")
                .HasMaxLength(255)
                .IsRequired();

            resume.Property(r => r.FileUrl)
                .HasColumnName("resume_file_url")
                .HasMaxLength(2000)
                .IsRequired();

            resume.Property(r => r.FileSizeBytes)
                .HasColumnName("resume_file_size_bytes")
                .IsRequired();

            resume.Property(r => r.ContentType)
                .HasColumnName("resume_content_type")
                .HasMaxLength(100)
                .IsRequired();
        });

        // Owned: CoverLetter (nullable)
        builder.OwnsOne(a => a.CoverLetter, coverLetter =>
        {
            coverLetter.Property(cl => cl.Content)
                .HasColumnName("cover_letter_content")
                .HasMaxLength(5000)
                .IsRequired();
        });

        // Indexes
        builder.HasIndex(a => a.JobPostingId);
        builder.HasIndex(a => a.ApplicantId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.SubmittedAt);

        // Unique constraint: one application per user per job posting
        builder.HasIndex(a => new { a.JobPostingId, a.ApplicantId }).IsUnique();

        builder.Ignore(a => a.DomainEvents);
    }
}
