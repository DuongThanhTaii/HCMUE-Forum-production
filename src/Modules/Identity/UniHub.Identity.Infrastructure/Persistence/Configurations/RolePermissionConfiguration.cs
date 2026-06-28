using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Identity.Domain.Roles;
using UniHub.Identity.Domain.Permissions;

namespace UniHub.Identity.Infrastructure.Persistence.Configurations;

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("role_permissions", "identity");

        builder.HasKey(rp => rp.Id);
        builder.Property(rp => rp.Id)
            .HasColumnName("id");

        builder.Property(rp => rp.RoleId)
            .HasConversion(
                id => id.Value,
                value => RoleId.Create(value))
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(rp => rp.PermissionId)
            .HasConversion(
                id => id.Value,
                value => PermissionId.Create(value))
            .HasColumnName("permission_id")
            .IsRequired();

        // Owned: PermissionScope
        builder.OwnsOne(rp => rp.Scope, scope =>
        {
            scope.Property(s => s.Type)
                .HasColumnName("scope_type")
                .HasConversion<int>()
                .IsRequired();

            scope.Property(s => s.Value)
                .HasColumnName("scope_value")
                .HasMaxLength(100);
        });

        builder.Property(rp => rp.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
    }
}
