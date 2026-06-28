using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UniHub.AI.Infrastructure.Persistence;

public sealed class AIDbContextFactory : IDesignTimeDbContextFactory<AIDbContext>
{
    public AIDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AIDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("AI_DB_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=unihub_shadow;Username=unihub;Password=unihub_dev_2026";

        optionsBuilder.UseNpgsql(connectionString);

        return new AIDbContext(optionsBuilder.Options);
    }
}
