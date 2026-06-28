using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Application.Services;

/// <summary>
/// Service for managing FAQ knowledge base
/// </summary>
public interface IFAQService
{
    /// <summary>
    /// Search for relevant FAQ items based on query
    /// </summary>
    Task<List<FAQItem>> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get FAQ by ID
    /// </summary>
    Task<FAQItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Get FAQ by category
    /// </summary>
    Task<List<FAQItem>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Create new FAQ
    /// </summary>
    Task<FAQItem> CreateAsync(FAQItem faqItem, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update existing FAQ
    /// </summary>
    Task<FAQItem> UpdateAsync(FAQItem faqItem, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Delete FAQ
    /// </summary>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Increment usage count for FAQ
    /// </summary>
    Task IncrementUsageCountAsync(Guid id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Update rating for FAQ
    /// </summary>
    Task UpdateRatingAsync(Guid id, int rating, CancellationToken cancellationToken = default);
}
