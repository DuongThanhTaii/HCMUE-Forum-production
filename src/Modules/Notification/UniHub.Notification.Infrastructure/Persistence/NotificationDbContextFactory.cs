using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UniHub.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("NOTIFICATION_DB_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=unihub_shadow;Username=unihub;Password=unihub_dev_2026";

        optionsBuilder.UseNpgsql(connectionString);

        return new NotificationDbContext(optionsBuilder.Options);
    }
}
