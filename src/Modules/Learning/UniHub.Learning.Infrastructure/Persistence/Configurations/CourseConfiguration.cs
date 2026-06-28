using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Learning.Domain.Courses;

namespace UniHub.Learning.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("courses", "learning");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CourseId.Create(value))
            .HasColumnName("id");

        // Owned: CourseCode (sealed record)
        builder.OwnsOne(c => c.Code, code =>
        {
            code.Property(x => x.Value)
                .HasColumnName("code")
                .HasMaxLength(20)
                .IsRequired();

            code.HasIndex(x => x.Value).IsUnique();
        });

        // Owned: CourseName (sealed record)
        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();
        });

        // Owned: CourseDescription (sealed record)
        builder.OwnsOne(c => c.Description, description =>
        {
            description.Property(x => x.Value)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();
        });

        // Owned: Semester (sealed record)
        builder.OwnsOne(c => c.Semester, semester =>
        {
            semester.Property(x => x.Value)
                .HasColumnName("semester")
                .HasMaxLength(50)
                .IsRequired();
        });

        // Enum: CourseStatus
        builder.Property(c => c.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Simple properties
        builder.Property(c => c.FacultyId)
            .HasColumnName("faculty_id");

        builder.Property(c => c.Credits)
            .HasColumnName("credits")
            .IsRequired();

        builder.Property(c => c.DocumentCount)
            .HasColumnName("document_count")
            .HasDefaultValue(0);

        builder.Property(c => c.EnrollmentCount)
            .HasColumnName("enrollment_count")
            .HasDefaultValue(0);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(c => c.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        // Primitive collection: ModeratorIds (List<Guid>) - stored as JSON
        builder.Property("_moderatorIds")
            .HasColumnName("moderator_ids")
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(c => c.FacultyId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.CreatedAt);
        builder.HasIndex(c => c.CreatedBy);

        builder.Ignore(c => c.DomainEvents);
    }
}
