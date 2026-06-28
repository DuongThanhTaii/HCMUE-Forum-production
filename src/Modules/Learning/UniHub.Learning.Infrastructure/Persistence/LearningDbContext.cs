using Microsoft.EntityFrameworkCore;
using UniHub.Learning.Domain.Courses;
using UniHub.Learning.Domain.Documents;
using UniHub.Learning.Domain.EventSourcing;
using UniHub.Learning.Domain.Faculties;

namespace UniHub.Learning.Infrastructure.Persistence;

public sealed class LearningDbContext : DbContext
{
    public LearningDbContext(DbContextOptions<LearningDbContext> options)
        : base(options)
    {
    }

    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Faculty> Faculties => Set<Faculty>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<StoredEvent> StoredEvents => Set<StoredEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LearningDbContext).Assembly);
    }
}
