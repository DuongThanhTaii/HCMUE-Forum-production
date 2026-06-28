using Microsoft.EntityFrameworkCore;
using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Persistence;

public sealed class AIDbContext : DbContext
{
    public AIDbContext(DbContextOptions<AIDbContext> options)
        : base(options)
    {
    }

    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<FAQItem> FAQItems => Set<FAQItem>();
    public DbSet<SearchHistory> SearchHistories => Set<SearchHistory>();
    public DbSet<SummaryCacheEntry> SummaryCacheEntries => Set<SummaryCacheEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AIDbContext).Assembly);
    }
}
