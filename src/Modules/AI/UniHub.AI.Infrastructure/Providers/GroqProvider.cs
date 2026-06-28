using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using UniHub.AI.Application.Abstractions;
using UniHub.AI.Domain.AIProviders;

namespace UniHub.AI.Infrastructure.Providers;

/// <summary>
/// Groq AI provider implementation.
/// Fast inference with OpenAI-compatible API.
/// </summary>
public sealed class GroqProvider :  AIProviderBase
{
    private const string DefaultBaseUrl = "https://api.groq.com";
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public GroqProvider(AIProviderConfiguration configuration, IHttpClientFactory httpClientFactory) 
        : base(configuration)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = CreateSafeBaseUri(configuration.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {configuration.ApiKey}");
        _httpClient.Timeout = TimeSpan.FromSeconds(configuration.TimeoutSeconds);
    }

    /// <inheritdoc />
    public override AIProviderType ProviderType => AIProviderType.Groq;

    /// <inheritdoc />
    public override async Task<AIResponse> SendChatRequestAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        // Check rate limit
        if (!await TryAcquireRequestSlotAsync(cancellationToken))
        {
            return CreateFailureResponse("Rate limit exceeded for Groq provider");
        }

        try
        {
            // Build messages array
            var messages = new List<object>();
            
            if (!string.IsNullOrEmpty(request.SystemMessage))
            {
                messages.Add(new { role = "system", content = request.SystemMessage });
            }

            if (request.ConversationHistory != null)
            {
                foreach (var msg in request.ConversationHistory)
                {
                    messages.Add(new { role = msg.Role, content = msg.Content });
                }
            }

            messages.Add(new { role = "user", content = request.Prompt });

            // Build request body
            var requestBody = new
            {
                model = Configuration.ModelName,
                messages = messages,
                max_tokens = Math.Min(request.MaxTokens, Configuration.MaxTokensPerRequest),
                temperature = request.Temperature
            };

            // Send request
            var response = await _httpClient.PostAsJsonAsync(
                "/openai/v1/chat/completions",
                requestBody,
                JsonOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                return CreateFailureResponse($"Groq API error ({response.StatusCode}): {errorContent}");
            }

            // Parse response
            var jsonResponse = await response.Content.ReadFromJsonAsync<GroqResponse>(JsonOptions, cancellationToken);
            
            if (jsonResponse == null || jsonResponse.Choices == null || jsonResponse.Choices.Length == 0)
            {
                return CreateFailureResponse("Empty response from Groq API");
            }

            var content = jsonResponse.Choices[0].Message?.Content ?? string.Empty;
            var tokensUsed = jsonResponse.Usage?.TotalTokens ?? 0;

            return CreateSuccessResponse(content, tokensUsed);
        }
        catch (HttpRequestException ex)
        {
            return CreateFailureResponse($"HTTP error calling Groq: {ex.Message}");
        }
        catch (TaskCanceledException ex)
        {
            return CreateFailureResponse($"Groq request timeout: {ex.Message}");
        }
        catch (Exception ex)
        {
            return CreateFailureResponse($"Unexpected error calling Groq: {ex.Message}");
        }
    }

    // Response models for Groq API (OpenAI-compatible)
    private sealed class GroqResponse
    {
        [JsonPropertyName("choices")]
        public GroqChoice[]? Choices { get; set; }

        [JsonPropertyName("usage")]
        public GroqUsage? Usage { get; set; }
    }

    private sealed class GroqChoice
    {
        [JsonPropertyName("message")]
        public GroqMessage? Message { get; set; }
    }

    private sealed class GroqMessage
    {
        [JsonPropertyName("content")]
        public string? Content { get; set; }
    }

    private sealed class GroqUsage
    {
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
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
