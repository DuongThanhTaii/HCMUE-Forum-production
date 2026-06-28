using Microsoft.Extensions.Options;
using UniHub.AI.Application.Abstractions;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;
using UniHub.AI.Infrastructure.Configuration;
using UniHub.AI.Infrastructure.Providers;
using System.Text.RegularExpressions;

namespace UniHub.AI.Infrastructure.Services;

/// <summary>
/// Content moderation service using AI and keyword-based detection
/// </summary>
public class ContentModerationService : IContentModerationService
{
    private readonly IAIProviderFactory _aiProviderFactory;
    private readonly ModerationSettings _settings;
    
    public ContentModerationService(
        IAIProviderFactory aiProviderFactory,
        IOptions<ModerationSettings> settings)
    {
        _aiProviderFactory = aiProviderFactory;
        _settings = settings.Value;
    }

    public async Task<ModerationResponse> ModerateAsync(ModerationRequest request, CancellationToken cancellationToken = default)
    {
        if (!_settings.IsEnabled)
        {
            return new ModerationResponse
            {
                IsSuccess = true,
                IsSafe = true,
                ConfidenceScore = 1.0,
                Reason = "Moderation is disabled"
            };
        }

        var violations = new List<ModerationViolation>();
        
        // Step 1: Keyword-based detection (fast, synchronous)
        if (_settings.UseKeywordDetection)
        {
            violations.AddRange(DetectViolationsByKeywords(request.Content));
            violations.AddRange(DetectSpamHeuristics(request.Content));
        }
        
        // Step 2: AI-based detection (more accurate but slower)
        if (_settings.UseAI)
        {
            var aiViolations = await DetectViolationsWithAI(request.Content, cancellationToken);
            if (aiViolations != null)
            {
                violations.AddRange(aiViolations);
            }
        }
        
        // Calculate overall severity
        double maxSeverity = violations.Any() ? violations.Max(v => v.Severity) : 0;
        double avgConfidence = violations.Any() ? violations.Average(v => v.Confidence) : 1.0;
        
        // Determine action based on thresholds
        bool isBlocked = maxSeverity >= _settings.BlockThreshold;
        bool requiresReview = maxSeverity >= _settings.ReviewThreshold && !isBlocked;
        bool isSafe = maxSeverity < _settings.SafeThreshold;
        
        var response = new ModerationResponse
        {
            IsSuccess = true,
            IsSafe = isSafe,
            IsBlocked = isBlocked,
            RequiresReview = requiresReview,
            ConfidenceScore = avgConfidence,
            Violations = violations,
            Reason = BuildReasonMessage(violations, isSafe, isBlocked, requiresReview)
        };
        
        return response;
    }

    public async Task<bool> IsSafeAsync(string content, CancellationToken cancellationToken = default)
    {
        var request = new ModerationRequest { Content = content };
        var response = await ModerateAsync(request, cancellationToken);
        return response.IsSafe;
    }

    public async Task<bool> IsSpamAsync(string content, CancellationToken cancellationToken = default)
    {
        var violations = DetectSpamHeuristics(content);
        
        if (_settings.UseAI)
        {
            var aiViolations = await DetectViolationsWithAI(content, CancellationToken.None);
            if (aiViolations != null)
            {
                violations.AddRange(aiViolations.Where(v => v.Type == ViolationType.Spam));
            }
        }
        
        return violations.Any(v => v.Type == ViolationType.Spam && v.Severity >= _settings.ReviewThreshold);
    }

    public async Task<bool> IsToxicAsync(string content, CancellationToken cancellationToken = default)
    {
        var violations = DetectViolationsByKeywords(content);
        
        if (_settings.UseAI)
        {
            var aiViolations = await DetectViolationsWithAI(content, cancellationToken);
            if (aiViolations != null)
            {
                violations.AddRange(aiViolations.Where(v => 
                    v.Type == ViolationType.Toxic || 
                    v.Type == ViolationType.HateSpeech || 
                    v.Type == ViolationType.Harassment));
            }
        }
        
        return violations.Any(v => v.Severity >= _settings.ReviewThreshold);
    }

    // Private helper methods
    
    private List<ModerationViolation> DetectViolationsByKeywords(string content)
    {
        var violations = new List<ModerationViolation>();
        var lowerContent = content.ToLowerInvariant();
        
        // Check profanity
        foreach (var keyword in _settings.ToxicContent.ProfanityKeywords)
        {
            if (lowerContent.Contains(keyword.ToLowerInvariant()))
            {
                violations.Add(new ModerationViolation
                {
                    Type = ViolationType.Profanity,
                    Severity = 0.6,
                    Confidence = 0.9,
                    Description = $"Detected profanity: '{keyword}'"
                });
            }
        }
        
        // Check hate speech
        foreach (var keyword in _settings.ToxicContent.HateSpeechKeywords)
        {
            if (lowerContent.Contains(keyword.ToLowerInvariant()))
            {
                violations.Add(new ModerationViolation
                {
                    Type = ViolationType.HateSpeech,
                    Severity = 0.9,
                    Confidence = 0.85,
                    Description = $"Detected hate speech indicator: '{keyword}'"
                });
            }
        }
        
        return violations;
    }
    
    private List<ModerationViolation> DetectSpamHeuristics(string content)
    {
        var violations = new List<ModerationViolation>();
        
        // Check URL count
        var urlCount = Regex.Matches(content, @"https?://\S+", RegexOptions.IgnoreCase).Count;
        if (urlCount > _settings.SpamDetection.MaxUrls)
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Spam,
                Severity = 0.7,
                Confidence = 0.8,
                Description = $"Too many URLs ({urlCount} > {_settings.SpamDetection.MaxUrls})"
            });
        }
        
        // Check repeated characters
        var repeatedChars = Regex.Matches(content, @"(.)\1{" + (_settings.SpamDetection.MaxRepeatedChars - 1) + ",}");
        if (repeatedChars.Count > 0)
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Spam,
                Severity = 0.5,
                Confidence = 0.7,
                Description = "Excessive repeated characters detected"
            });
        }
        
        // Check uppercase ratio
        var uppercaseCount = content.Count(char.IsUpper);
        var letterCount = content.Count(char.IsLetter);
        if (letterCount > 10 && (double)uppercaseCount / letterCount > _settings.SpamDetection.MaxUppercaseRatio)
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Spam,
                Severity = 0.6,
                Confidence = 0.75,
                Description = "Excessive uppercase text"
            });
        }
        
        // Check spam keywords
        var lowerContent = content.ToLowerInvariant();
        foreach (var keyword in _settings.SpamDetection.SpamKeywords)
        {
            if (lowerContent.Contains(keyword.ToLowerInvariant()))
            {
                violations.Add(new ModerationViolation
                {
                    Type = ViolationType.Spam,
                    Severity = 0.7,
                    Confidence = 0.8,
                    Description = $"Spam keyword detected: '{keyword}'"
                });
            }
        }
        
        return violations;
    }
    
    private async Task<List<ModerationViolation>?> DetectViolationsWithAI(string content, CancellationToken cancellationToken)
    {
        try
        {
            var provider = await _aiProviderFactory.GetAvailableProviderAsync(cancellationToken);
            if (provider == null)
            {
                return null; // AI not available, rely on keyword detection
            }
            
            var prompt = BuildModerationPrompt(content);
            var aiRequest = new AIRequest
            {
                Prompt = prompt,
                SystemMessage = "You are a content moderation AI. Analyze content for toxicity, hate speech, spam, and other violations. Respond in JSON format.",
                MaxTokens = 512,
                Temperature = 0.3 // Lower temperature for more consistent moderation
            };
            
            var aiResponse = await provider.SendChatRequestAsync(aiRequest, cancellationToken);
            
            if (!aiResponse.IsSuccess || string.IsNullOrEmpty(aiResponse.Content))
            {
                return null;
            }
            
            // Parse AI response to extract violations
            return ParseAIResponse(aiResponse.Content);
        }
        catch (Exception)
        {
            // If AI fails, return null to rely on keyword detection
            return null;
        }
    }
    
    private string BuildModerationPrompt(string content)
    {
        return $@"Analyze the following content for moderation issues:

Content: ""{content}""

Identify any violations in these categories:
1. Toxic language (insults, aggression)
2. Hate speech (discrimination, prejudice)
3. Harassment or bullying
4. Sexual content
5. Violence or threats
6. Spam or advertising
7. Profanity

For each violation found, provide:
- Type (toxic/hate/harassment/sexual/violence/spam/profanity)
- Severity (0.0 to 1.0, where 1.0 is most severe)
- Brief description

If content is safe, respond with: ""SAFE""

Be strict but fair. University forum context.";
    }
    
    private List<ModerationViolation> ParseAIResponse(string aiResponse)
    {
        var violations = new List<ModerationViolation>();
        
        // Simple parsing - in production, use structured JSON response
        var lower = aiResponse.ToLowerInvariant();
        
        if (lower.Contains("safe") && !lower.Contains("unsafe"))
        {
            return violations; // No violations
        }
        
        // Parse violations based on keywords in AI response
        if (lower.Contains("toxic"))
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Toxic,
                Severity = ExtractSeverity(aiResponse, "toxic"),
                Confidence = 0.8,
                Description = "AI detected toxic language"
            });
        }
        
        if (lower.Contains("hate") || lower.Contains("discriminat"))
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.HateSpeech,
                Severity = ExtractSeverity(aiResponse, "hate"),
                Confidence = 0.85,
                Description = "AI detected hate speech"
            });
        }
        
        if (lower.Contains("harass") || lower.Contains("bully"))
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Harassment,
                Severity = ExtractSeverity(aiResponse, "harass"),
                Confidence = 0.8,
                Description = "AI detected harassment"
            });
        }
        
        if (lower.Contains("spam") || lower.Contains("advertis"))
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Spam,
                Severity = ExtractSeverity(aiResponse, "spam"),
                Confidence = 0.75,
                Description = "AI detected spam"
            });
        }
        
        if (lower.Contains("profan") || lower.Contains("vulgar"))
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Profanity,
                Severity = ExtractSeverity(aiResponse, "profan"),
                Confidence = 0.8,
                Description = "AI detected profanity"
            });
        }
        
        if (lower.Contains("violen") || lower.Contains("threat"))
        {
            violations.Add(new ModerationViolation
            {
                Type = ViolationType.Violence,
                Severity = ExtractSeverity(aiResponse, "violen"),
                Confidence = 0.85,
                Description = "AI detected violence or threats"
            });
        }
        
        return violations;
    }
    
    private double ExtractSeverity(string aiResponse, string keyword)
    {
        // Try to extract severity from response like "severity: 0.8" or "0.7/1.0"
        var match = Regex.Match(aiResponse, @"severity[:\s]+([0-9.]+)", RegexOptions.IgnoreCase);
        if (match.Success && double.TryParse(match.Groups[1].Value, out var severity))
        {
            return Math.Min(1.0, Math.Max(0.0, severity));
        }
        
        // Default severity based on violation type
        return keyword.ToLowerInvariant() switch
        {
            "hate" => 0.9,
            "violen" => 0.85,
            "toxic" => 0.7,
            "harass" => 0.75,
            "spam" => 0.6,
            _ => 0.5
        };
    }
    
    private string BuildReasonMessage(List<ModerationViolation> violations, bool isSafe, bool isBlocked, bool requiresReview)
    {
        if (isSafe)
        {
            return "Content passed moderation checks";
        }
        
        if (isBlocked)
        {
            var mainViolations = string.Join(", ", violations
                .OrderByDescending(v => v.Severity)
                .Take(3)
                .Select(v => v.Type.ToString()));
            return $"Content blocked due to: {mainViolations}";
        }
        
        if (requiresReview)
        {
            return "Content flagged for manual review";
        }
        
        return "Content contains minor issues";
    }
}
