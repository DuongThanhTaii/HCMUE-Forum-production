using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Forum.Domain.Categories;
using UniHub.Forum.Domain.Categories.ValueObjects;

namespace UniHub.Forum.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("categories", "forum");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id)
            .HasConversion(
                id => id.Value,
                value => CategoryId.Create(value))
            .HasColumnName("id");

        // Owned: CategoryName
        builder.OwnsOne(c => c.Name, name =>
        {
            name.Property(n => n.Value)
                .HasColumnName("name")
                .HasMaxLength(100)
                .IsRequired();

            name.HasIndex(n => n.Value).IsUnique();
        });

        // Owned: CategoryDescription
        builder.OwnsOne(c => c.Description, description =>
        {
            description.Property(d => d.Value)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();
        });

        // Owned: Slug
        builder.OwnsOne(c => c.Slug, slug =>
        {
            slug.Property(s => s.Value)
                .HasColumnName("slug")
                .HasMaxLength(250)
                .IsRequired();

            slug.HasIndex(s => s.Value).IsUnique();
        });

        // Self-referencing foreign key (nullable)
        builder.Property(c => c.ParentCategoryId)
            .HasConversion(
                id => id!.Value,
                value => CategoryId.Create(value))
            .HasColumnName("parent_category_id");

        // Simple properties
        builder.Property(c => c.PostCount)
            .HasColumnName("post_count")
            .HasDefaultValue(0);

        builder.Property(c => c.DisplayOrder)
            .HasColumnName("display_order")
            .HasDefaultValue(0);

        builder.Property(c => c.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(c => c.UpdatedAt)
            .HasColumnName("updated_at");

        // Primitive collection: ModeratorIds (List<Guid>) - stored as JSON
        builder.Property("_moderatorIds")
            .HasColumnName("moderator_ids")
            .HasColumnType("jsonb");

        // Indexes
        builder.HasIndex(c => c.ParentCategoryId);
        builder.HasIndex(c => c.DisplayOrder);
        builder.HasIndex(c => c.IsActive);

        builder.Ignore(c => c.DomainEvents);
    }
}
