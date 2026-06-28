using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using UniHub.Identity.Domain.Users;
using UniHub.Identity.Domain.Users.ValueObjects;

namespace UniHub.Identity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "identity");

        // Primary Key
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => UserId.Create(value))
            .HasColumnName("id");

        // Email — scalar column with value converter (avoids EF Core 10 shadow FK bug on OwnsOne).
        // Physical schema unchanged: single "email" column on identity.users.
        var emailConverter = new ValueConverter<Email, string>(
            email => email.Value,
            value => Email.Create(value).Value!);

        builder.Property(u => u.Email)
            .HasConversion(emailConverter)
            .HasColumnName("email")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("IX_users_email");

        // PasswordHash
        builder.Property(u => u.PasswordHash)
            .HasColumnName("password_hash")
            .HasMaxLength(500)
            .IsRequired();

        // Owned: UserProfile (multiple columns)
        builder.OwnsOne(u => u.Profile, profile =>
        {
            profile.Property(p => p.FirstName)
                .HasColumnName("first_name")
                .HasMaxLength(100)
                .IsRequired();

            profile.Property(p => p.LastName)
                .HasColumnName("last_name")
                .HasMaxLength(100)
                .IsRequired();

            profile.Property(p => p.Avatar)
                .HasColumnName("avatar")
                .HasMaxLength(1000);

            profile.Property(p => p.Bio)
                .HasColumnName("bio")
                .HasMaxLength(500);

            profile.Property(p => p.Phone)
                .HasColumnName("phone")
                .HasMaxLength(15);

            profile.Property(p => p.DateOfBirth)
                .HasColumnName("date_of_birth");
        });

        // Owned: OfficialBadge (nullable, multiple columns)
        builder.OwnsOne(u => u.Badge, badge =>
        {
            badge.Property(b => b.Type)
                .HasColumnName("badge_type")
                .HasConversion<int>();

            badge.Property(b => b.Name)
                .HasColumnName("badge_name")
                .HasMaxLength(100);

            badge.Property(b => b.Description)
                .HasColumnName("badge_description")
                .HasMaxLength(200);

            badge.Property(b => b.VerifiedAt)
                .HasColumnName("badge_verified_at");

            badge.Property(b => b.VerifiedBy)
                .HasColumnName("badge_verified_by")
                .HasMaxLength(200);
        });

        // Enum
        builder.Property(u => u.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        // Timestamps
        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at");

        // Navigation: Roles (one-to-many via backing field)
        builder.HasMany(u => u.Roles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.Roles)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Navigation: RefreshTokens (one-to-many via backing field)
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(u => u.RefreshTokens)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        // Ignore domain events
        builder.Ignore(u => u.DomainEvents);
    }
}
