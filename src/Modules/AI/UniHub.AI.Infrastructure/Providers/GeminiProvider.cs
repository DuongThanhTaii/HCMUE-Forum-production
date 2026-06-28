using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniHub.AI.Application.Abstractions;
using UniHub.AI.Domain.AIProviders;

namespace UniHub.AI.Infrastructure.Providers;

/// <summary>
/// Google Gemini AI provider implementation.
/// Google's generative AI with Gemini API format.
/// </summary>
public sealed class GeminiProvider : AIProviderBase
{
    private const string DefaultBaseUrl = "https://generativelanguage.googleapis.com";
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GeminiProvider(AIProviderConfiguration configuration, IHttpClientFactory httpClientFactory) 
        : base(configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = CreateSafeBaseUri(configuration.BaseUrl);
        _httpClient.Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds);
    }

    /// <inheritdoc />
    public override AIProviderType ProviderType => AIProviderType.Gemini;

    /// <inheritdoc />
    public override async Task<AIResponse> SendChatRequestAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        // Check rate limit
        if (!await TryAcquireRequestSlotAsync(cancellationToken))
        {
            return CreateFailureResponse("Rate limit exceeded for Gemini provider");
        }

        try
        {
            // Build contents array for Gemini API
            var contents = new List<object>();

            // Gemini uses "contents" with parts
            if (!string.IsNullOrEmpty(request.SystemMessage))
            {
                contents.Add(new
                {
                    role = "model",
                    parts = new[] { new { text = $"System: {request.SystemMessage}" } }
                });
            }

            if (request.ConversationHistory != null)
            {
                foreach (var msg in request.ConversationHistory)
                {
                    var role = msg.Role == "assistant" ? "model" : "user";
                    contents.Add(new
                    {
                        role = role,
                        parts = new[] { new { text = msg.Content } }
                    });
                }
            }

            contents.Add(new
            {
                role = "user",
                parts = new[] { new { text = request.Prompt } }
            });

            // Build request body (Gemini format)
            var requestBody = new
            {
                contents = contents,
                generationConfig = new
                {
                    maxOutputTokens = Math.Min(request.MaxTokens, Configuration.MaxTokensPerRequest),
                    temperature = request.Temperature
                }
            };

            // Send request (API key in URL query parameter)
            var url = $"/v1beta/models/{Configuration.ModelName}:generateContent?key={Configuration.ApiKey}";
            var response = await _httpClient.PostAsJsonAsync(
                url,
                requestBody,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return CreateFailureResponse($"Gemini API error ({response.StatusCode}): {errorContent}");
            }

            // Parse response
            var jsonResponse = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);
            
            if (jsonResponse == null || jsonResponse.Candidates == null || jsonResponse.Candidates.Length == 0)
            {
                return CreateFailureResponse("Empty response from Gemini API");
            }

            var content = jsonResponse.Candidates[0].Content?.Parts?[0].Text ?? string.Empty;
            var tokensUsed = jsonResponse.UsageMetadata?.TotalTokenCount ?? 0;

            return CreateSuccessResponse(content, tokensUsed);
        }
        catch (HttpRequestException ex)
        {
            return CreateFailureResponse($"HTTP error calling Gemini: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return CreateFailureResponse($"Gemini request timeout: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CreateFailureResponse($"Unexpected error calling Gemini: {ex.Message}");
        }
    }

    // Response models for Gemini API
    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public GeminiCandidate[]? Candidates { get; set; }

        [JsonPropertyName("usageMetadata")]
        public GeminiUsageMetadata? UsageMetadata { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")]
        public GeminiPart[]? Parts { get; set; }
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    private sealed class GeminiUsageMetadata
    {
        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }

    private static Uri CreateSafeBaseUri(string? candidate)
    {
        if (Uri.TryCreate(candidate, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        return new Uri(DefaultBaseUrl, UriKind.Absolute);
    }
}
