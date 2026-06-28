using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.JobPostings;

namespace UniHub.Career.Infrastructure.Persistence.Configurations;

public class JobPostingConfiguration : IEntityTypeConfiguration<JobPosting>
{
    public void Configure(EntityTypeBuilder<JobPosting> builder)
    {
        builder.ToTable("job_postings", "career");

        builder.HasKey(j => j.Id);
        builder.Property(j => j.Id)
            .HasConversion(
                id => id.Value,
                value => JobPostingId.Create(value))
            .HasColumnName("id");

        // Simple properties
        builder.Property(j => j.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(j => j.Description)
            .HasColumnName("description")
            .HasMaxLength(10000)
            .IsRequired();

        // CompanyId is raw Guid (not strongly-typed in JobPosting)
        builder.Property(j => j.CompanyId)
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(j => j.PostedBy)
            .HasColumnName("posted_by")
            .IsRequired();

        builder.Property(j => j.JobType)
            .HasColumnName("job_type")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(j => j.ExperienceLevel)
            .HasColumnName("experience_level")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(j => j.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(j => j.Deadline)
            .HasColumnName("deadline");

        builder.Property(j => j.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(j => j.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(j => j.PublishedAt)
            .HasColumnName("published_at");

        builder.Property(j => j.ClosedAt)
            .HasColumnName("closed_at");

        builder.Property(j => j.ViewCount)
            .HasColumnName("view_count")
            .HasDefaultValue(0);

        builder.Property(j => j.ApplicationCount)
            .HasColumnName("application_count")
            .HasDefaultValue(0);

        // Owned: SalaryRange (nullable)
        builder.OwnsOne(j => j.Salary, salary =>
        {
            salary.Property(s => s.MinAmount)
                .HasColumnName("salary_min_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            salary.Property(s => s.MaxAmount)
                .HasColumnName("salary_max_amount")
                .HasPrecision(18, 2)
                .IsRequired();

            salary.Property(s => s.Currency)
                .HasColumnName("salary_currency")
                .HasMaxLength(10)
                .IsRequired();

            salary.Property(s => s.Period)
                .HasColumnName("salary_period")
                .HasMaxLength(20)
                .IsRequired();
        });

        // Owned: WorkLocation (non-nullable)
        builder.OwnsOne(j => j.Location, location =>
        {
            location.Property(l => l.City)
                .HasColumnName("location_city")
                .HasMaxLength(100)
                .IsRequired();

            location.Property(l => l.District)
                .HasColumnName("location_district")
                .HasMaxLength(100);

            location.Property(l => l.Address)
                .HasColumnName("location_address")
                .HasMaxLength(300);

            location.Property(l => l.IsRemote)
                .HasColumnName("location_is_remote")
                .HasDefaultValue(false);
        });

        // Owned collection: JobRequirements
        builder.OwnsMany(j => j.Requirements, requirement =>
        {
            requirement.ToTable("job_posting_requirements", "career");

            requirement.Property(r => r.Skill)
                .HasColumnName("skill")
                .HasMaxLength(100)
                .IsRequired();

            requirement.Property(r => r.IsRequired)
                .HasColumnName("is_required")
                .HasDefaultValue(true);

            requirement.WithOwner().HasForeignKey("job_posting_id");
            requirement.HasKey("job_posting_id", nameof(JobRequirement.Skill));
        });

        // Primitive collection: Tags (List<string>) - stored as JSON
        builder.Property("_tags")
            .HasColumnName("tags")
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(j => j.CompanyId);
        builder.HasIndex(j => j.PostedBy);
        builder.HasIndex(j => j.Status);
        builder.HasIndex(j => j.JobType);
        builder.HasIndex(j => j.ExperienceLevel);
        builder.HasIndex(j => j.CreatedAt);
        builder.HasIndex(j => j.Deadline);

        builder.Ignore(j => j.DomainEvents);
    }
}
