using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UniHub.Notification.Domain.NotificationTemplates;

namespace UniHub.Notification.Infrastructure.Persistence.Configurations;

public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
{
    public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
    {
        builder.ToTable("notification_templates", "notification");

        builder.HasKey(nt => nt.Id);
        builder.Property(nt => nt.Id)
            .HasConversion(
                id => id.Value,
                value => NotificationTemplateId.Create(value))
            .HasColumnName("id");

        // Simple properties
        builder.Property(nt => nt.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(nt => nt.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(nt => nt.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        builder.Property(nt => nt.Category)
            .HasColumnName("category")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(nt => nt.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(nt => nt.CreatedBy)
            .HasColumnName("created_by")
            .IsRequired();

        builder.Property(nt => nt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(nt => nt.UpdatedBy)
            .HasColumnName("updated_by");

        builder.Property(nt => nt.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(nt => nt.ActivatedAt)
            .HasColumnName("activated_at");

        builder.Property(nt => nt.ActivatedBy)
            .HasColumnName("activated_by");

        // Owned: EmailTemplateContent (nullable)
        builder.OwnsOne(nt => nt.EmailContent, email =>
        {
            email.Property(e => e.Subject)
                .HasColumnName("email_subject")
                .HasMaxLength(200)
                .IsRequired();

            email.Property(e => e.Body)
                .HasColumnName("email_body")
                .HasMaxLength(50000)
                .IsRequired();

            email.Property(e => e.FromName)
                .HasColumnName("email_from_name")
                .HasMaxLength(100);

            email.Property(e => e.FromEmail)
                .HasColumnName("email_from_email")
                .HasMaxLength(256);
        });

        // Owned: PushTemplateContent (nullable)
        builder.OwnsOne(nt => nt.PushContent, push =>
        {
            push.Property(p => p.Title)
                .HasColumnName("push_title")
                .HasMaxLength(100)
                .IsRequired();

            push.Property(p => p.Body)
                .HasColumnName("push_body")
                .HasMaxLength(500)
                .IsRequired();

            push.Property(p => p.IconUrl)
                .HasColumnName("push_icon_url")
                .HasMaxLength(1000);

            push.Property(p => p.BadgeCount)
                .HasColumnName("push_badge_count");
        });

        // Owned: InAppTemplateContent (nullable)
        builder.OwnsOne(nt => nt.InAppContent, inApp =>
        {
            inApp.Property(i => i.Title)
                .HasColumnName("inapp_title")
                .HasMaxLength(200)
                .IsRequired();

            inApp.Property(i => i.Body)
                .HasColumnName("inapp_body")
                .HasMaxLength(1000)
                .IsRequired();

            inApp.Property(i => i.ActionUrl)
                .HasColumnName("inapp_action_url")
                .HasMaxLength(2000);

            inApp.Property(i => i.IconUrl)
                .HasColumnName("inapp_icon_url")
                .HasMaxLength(1000);
        });

        // Primitive collection: Channels (List<NotificationChannel> enum) - stored as JSON
        builder.Property("_channels")
            .HasColumnName("channels")
            .HasColumnType("jsonb");

        // Owned collection: Variables (TemplateVariable value objects)
        builder.OwnsMany(nt => nt.Variables, variable =>
        {
            variable.ToTable("notification_template_variables", "notification");

            variable.Property(v => v.Name)
                .HasColumnName("name")
                .HasMaxLength(50)
                .IsRequired();

            variable.Property(v => v.Description)
                .HasColumnName("description")
                .HasMaxLength(500)
                .IsRequired();

            variable.Property(v => v.ExampleValue)
                .HasColumnName("example_value")
                .HasMaxLength(200);

            variable.WithOwner().HasForeignKey("template_id");
            variable.HasKey("template_id", nameof(TemplateVariable.Name));
        });

        // Unique constraint: template name must be unique
        builder.HasIndex(nt => nt.Name).IsUnique();

        // Indexes
        builder.HasIndex(nt => nt.Category);
        builder.HasIndex(nt => nt.Status);
        builder.HasIndex(nt => nt.CreatedAt);

        builder.Ignore(nt => nt.DomainEvents);
    }
}
