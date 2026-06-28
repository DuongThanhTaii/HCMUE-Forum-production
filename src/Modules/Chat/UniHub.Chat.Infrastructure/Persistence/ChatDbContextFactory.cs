using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UniHub.Chat.Infrastructure.Persistence;

public sealed class ChatDbContextFactory : IDesignTimeDbContextFactory<ChatDbContext>
{
    public ChatDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ChatDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("CHAT_DB_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=unihub_shadow;Username=unihub;Password=unihub_dev_2026";

        optionsBuilder.UseNpgsql(connectionString);

        return new ChatDbContext(optionsBuilder.Options);
    }
}
