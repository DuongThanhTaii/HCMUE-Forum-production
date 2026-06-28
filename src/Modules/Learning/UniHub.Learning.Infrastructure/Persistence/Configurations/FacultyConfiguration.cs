using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Learning.Domain.Faculties;

namespace UniHub.Learning.Infrastructure.Persistence.Configurations;

public class FacultyConfiguration : IEntityTypeConfiguration<Faculty>
{
    public void Configure(EntityTypeBuilder<Faculty> builder)
    {
        builder.ToTable("faculties", "learning");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id)
            .HasConversion(
                id => id.Value,
                value => FacultyId.Create(value))
            .HasColumnName("id");

        // Owned: FacultyCode (sealed record)
        builder.OwnsOne(f => f.Code, code =>
        {
            code.Property(x => x.Value)
                .HasColumnName("code")
                .HasMaxLength(20)
                .IsRequired();

            code.HasIndex(x => x.Value).IsUnique();
        });

        // Owned: FacultyName (sealed record)
        builder.OwnsOne(f => f.Name, name =>
        {
            name.Property(x => x.Value)
                .HasColumnName("name")
                .HasMaxLength(200)
                .IsRequired();

            name.HasIndex(x => x.Value).IsUnique();
        });

        // Owned: FacultyDescription (sealed record)
        builder.OwnsOne(f => f.Description, description =>
        {
            description.Property(x => x.Value)
                .HasColumnName("description")
                .HasMaxLength(2000)
                .IsRequired();
        });

        // Enum: FacultyStatus
        builder.Property(f => f.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Simple properties
        builder.Property(f => f.ManagerId)
            .HasColumnName("manager_id");

        builder.Property(f => f.CourseCount)
            .HasColumnName("course_count")
            .HasDefaultValue(0);

        builder.Property(f => f.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(f => f.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(f => f.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        // Indexes
        builder.HasIndex(f => f.ManagerId);
        builder.HasIndex(f => f.Status);
        builder.HasIndex(f => f.CreatedAt);

        builder.Ignore(f => f.DomainEvents);
    }
}
