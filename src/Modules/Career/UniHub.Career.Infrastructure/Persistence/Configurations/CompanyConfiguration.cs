using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Career.Domain.Companies;

namespace UniHub.Career.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("companies", "career");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CompanyId.Create(value))
            .HasColumnName("id");

        // Simple properties
        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasColumnName("description")
            .HasMaxLength(5000)
            .IsRequired();

        builder.Property(c => c.Industry)
            .HasColumnName("industry")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Size)
            .HasColumnName("size")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.Website)
            .HasColumnName("website")
            .HasMaxLength(500);

        builder.Property(c => c.LogoUrl)
            .HasColumnName("logo_url")
            .HasMaxLength(1000);

        builder.Property(c => c.FoundedYear)
            .HasColumnName("founded_year");

        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(c => c.RegisteredBy)
            .HasColumnName("registered_by")
            .IsRequired();

        builder.Property(c => c.RegisteredAt)
            .HasColumnName("registered_at")
            .IsRequired();

        builder.Property(c => c.VerifiedAt)
            .HasColumnName("verified_at");

        builder.Property(c => c.VerifiedBy)
            .HasColumnName("verified_by");

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.TotalJobPostings)
            .HasColumnName("total_job_postings")
            .HasDefaultValue(0);

        // Owned: ContactInfo
        builder.OwnsOne(c => c.ContactInfo, contact =>
        {
            contact.Property(ci => ci.Email)
                .HasColumnName("contact_email")
                .HasMaxLength(256)
                .IsRequired();

            contact.Property(ci => ci.Phone)
                .HasColumnName("contact_phone")
                .HasMaxLength(20);

            contact.Property(ci => ci.Address)
                .HasColumnName("contact_address")
                .HasMaxLength(500);
        });

        // Owned: SocialLinks
        builder.OwnsOne(c => c.SocialLinks, social =>
        {
            social.Property(sl => sl.LinkedIn)
                .HasColumnName("social_linkedin")
                .HasMaxLength(500);

            social.Property(sl => sl.Facebook)
                .HasColumnName("social_facebook")
                .HasMaxLength(500);

            social.Property(sl => sl.Twitter)
                .HasColumnName("social_twitter")
                .HasMaxLength(500);

            social.Property(sl => sl.Instagram)
                .HasColumnName("social_instagram")
                .HasMaxLength(500);

            social.Property(sl => sl.YouTube)
                .HasColumnName("social_youtube")
                .HasMaxLength(500);
        });

        // Primitive collection: Benefits (List<string>) - stored as JSON
        builder.Property("_benefits")
            .HasColumnName("benefits")
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(c => c.Name);
        builder.HasIndex(c => c.Industry);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.RegisteredBy);
        builder.HasIndex(c => c.RegisteredAt);

        builder.Ignore(c => c.DomainEvents);
    }
}
