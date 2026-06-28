using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Career.Domain.Companies;
using UniHub.Career.Domain.Recruiters;

namespace UniHub.Career.Infrastructure.Persistence.Configurations;

public class RecruiterConfiguration : IEntityTypeConfiguration<Recruiter>
{
    public void Configure(EntityTypeBuilder<Recruiter> builder)
    {
        builder.ToTable("recruiters", "career");

        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(
                id => id.Value,
                value => RecruiterId.Create(value))
            .HasColumnName("id");

        // Simple properties
        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        // Strongly-typed foreign key: CompanyId
        builder.Property(r => r.CompanyId)
            .HasConversion(
                id => id.Value,
                value => CompanyId.Create(value))
            .HasColumnName("company_id")
            .IsRequired();

        builder.Property(r => r.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(r => r.AddedBy)
            .HasColumnName("added_by")
            .IsRequired();

        builder.Property(r => r.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        builder.Property(r => r.LastModifiedAt)
            .HasColumnName("last_modified_at");

        // Owned: RecruiterPermissions
        builder.OwnsOne(r => r.Permissions, permissions =>
        {
            permissions.Property(p => p.CanManageJobPostings)
                .HasColumnName("can_manage_job_postings")
                .HasDefaultValue(false);

            permissions.Property(p => p.CanReviewApplications)
                .HasColumnName("can_review_applications")
                .HasDefaultValue(false);

            permissions.Property(p => p.CanUpdateApplicationStatus)
                .HasColumnName("can_update_application_status")
                .HasDefaultValue(false);

            permissions.Property(p => p.CanInviteRecruiters)
                .HasColumnName("can_invite_recruiters")
                .HasDefaultValue(false);
        });

        // Indexes
        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => r.CompanyId);
        builder.HasIndex(r => r.Status);

        // Unique constraint: one recruiter record per user per company
        builder.HasIndex(r => new { r.UserId, r.CompanyId }).IsUnique();

        builder.Ignore(r => r.DomainEvents);
    }
}
