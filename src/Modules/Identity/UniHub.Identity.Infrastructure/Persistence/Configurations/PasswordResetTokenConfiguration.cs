using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens", "identity");

        builder.HasKey(prt => prt.Id);
        builder.Property(prt => prt.Id)
            .HasColumnName("id");

        builder.Property(prt => prt.UserId)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(prt => prt.Token)
            .HasColumnName("token")
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(prt => prt.Token).IsUnique();

        builder.Property(prt => prt.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(prt => prt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(prt => prt.IsUsed)
            .HasColumnName("is_used")
            .IsRequired();

        builder.Property(prt => prt.UsedAt)
            .HasColumnName("used_at");
    }
}
