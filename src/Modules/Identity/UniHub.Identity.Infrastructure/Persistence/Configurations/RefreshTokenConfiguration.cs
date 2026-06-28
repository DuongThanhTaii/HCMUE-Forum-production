using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Identity.Domain.Tokens;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", "identity");

        builder.HasKey(rt => rt.Id);
        builder.Property(rt => rt.Id)
            .HasConversion(
                id => id.Value,
                value => RefreshTokenId.Create(value))
            .HasColumnName("id");

        builder.Property(rt => rt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(rt => rt.Token).IsUnique();

        builder.Property(rt => rt.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(rt => rt.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(rt => rt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(rt => rt.CreatedByIp)
            .HasColumnName("created_by_ip")
            .HasMaxLength(50);

        builder.Property(rt => rt.RevokedAt)
            .HasColumnName("revoked_at");

        builder.Property(rt => rt.RevokedBy)
            .HasColumnName("revoked_by")
            .HasMaxLength(200);

        builder.Property(rt => rt.RevokedByIp)
            .HasColumnName("revoked_by_ip")
            .HasMaxLength(50);

        builder.Property(rt => rt.ReplacedByToken)
            .HasColumnName("replaced_by_token")
            .HasMaxLength(500);

        builder.Property(rt => rt.RevokeReason)
            .HasColumnName("revoke_reason")
            .HasMaxLength(500);

        // Computed properties - ignore
        builder.Ignore(rt => rt.IsExpired);
        builder.Ignore(rt => rt.IsActive);
        builder.Ignore(rt => rt.IsRevoked);
    }
}
