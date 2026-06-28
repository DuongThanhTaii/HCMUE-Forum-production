using UniHub.AI.Domain.Entities;

namespace UniHub.AI.Infrastructure.Repositories;

/// <summary>
/// In-memory implementation of FAQ repository
/// </summary>
public class InMemoryFAQRepository : IFAQRepository
{
    private readonly List<FAQItem> _faqs = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public InMemoryFAQRepository()
    {
        // Seed with some default FAQs
        SeedData();
    }

    private void SeedData()
    {
        var defaultFAQs = new List<FAQItem>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Question = "What are the admission requirements for HCMUE?",
                Answer = "Admission requirements include: (1) High school diploma or equivalent, (2) Entrance exam scores, (3) Required documents (ID, photos, certificates). Visit our website or contact the admissions office for detailed information.",
                Category = "Admission",
                Tags = ["admission", "requirements", "entrance"],
                Priority = 10,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Question = "How do I register for courses?",
                Answer = "Course registration is done online through the student portal. Log in with your student ID and password, select your courses, and submit the registration form. Registration periods are announced via email and the university website.",
                Category = "Registration",
                Tags = ["registration", "courses", "enrollment"],
                Priority = 9,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Question = "Where is the library located?",
                Answer = "The main library is located in Building A, Ground Floor. It's open Monday-Friday 7:00 AM - 8:00 PM, and Saturday 8:00 AM - 5:00 PM. Students need their student ID card to access library services.",
                Category = "Campus Life",
                Tags = ["library", "location", "hours"],
                Priority = 5,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Question = "How can I contact technical support?",
                Answer = "Technical support is available via: (1) Email: itsupport@hcmue.edu.vn, (2) Phone: (028) 3835-9999, (3) In-person: IT Center, Building B, Room 101. Office hours: Monday-Friday 8:00 AM - 5:00 PM.",
                Category = "Support",
                Tags = ["support", "technical", "IT", "contact"],
                Priority = 8,
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Question = "What scholarships are available?",
                Answer = "HCMUE offers several scholarships including: (1) Merit-based scholarships (GPA ≥ 3.5), (2) Need-based financial aid, (3) Research scholarships for outstanding students. Apply through the student portal during scholarship periods.",
                Category = "Financial Aid",
                Tags = ["scholarship", "financial aid", "funding"],
                Priority = 7,
                IsActive = true
            }
        };

        _faqs.AddRange(defaultFAQs);
    }

    public async Task<FAQItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _faqs.FirstOrDefault(f => f.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<FAQItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _faqs.Where(f => f.IsActive).OrderByDescending(f => f.Priority).ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<List<FAQItem>> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            query = query.ToLowerInvariant();
            
            return _faqs
                .Where(f => f.IsActive)
                .Select(f => new
                {
                    FAQ = f,
                    Score = CalculateRelevanceScore(f, query)
                })
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.FAQ.Priority)
                .Take(maxResults)
                .Select(x => x.FAQ)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    private int CalculateRelevanceScore(FAQItem faq, string query)
    {
        int score = 0;
        var queryWords = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in queryWords)
        {
            if (faq.Question.Contains(word, StringComparison.OrdinalIgnoreCase))
                score += 10;
            if (faq.Answer.Contains(word, StringComparison.OrdinalIgnoreCase))
                score += 5;
            if (faq.Tags.Any(t => t.Contains(word, StringComparison.OrdinalIgnoreCase)))
                score += 8;
            if (faq.Category.Contains(word, StringComparison.OrdinalIgnoreCase))
                score += 7;
        }

        return score;
    }

    public async Task<List<FAQItem>> GetByCategoryAsync(string category, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            return _faqs
                .Where(f => f.IsActive && f.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Priority)
                .ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<FAQItem> CreateAsync(FAQItem faqItem, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            faqItem.Id = Guid.NewGuid();
            faqItem.CreatedAt = DateTime.UtcNow;
            faqItem.UpdatedAt = DateTime.UtcNow;
            _faqs.Add(faqItem);
            return faqItem;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<FAQItem> UpdateAsync(FAQItem faqItem, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var existing = _faqs.FirstOrDefault(f => f.Id == faqItem.Id);
            if (existing != null)
            {
                _faqs.Remove(existing);
                faqItem.UpdatedAt = DateTime.UtcNow;
                _faqs.Add(faqItem);
            }
            return faqItem;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var faq = _faqs.FirstOrDefault(f => f.Id == id);
            if (faq != null)
            {
                _faqs.Remove(faq);
                return true;
            }
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }
}
