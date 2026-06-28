using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Notification.Domain.NotificationPreferences;

namespace UniHub.Notification.Infrastructure.Persistence.Configurations;

public class NotificationPreferenceConfiguration : IEntityTypeConfiguration<NotificationPreference>
{
    public void Configure(EntityTypeBuilder<NotificationPreference> builder)
    {
        builder.ToTable("notification_preferences", "notification");

        builder.HasKey(np => np.Id);
        builder.Property(np => np.Id)
            .HasConversion(
                id => id.Value,
                value => NotificationPreferenceId.Create(value))
            .HasColumnName("id");

        // Simple properties
        builder.Property(np => np.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(np => np.EmailEnabled)
            .HasColumnName("email_enabled")
            .HasDefaultValue(true);

        builder.Property(np => np.PushEnabled)
            .HasColumnName("push_enabled")
            .HasDefaultValue(true);

        builder.Property(np => np.InAppEnabled)
            .HasColumnName("in_app_enabled")
            .HasDefaultValue(true);

        builder.Property(np => np.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(np => np.UpdatedAt)
            .HasColumnName("updated_at");

        // Unique constraint: one preference record per user
        builder.HasIndex(np => np.UserId).IsUnique();

        builder.Ignore(np => np.DomainEvents);
    }
}
