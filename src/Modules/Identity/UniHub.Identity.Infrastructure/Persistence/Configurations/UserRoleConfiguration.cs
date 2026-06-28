using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Roles;

namespace UniHub.Identity.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("user_roles", "identity");

        builder.HasKey(ur => ur.Id);
        builder.Property(ur => ur.Id)
            .HasColumnName("id");

        builder.Property(ur => ur.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(ur => ur.RoleId)
            .HasConversion(
                id => id.Value,
                value => RoleId.Create(value))
            .HasColumnName("role_id")
            .IsRequired();

        builder.Property(ur => ur.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
    }
}
