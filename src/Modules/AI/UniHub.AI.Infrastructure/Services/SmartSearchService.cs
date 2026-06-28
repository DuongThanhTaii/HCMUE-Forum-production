using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using UniHub.AI.Application.Abstractions;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;
using UniHub.AI.Domain.Entities;
using UniHub.AI.Infrastructure.Configuration;
using UniHub.AI.Infrastructure.Providers;

namespace UniHub.AI.Infrastructure.Services;

/// <summary>
/// Implementation of smart search service with AI-powered query understanding.
/// </summary>
public sealed class SmartSearchService : ISmartSearchService
{
    private readonly IAIProviderFactory _aiProviderFactory;
    private readonly SmartSearchSettings _settings;
    private readonly List<SearchHistory> _searchHistory; // In-memory for demo; use repository in production

    public SmartSearchService(
        IAIProviderFactory aiProviderFactory,
        IOptions<SmartSearchSettings> settings)
    {
        _aiProviderFactory = aiProviderFactory ?? throw new ArgumentNullException(nameof(aiProviderFactory));
        _settings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _searchHistory = new List<SearchHistory>();
    }

    /// <inheritdoc />
    public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Search query cannot be empty.", nameof(request));

        if (!_settings.IsEnabled)
            throw new InvalidOperationException("Smart search is disabled.");

        var stopwatch = Stopwatch.StartNew();

        // Validate and normalize inputs
        var query = request.Query.Trim();
        if (query.Length > _settings.MaxQueryLength)
            query = query.Substring(0, _settings.MaxQueryLength);

        var pageSize = Math.Min(request.PageSize, _settings.MaxPageSize);
        pageSize = Math.Max(1, pageSize);

        // Understand query with AI
        QueryUnderstanding? queryUnderstanding = null;
        if (_settings.EnableQueryUnderstanding)
        {
            try
            {
                queryUnderstanding = await UnderstandQueryAsync(query, cancellationToken);
            }
            catch
            {
                // If AI query understanding fails, continue with basic search
                queryUnderstanding = new QueryUnderstanding
                {
                    OriginalQuery = query,
                    ExpandedQuery = query,
                    Intent = "search",
                    Language = "en"
                };
            }
        }

        // For demo purposes, create mock search results
        // In production, this would query actual database/search index
        var allResults = await GenerateMockSearchResults(query, request.SearchType, cancellationToken);

        // Apply filters
        var filteredResults = ApplyFilters(allResults, request);

        // Calculate relevance scores and rank
        var rankedResults = await RankResultsAsync(query, filteredResults, cancellationToken);

        // Apply minimum relevance threshold
        rankedResults = rankedResults
            .Where(r => r.RelevanceScore >= request.MinRelevanceScore)
            .ToList();

        // Pagination
        var totalCount = rankedResults.Count;
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        var page = Math.Max(1, Math.Min(request.Page, totalPages > 0 ? totalPages : 1));

        var paginatedResults = rankedResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Generate suggestions
        var suggestions = request.IncludeSuggestions
            ? await GetSuggestionsAsync(query, _settings.DefaultSuggestionCount, cancellationToken)
            : new List<string>();

        stopwatch.Stop();

        // Track search history
        if (_settings.EnableSearchHistory)
        {
            TrackSearchHistory(request, totalCount, stopwatch.ElapsedMilliseconds);
        }

        return new SearchResponse
        {
            Results = paginatedResults,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            Suggestions = suggestions,
            QueryUnderstanding = queryUnderstanding,
            ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
            Timestamp = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public async Task<List<string>> GetSuggestionsAsync(string partialQuery, int limit = 5, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(partialQuery))
            return new List<string>();

        var suggestions = new List<string>();

        try
        {
            // Use AI to generate contextual suggestions
            var provider = await _aiProviderFactory.GetAvailableProviderAsync(cancellationToken);
            if (provider == null)
            {
                return GetBasicSuggestions(partialQuery, limit);
            }

            var prompt = $"Given the search query '{partialQuery}', suggest {limit} related or completed search queries that users might want to search for. " +
                         $"Return only the suggestions, one per line, without numbering or explanations.";

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                SystemMessage = "You are a helpful search assistant that suggests relevant search queries.",
                MaxTokens = 200,
                Temperature = 0.7
            };

            var response = await provider.SendChatRequestAsync(aiRequest, cancellationToken);

            if (response.IsSuccess && !string.IsNullOrWhiteSpace(response.Content))
            {
                suggestions = response.Content
                    .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim().TrimStart('-', '*', '•').Trim())
                    .Where(s => !string.IsNullOrWhiteSpace(s) && s.Length <= 100)
                    .Take(limit)
                    .ToList();
            }
        }
        catch
        {
            // Fallback to basic suggestions if AI fails
            suggestions = GetBasicSuggestions(partialQuery, limit);
        }

        // If no suggestions generated, return popular/recent searches
        if (!suggestions.Any())
        {
            suggestions = GetPopularSuggestions(limit);
        }

        return suggestions;
    }

    /// <inheritdoc />
    public async Task<QueryUnderstanding> UnderstandQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return new QueryUnderstanding
            {
                OriginalQuery = query,
                ExpandedQuery = query,
                Intent = "unknown",
                Language = "en"
            };
        }

        var normalizedQuery = query.Trim();
        var language = DetectLanguage(normalizedQuery);

        try
        {
            var provider = await _aiProviderFactory.GetAvailableProviderAsync(cancellationToken);
            if (provider == null)
            {
                return CreateBasicUnderstanding(normalizedQuery, language);
            }

            var prompt = BuildQueryUnderstandingPrompt(normalizedQuery, language);

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                SystemMessage = "You are an expert at understanding search queries and user intent. " +
                               "Analyze queries to expand them with synonyms, detect intent, and extract key entities.",
                MaxTokens = 300,
                Temperature = 0.3
            };

            var response = await provider.SendChatRequestAsync(aiRequest, cancellationToken);

            if (response.IsSuccess && !string.IsNullOrWhiteSpace(response.Content))
            {
                return ParseQueryUnderstanding(normalizedQuery, response.Content, language);
            }
        }
        catch
        {
            // If AI fails, return basic understanding
        }

        return CreateBasicUnderstanding(normalizedQuery, language);
    }

    /// <inheritdoc />
    public async Task<double> CalculateRelevanceAsync(string query, string content, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(query) || string.IsNullOrWhiteSpace(content))
            return 0.0;

        var queryTerms = TokenizeQuery(query);
        var contentLower = content.ToLowerInvariant();

        double score = 0.0;
        int matchCount = 0;

        // Calculate term frequency matches
        foreach (var term in queryTerms)
        {
            if (contentLower.Contains(term))
            {
                matchCount++;
                // Boost for exact phrase match
                if (contentLower.Contains(query.ToLowerInvariant()))
                {
                    score += _settings.Weights.ExactMatchBoost;
                }
            }
        }

        // Base score from term frequency
        if (queryTerms.Count > 0)
        {
            score += (double)matchCount / queryTerms.Count;
        }

        // Normalize to 0.0 - 1.0 range
        score = Math.Min(1.0, score);

        return await Task.FromResult(score);
    }

    // Private helper methods

    private async Task<List<SearchResult>> GenerateMockSearchResults(
        string query, SearchType searchType, CancellationToken cancellationToken)
    {
        // In production, this would query actual database/search engine
        // For now, generate mock results for demonstration
        var results = new List<SearchResult>();

        // Generate 20 mock results
        for (int i = 1; i <= 20; i++)
        {
            var contentTypeStr = searchType == SearchType.All
                ? GetRandomContentType(i)
                : searchType.ToString();

            results.Add(new SearchResult
            {
                Id = $"result-{i}",
                ContentType = contentTypeStr,
                Title = $"Result {i} matching '{query}'",
                Snippet = $"This is a snippet of content that matches your search query '{query}'. " +
                         $"It contains relevant information about the topic you're looking for...",
                RelevanceScore = 0.0, // Will be calculated during ranking
                Url = $"/content/{contentTypeStr.ToLower()}/{i}",
                Author = $"User{i % 5 + 1}",
                CreatedAt = DateTime.UtcNow.AddDays(-i),
                Category = GetRandomCategory(i),
                Tags = GetRandomTags(i),
                ViewCount = (20 - i) * 10 + new Random().Next(50)
            });
        }

        return await Task.FromResult(results);
    }

    private List<SearchResult> ApplyFilters(List<SearchResult> results, SearchRequest request)
    {
        var filtered = results.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            filtered = filtered.Where(r => r.Category == request.Category);
        }

        if (request.Tags?.Any() == true)
        {
            filtered = filtered.Where(r => r.Tags?.Any(t => request.Tags.Contains(t)) == true);
        }

        if (request.StartDate.HasValue)
        {
            filtered = filtered.Where(r => r.CreatedAt >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            filtered = filtered.Where(r => r.CreatedAt <= request.EndDate.Value);
        }

        return filtered.ToList();
    }

    private async Task<List<SearchResult>> RankResultsAsync(
        string query, List<SearchResult> results, CancellationToken cancellationToken)
    {
        var rankedResults = new List<SearchResult>();
        
        foreach (var result in results)
        {
            // Calculate relevance score
            var titleScore = await CalculateRelevanceAsync(query, result.Title, cancellationToken);
            var contentScore = await CalculateRelevanceAsync(query, result.Snippet, cancellationToken);
            
            var tagScore = 0.0;
            if (result.Tags?.Any() == true)
            {
                var queryTerms = TokenizeQuery(query);
                var matchingTags = result.Tags.Count(t => queryTerms.Contains(t.ToLowerInvariant()));
                tagScore = result.Tags.Count > 0 ? (double)matchingTags / result.Tags.Count : 0.0;
            }

            // Recency score (newer content gets higher score)
            var daysSinceCreation = (DateTime.UtcNow - result.CreatedAt).TotalDays;
            var recencyScore = Math.Max(0, 1.0 - (daysSinceCreation / 365.0)); // Decay over a year

            // Popularity boost
            var popularityMultiplier = result.ViewCount.HasValue && result.ViewCount > 100
                ? _settings.Weights.PopularityBoost
                : 1.0;

            // Weighted combination
            var baseScore = (titleScore * _settings.Weights.TitleWeight) +
                           (contentScore * _settings.Weights.ContentWeight) +
                           (tagScore * _settings.Weights.TagWeight) +
                           (recencyScore * _settings.Weights.RecencyWeight);

            var finalScore = Math.Min(1.0, baseScore * popularityMultiplier);
            
            // Create new SearchResult with updated relevance score
            rankedResults.Add(new SearchResult
            {
                Id = result.Id,
                ContentType = result.ContentType,
                Title = result.Title,
                Snippet = result.Snippet,
                RelevanceScore = finalScore,
                Url = result.Url,
                Author = result.Author,
                CreatedAt = result.CreatedAt,
                Category = result.Category,
                Tags = result.Tags,
                ViewCount = result.ViewCount,
                Metadata = result.Metadata
            });
        }

        // Sort by relevance score descending
        return rankedResults.OrderByDescending(r => r.RelevanceScore).ToList();
    }

    private void TrackSearchHistory(SearchRequest request, int resultCount, long processingTimeMs)
    {
        var history = new SearchHistory
        {
            UserId = request.UserId,
            Query = request.Query,
            NormalizedQuery = request.Query.ToLowerInvariant().Trim(),
            SearchType = request.SearchType.ToString(),
            ResultCount = resultCount,
            ProcessingTimeMs = processingTimeMs,
            SearchedAt = DateTime.UtcNow,
            Language = request.Language ?? "en"
        };

        _searchHistory.Add(history);

        // Keep only recent history (last 1000 searches)
        if (_searchHistory.Count > 1000)
        {
            _searchHistory.RemoveAt(0);
        }
    }

    private string BuildQueryUnderstandingPrompt(string query, string language)
    {
        var languageNote = language != "en" ? $" (query is in {language})" : "";
        
        return $"Analyze this search query{languageNote}: \"{query}\"\n\n" +
               "Provide the following in JSON format:\n" +
               "1. expandedQuery: Add synonyms and related terms to improve search\n" +
               "2. intent: Classify the intent (e.g., 'find_information', 'ask_question', 'find_person', 'find_document')\n" +
               "3. entities: Extract key entities (names, topics, dates)\n" +
               "4. suggestedCorrection: If the query has typos, suggest correction (null if none)\n\n" +
               "Example format:\n" +
               "{\n" +
               "  \"expandedQuery\": \"...\",\n" +
               "  \"intent\": \"...\",\n" +
               "  \"entities\": [\"...\"],\n" +
               "  \"suggestedCorrection\": null\n" +
               "}";
    }

    private QueryUnderstanding ParseQueryUnderstanding(string originalQuery, string aiResponse, string language)
    {
        try
        {
            // Try to parse JSON response
            var jsonMatch = Regex.Match(aiResponse, @"\{[\s\S]*\}");
            if (jsonMatch.Success)
            {
                var json = System.Text.Json.JsonDocument.Parse(jsonMatch.Value);
                var root = json.RootElement;

                return new QueryUnderstanding
                {
                    OriginalQuery = originalQuery,
                    ExpandedQuery = root.TryGetProperty("expandedQuery", out var exp) ? exp.GetString() ?? originalQuery : originalQuery,
                    Intent = root.TryGetProperty("intent", out var intent) ? intent.GetString() ?? "search" : "search",
                    Entities = root.TryGetProperty("entities", out var ent) 
                        ? ent.EnumerateArray().Select(e => e.GetString() ?? "").Where(s => !string.IsNullOrEmpty(s)).ToList()
                        : new List<string>(),
                    Language = language,
                    SuggestedCorrection = root.TryGetProperty("suggestedCorrection", out var corr) && corr.ValueKind != System.Text.Json.JsonValueKind.Null
                        ? corr.GetString()
                        : null
                };
            }
        }
        catch
        {
            // If parsing fails, return basic understanding
        }

        return CreateBasicUnderstanding(originalQuery, language);
    }

    private QueryUnderstanding CreateBasicUnderstanding(string query, string language)
    {
        return new QueryUnderstanding
        {
            OriginalQuery = query,
            ExpandedQuery = query,
            Intent = "search",
            Entities = ExtractBasicEntities(query),
            Language = language
        };
    }

    private List<string> ExtractBasicEntities(string query)
    {
        // Simple entity extraction: capitalized words and quoted phrases
        var entities = new List<string>();

        // Extract quoted phrases
        var quotedMatches = Regex.Matches(query, @"""([^""]+)""");
        foreach (Match match in quotedMatches)
        {
            if (match.Groups.Count > 1)
                entities.Add(match.Groups[1].Value);
        }

        // Extract capitalized words (potential proper nouns)
        var capitalizedMatches = Regex.Matches(query, @"\b[A-Z][a-z]+(?:\s+[A-Z][a-z]+)*\b");
        foreach (Match match in capitalizedMatches)
        {
            entities.Add(match.Value);
        }

        return entities.Distinct().ToList();
    }

    private string DetectLanguage(string text)
    {
        // Simple language detection based on character patterns
        if (Regex.IsMatch(text, @"[àáảãạăắằẳẵặâấầẩẫậđèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵ]"))
            return "vi";
        if (Regex.IsMatch(text, @"[\u4e00-\u9fff]"))
            return "zh";
        if (Regex.IsMatch(text, @"[\u3040-\u309f\u30a0-\u30ff]"))
            return "ja";
        if (Regex.IsMatch(text, @"[\uac00-\ud7af]"))
            return "ko";

        return "en";
    }

    private List<string> TokenizeQuery(string query)
    {
        // Remove special characters and split into terms
        var normalized = Regex.Replace(query.ToLowerInvariant(), @"[^\w\s]", " ");
        return normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(term => term.Length > 2) // Filter out very short terms
            .Distinct()
            .ToList();
    }

    private List<string> GetBasicSuggestions(string partialQuery, int limit)
    {
        var suggestions = new List<string>();
        var query = partialQuery.ToLowerInvariant();

        // Add query variations
        suggestions.Add($"{partialQuery} tutorial");
        suggestions.Add($"{partialQuery} guide");
        suggestions.Add($"how to {partialQuery}");
        suggestions.Add($"{partialQuery} examples");
        suggestions.Add($"best {partialQuery}");

        return suggestions.Take(limit).ToList();
    }

    private List<string> GetPopularSuggestions(int limit)
    {
        // Get most frequent searches from history
        if (!_searchHistory.Any())
        {
            return new List<string>
            {
                "programming tutorials",
                "web development",
                "data structures",
                "algorithms",
                "career advice"
            }.Take(limit).ToList();
        }

        var cutoffTime = DateTime.UtcNow.AddHours(-_settings.PopularSearchesWindowHours);
        
        return _searchHistory
            .Where(h => h.SearchedAt >= cutoffTime)
            .GroupBy(h => h.NormalizedQuery)
            .OrderByDescending(g => g.Count())
            .Take(limit)
            .Select(g => g.First().Query)
            .ToList();
    }

    private string GetRandomContentType(int seed)
    {
        var types = new[] { "Post", "Question", "Article", "Document", "FAQ" };
        return types[seed % types.Length];
    }

    private string GetRandomCategory(int seed)
    {
        var categories = new[] { "Technology", "Education", "Career", "General", "Resources" };
        return categories[seed % categories.Length];
    }

    private List<string> GetRandomTags(int seed)
    {
        var allTags = new[] { "programming", "web", "database", "career", "tutorial", "guide", "tips", "discussion" };
        var count = (seed % 3) + 1;
        return allTags.Skip(seed % 5).Take(count).ToList();
    }
}
