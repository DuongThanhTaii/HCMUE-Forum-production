using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.Tags;
using UniHub.Forum.Domain.Tags.ValueObjects;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public class TagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("tags", "forum");

        // NOTE: TagId uses int, not Guid
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(
                id => id.Value,
                value => TagId.Create(value))
            .HasColumnName("id")
            .ValueGeneratedOnAdd();

        // Owned: TagName
        builder.OwnsOne(t => t.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            name.HasIndex(n => n.Value).IsUnique();
        });

        // Owned: TagDescription
        builder.OwnsOne(t => t.Description, description =>
        {
            description.Property(d => d.Value)
                .HasColumnName("description")
                .HasMaxLength(200)
                .IsRequired();
        });

        // Owned: Slug
        builder.OwnsOne(t => t.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .HasMaxLength(250)
                .IsRequired();

            slug.HasIndex(s => s.Value).IsUnique();
        });

        // Simple properties
        builder.Property(t => t.UsageCount)
            .HasColumnName("usage_count")
            .HasDefaultValue(0);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Indexes
        builder.HasIndex(t => t.UsageCount);
        builder.HasIndex(t => t.CreatedAt);

        builder.Ignore(t => t.DomainEvents);
    }
}
