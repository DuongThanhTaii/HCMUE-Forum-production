using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UniHub.Career.Infrastructure.Persistence;

public sealed class CareerDbContextFactory : IDesignTimeDbContextFactory<CareerDbContext>
{
    public CareerDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<CareerDbContext>();

        var connectionString = Environment.GetEnvironmentVariable("CAREER_DB_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=unihub_shadow;Username=unihub;Password=unihub_dev_2026";

        optionsBuilder.UseNpgsql(connectionString);

        return new CareerDbContext(optionsBuilder.Options);
    }
}
