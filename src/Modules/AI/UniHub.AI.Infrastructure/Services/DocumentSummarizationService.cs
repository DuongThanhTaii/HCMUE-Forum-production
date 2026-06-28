using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UniHub.AI.Application.Abstractions;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;
using UniHub.AI.Domain.Entities;
using UniHub.AI.Infrastructure.Configuration;
using UniHub.AI.Infrastructure.Providers;
using UniHub.AI.Infrastructure.Repositories;

namespace UniHub.AI.Infrastructure.Services;

/// <summary>
/// Document summarization service using AI with caching
/// </summary>
public class DocumentSummarizationService : IDocumentSummarizationService
{
    private readonly IAIProviderFactory _aiProviderFactory;
    private readonly ISummaryCacheRepository _cacheRepository;
    private readonly SummarizationSettings _settings;

    public DocumentSummarizationService(
        IAIProviderFactory aiProviderFactory,
        ISummaryCacheRepository cacheRepository,
        IOptions<SummarizationSettings> settings)
    {
        _aiProviderFactory = aiProviderFactory;
        _cacheRepository = cacheRepository;
        _settings = settings.Value;
    }

    public async Task<SummarizationResponse> SummarizeAsync(SummarizationRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (!_settings.IsEnabled)
            {
                return new SummarizationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Summarization service is disabled"
                };
            }

            // Validate content length
            if (string.IsNullOrWhiteSpace(request.Content))
            {
                return new SummarizationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "Content cannot be empty"
                };
            }

            if (request.Content.Length < _settings.MinDocumentLength)
            {
                return new SummarizationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Content too short (minimum {_settings.MinDocumentLength} characters)"
                };
            }

            if (request.Content.Length > _settings.MaxDocumentLength)
            {
                return new SummarizationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = $"Content too long (maximum {_settings.MaxDocumentLength} characters)"
                };
            }

            // Generate cache key
            var cacheKey = GenerateCacheKey(request);
            
            // Check cache if enabled
            if (request.EnableCaching && _settings.EnableCaching)
            {
                var cachedResult = await GetFromCacheAsync(cacheKey, cancellationToken);
                if (cachedResult != null)
                {
                    stopwatch.Stop();
                    cachedResult.ProcessingTimeMs = stopwatch.ElapsedMilliseconds;
                    return cachedResult;
                }
            }

            // Detect language if not specified
            var detectedLanguage = request.SourceLanguage ?? await DetectLanguageAsync(request.Content, cancellationToken);

            // Get AI provider
            var provider = await _aiProviderFactory.GetAvailableProviderAsync(cancellationToken);
            if (provider == null)
            {
                return new SummarizationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "AI service is temporarily unavailable"
                };
            }

            // Build summarization prompt
            var prompt = BuildSummarizationPrompt(request, detectedLanguage);
            var maxTokens = request.MaxTokens ?? GetMaxTokensForLength(request.Length);

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                SystemMessage = GetSystemMessage(request.DocumentType, detectedLanguage),
                MaxTokens = maxTokens,
                Temperature = 0.5 // Medium temperature for balanced creativity
            };

            // Get AI summary
            var aiResponse = await provider.SendChatRequestAsync(aiRequest, cancellationToken);

            if (!aiResponse.IsSuccess || string.IsNullOrEmpty(aiResponse.Content))
            {
                return new SummarizationResponse
                {
                    IsSuccess = false,
                    ErrorMessage = aiResponse.ErrorMessage ?? "Failed to generate summary",
                    ProcessingTimeMs = stopwatch.ElapsedMilliseconds
                };
            }

            // Extract key points
            var keyPoints = ExtractKeyPointsFromSummary(aiResponse.Content);

            // Calculate metrics
            var originalLength = request.Content.Length;
            var summaryLength = aiResponse.Content.Length;
            var compressionRatio = (double)summaryLength / originalLength;

            var response = new SummarizationResponse
            {
                IsSuccess = true,
                Summary = aiResponse.Content.Trim(),
                DetectedLanguage = detectedLanguage,
                KeyPoints = keyPoints,
                OriginalLength = originalLength,
                SummaryLength = summaryLength,
                CompressionRatio = Math.Round(compressionRatio, 3),
                TokensUsed = aiResponse.TokensUsed,
                FromCache = false,
                CacheKey = cacheKey,
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };

            // Cache the result if enabled
            if (request.EnableCaching && _settings.EnableCaching)
            {
                await CacheResultAsync(cacheKey, response, cancellationToken);
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            return new SummarizationResponse
            {
                IsSuccess = false,
                ErrorMessage = $"An error occurred: {ex.Message}",
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };
        }
    }

    public async Task<List<string>> ExtractKeyPointsAsync(string content, int maxPoints = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            var provider = await _aiProviderFactory.GetAvailableProviderAsync(cancellationToken);
            if (provider == null)
            {
                return new List<string>();
            }

            var prompt = $@"Extract {maxPoints} key points from the following text. 
List them as short, clear bullet points.

Text: ""{content}""

Key points:";

            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                SystemMessage = "You are a text analysis assistant. Extract the most important key points from text.",
                MaxTokens = 300,
                Temperature = 0.3
            };

            var response = await provider.SendChatRequestAsync(aiRequest, cancellationToken);
            
            if (response.IsSuccess && !string.IsNullOrEmpty(response.Content))
            {
                return ExtractKeyPointsFromSummary(response.Content);
            }

            return new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    public async Task<string> DetectLanguageAsync(string content, CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple heuristic-based language detection
            // Check for common character ranges
            var sampleText = content.Length > 500 ? content[..500] : content;
            
            // Vietnamese detection (high frequency of Vietnamese-specific characters)
            if (Regex.IsMatch(sampleText, @"[àáảãạăắằẳẵặâấầẩẫậđèéẻẽẹêếềểễệìíỉĩịòóỏõọôốồổỗộơớờởỡợùúủũụưứừửữựỳýỷỹỵ]", RegexOptions.IgnoreCase))
            {
                return "vi";
            }
            
            // Chinese detection (CJK Unified Ideographs)
            if (Regex.IsMatch(sampleText, @"[\u4e00-\u9fff]"))
            {
                return "zh";
            }
            
            // Japanese detection (Hiragana, Katakana)
            if (Regex.IsMatch(sampleText, @"[\u3040-\u309f\u30a0-\u30ff]"))
            {
                return "ja";
            }
            
            // Korean detection (Hangul)
            if (Regex.IsMatch(sampleText, @"[\uac00-\ud7af]"))
            {
                return "ko";
            }
            
            // Default to English
            return _settings.DefaultLanguage;
        }
        catch
        {
            return _settings.DefaultLanguage;
        }
    }

    public async Task ClearCacheAsync(string cacheKey)
    {
        await _cacheRepository.RemoveAsync(cacheKey);
    }

    public async Task ClearAllCacheAsync()
    {
        await _cacheRepository.ClearAllAsync();
    }

    // Private helper methods

    private string GenerateCacheKey(SummarizationRequest request)
    {
        // Create a hash of the content + settings for cache key
        var keyData = $"{request.Content}_{request.Length}_{request.TargetLanguage}_{request.DocumentType}";
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(keyData));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    private async Task<SummarizationResponse?> GetFromCacheAsync(string cacheKey, CancellationToken cancellationToken)
    {
        var cached = await _cacheRepository.GetAsync(cacheKey, cancellationToken);
        if (cached == null)
            return null;

        return new SummarizationResponse
        {
            IsSuccess = true,
            Summary = cached.Summary,
            DetectedLanguage = cached.DetectedLanguage,
            KeyPoints = cached.KeyPoints,
            OriginalLength = cached.OriginalLength,
            SummaryLength = cached.SummaryLength,
            CompressionRatio = cached.CompressionRatio,
            TokensUsed = cached.TokensUsed,
            FromCache = true,
            CacheKey = cacheKey,
            Timestamp = cached.CreatedAt
        };
    }

    private async Task CacheResultAsync(string cacheKey, SummarizationResponse response, CancellationToken cancellationToken)
    {
        var entry = new SummaryCacheEntry
        {
            CacheKey = cacheKey,
            Summary = response.Summary,
            KeyPoints = response.KeyPoints,
            DetectedLanguage = response.DetectedLanguage,
            OriginalLength = response.OriginalLength,
            SummaryLength = response.SummaryLength,
            CompressionRatio = response.CompressionRatio,
            TokensUsed = response.TokensUsed,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddHours(_settings.CacheExpirationHours)
        };

        await _cacheRepository.AddAsync(entry, cancellationToken);
    }

    private string BuildSummarizationPrompt(SummarizationRequest request, string detectedLanguage)
    {
        var lengthInstruction = request.Length switch
        {
            SummaryLength.VeryShort => "very brief 1-2 sentence summary",
            SummaryLength.Short => "short 2-3 sentence summary",
            SummaryLength.Medium => "medium-length paragraph summary",
            SummaryLength.Long => "detailed 2-3 paragraph summary",
            SummaryLength.Detailed => "comprehensive multi-paragraph summary",
            _ => "clear summary"
        };

        var typeContext = request.DocumentType switch
        {
            DocumentType.Academic => "This is an academic/research document.",
            DocumentType.NewsArticle => "This is a news article.",
            DocumentType.Technical => "This is technical documentation.",
            DocumentType.ForumPost => "This is a forum post or discussion.",
            DocumentType.Article => "This is an article or blog post.",
            _ => ""
        };

        var languageInstruction = !string.IsNullOrEmpty(request.TargetLanguage)
            ? $"Write the summary in {request.TargetLanguage}."
            : detectedLanguage != "en"
                ? $"Maintain the same language as the original text ({detectedLanguage})."
                : "";

        return $@"Summarize the following document. {typeContext}

Create a {lengthInstruction} that captures the main ideas and key points.
{languageInstruction}

Document:
{request.Content}

Summary:";
    }

    private string GetSystemMessage(DocumentType documentType, string language)
    {
        var typeInstruction = documentType switch
        {
            DocumentType.Academic => "Focus on methodology, findings, and conclusions.",
            DocumentType.NewsArticle => "Focus on who, what, when, where, why, and how.",
            DocumentType.Technical => "Focus on key technical concepts and procedures.",
            DocumentType.ForumPost => "Focus on main discussion points and conclusions.",
            _ => "Focus on main ideas and key takeaways."
        };

        var languageNote = language != "en"
            ? $" Respond in {language}."
            : "";

        return $"You are a professional document summarizer. Create clear, accurate, and informative summaries. {typeInstruction}{languageNote}";
    }

    private int GetMaxTokensForLength(SummaryLength length)
    {
        return length switch
        {
            SummaryLength.VeryShort => _settings.LengthTokens.VeryShort,
            SummaryLength.Short => _settings.LengthTokens.Short,
            SummaryLength.Medium => _settings.LengthTokens.Medium,
            SummaryLength.Long => _settings.LengthTokens.Long,
            SummaryLength.Detailed => _settings.LengthTokens.Detailed,
            _ => _settings.LengthTokens.Medium
        };
    }

    private List<string> ExtractKeyPointsFromSummary(string summary)
    {
        var keyPoints = new List<string>();
        
        // Try to extract bullet points or numbered list items
        var bulletMatches = Regex.Matches(summary, @"^[\s]*[-•*]\s*(.+)$", RegexOptions.Multiline);
        foreach (Match match in bulletMatches)
        {
            if (match.Groups.Count > 1)
            {
                keyPoints.Add(match.Groups[1].Value.Trim());
            }
        }
        
        // Try numbered lists
        var numberedMatches = Regex.Matches(summary, @"^[\s]*\d+[\.)]\s*(.+)$", RegexOptions.Multiline);
        foreach (Match match in numberedMatches)
        {
            if (match.Groups.Count > 1)
            {
                keyPoints.Add(match.Groups[1].Value.Trim());
            }
        }
        
        // If no structured list found, split by sentences and take first few
        if (keyPoints.Count == 0)
        {
            var sentences = Regex.Split(summary, @"(?<=[.!?])\s+")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Take(5)
                .ToList();
            keyPoints.AddRange(sentences);
        }
        
        return keyPoints.Take(5).ToList();
    }
}
