using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Identity.Domain.Users;

namespace UniHub.Identity.Infrastructure.Persistence.Configurations;

public sealed class UserBlockConfiguration : IEntityTypeConfiguration<UserBlock>
{
    public void Configure(EntityTypeBuilder<UserBlock> builder)
    {
        builder.ToTable("user_blocks", "identity");

        builder.HasKey(b => new { b.BlockerUserId, b.BlockedUserId });

        builder.Property(b => b.BlockerUserId).HasColumnName("blocker_user_id");
        builder.Property(b => b.BlockedUserId).HasColumnName("blocked_user_id");
        builder.Property(b => b.CreatedAt).HasColumnName("created_at");

        builder.HasIndex(b => b.BlockedUserId);
    }
}
