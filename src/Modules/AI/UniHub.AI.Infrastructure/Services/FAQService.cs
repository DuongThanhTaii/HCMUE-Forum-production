using UniHub.AI.Application.Services;
using UniHub.AI.Domain.Entities;
using UniHub.AI.Infrastructure.Repositories;

namespace UniHub.AI.Infrastructure.Services;

/// <summary>
/// Implementation of FAQ service
/// </summary>
public class FAQService : IFAQService
{
    private readonly IFAQRepository _repository;

    public FAQService(IFAQRepository repository)
    {
        _repository = repository;
    }

    public Task<List<FAQItem>> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        return _repository.SearchAsync(query, maxResults, cancellationToken);
    }

    public Task<FAQItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.GetByIdAsync(id, cancellationToken);
    }

    public Task<List<FAQItem>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        return _repository.GetByCategoryAsync(category, cancellationToken);
    }

    public Task<FAQItem> CreateAsync(FAQItem faqItem, CancellationToken cancellationToken = default)
    {
        return _repository.CreateAsync(faqItem, cancellationToken);
    }

    public Task<FAQItem> UpdateAsync(FAQItem faqItem, CancellationToken cancellationToken = default)
    {
        return _repository.UpdateAsync(faqItem, cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return _repository.DeleteAsync(id, cancellationToken);
    }

    public async Task IncrementUsageCountAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var faq = await _repository.GetByIdAsync(id, cancellationToken);
        if (faq != null)
        {
            faq.UsageCount++;
            await _repository.UpdateAsync(faq, cancellationToken);
        }
    }

    public async Task UpdateRatingAsync(Guid id, int rating, CancellationToken cancellationToken = default)
    {
        var faq = await _repository.GetByIdAsync(id, cancellationToken);
        if (faq != null)
        {
            // Simple average calculation (can be improved with separate rating tracking)
            if (faq.AverageRating.HasValue)
            {
                faq.AverageRating = (faq.AverageRating.Value + rating) / 2.0;
            }
            else
            {
                faq.AverageRating = rating;
            }
            await _repository.UpdateAsync(faq, cancellationToken);
        }
    }
}
