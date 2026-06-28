using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Repositories;

/// <summary>
/// Repository for FAQ operations
/// </summary>
public interface IFAQRepository
{
    Task<FAQItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FAQItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<FAQItem>> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
    Task<List<FAQItem>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    Task<FAQItem> CreateAsync(FAQItem faqItem, CancellationToken cancellationToken = default);
    Task<FAQItem> UpdateAsync(FAQItem faqItem, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
