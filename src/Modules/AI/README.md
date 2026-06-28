# AI Provider Configuration Guide

## Overview

UniHub AI Module supports multiple AI providers with automatic fallback based on priority and availability.

## Supported Providers

### 1. Groq (Primary - Priority 1)

- **Fast inference** with free tier
- **OpenAI-compatible API**
- **Model**: `llama-3.1-70b-versatile`
- **Free tier**: 30 requests/minute
- **Get API Key**: https://console.groq.com/keys

### 2. Google Gemini (Secondary - Priority 2)

- **Google's generative AI**
- **Gemini API format**
- **Model**: `gemini-pro`
- **Free tier**: 60 requests/minute
- **Get API Key**: https://makersuite.google.com/app/apikey

### 3. OpenRouter (Tertiary - Priority 3)

- **Unified API** for multiple models
- **OpenAI-compatible API**
- **Model**: `meta-llama/llama-3.1-8b-instruct:free`
- **Free tier**: 20 requests/minute
- **Get API Key**: https://openrouter.ai/keys

## Configuration

### 1. Copy example config

```bash
cp appsettings.AI.example.json appsettings.AI.json
```

### 2. Add API keys

Edit `appsettings.AI.json` and replace placeholders with your actual API keys:

```json
{
  "AIProviders": {
    "Groq": {
      "ApiKey": "gsk_YOUR_ACTUAL_KEY",
      ...
    },
    "Gemini": {
      "ApiKey": "YOUR_ACTUAL_KEY",
      ...
    },
    "OpenRouter": {
      "ApiKey": "sk-or-YOUR_ACTUAL_KEY",
      ...
    }
  }
}
```

### 3. Merge into appsettings.json

Add the AIProviders section to your main `appsettings.json` or `appsettings.Development.json`.

## How Provider Rotation Works

1. **Priority-based selection**: Providers are tried in order of priority (1 = highest)
2. **Automatic fallback**: If a provider fails or hits rate limit, next provider is used
3. **Rate limiting**: Each provider tracks requests per minute independently
4. **Quota tracking**: Built-in quota management prevents exceeding API limits

## Provider Features

| Feature    | Groq          | Gemini        | OpenRouter    |
| ---------- | ------------- | ------------- | ------------- |
| Speed      | ⚡ Very Fast  | 🐢 Medium     | 🐇 Fast       |
| Free Tier  | ✅ 30 req/min | ✅ 60 req/min | ✅ 20 req/min |
| Max Tokens | 8000          | 4096          | 4096          |
| API Format | OpenAI        | Gemini        | OpenAI        |
| Models     | Llama 3.1     | Gemini Pro    | Multiple      |

## Configuration Options

### Per-Provider Settings

- **ApiKey**: Your API key for the provider
- **BaseUrl**: API base URL (don't change unless necessary)
- **ModelName**: Which model to use
- **MaxRequestsPerMinute**: Rate limit (adjust based on your API plan)
- **MaxTokensPerRequest**: Maximum tokens per request
- **IsEnabled**: Enable/disable this provider
- **Priority**: Lower number = higher priority (1, 2, 3, ...)
- **TimeoutSeconds**: Request timeout in seconds

### Disable a Provider

Set `IsEnabled` to `false`:

```json
{
  "Groq": {
    "IsEnabled": false,
    ...
  }
}
```

### Change Priority

Adjust `Priority` values to change fallback order:

```json
{
  "Gemini": {
    "Priority": 1,  // Try Gemini first
    ...
  },
  "Groq": {
    "Priority": 2,  // Then Groq
    ...
  }
}
```

## Usage Example

The AIProviderFactory automatically selects the best available provider:

```csharp
// Inject factory
public class MyChatService
{
    private readonly IAIProviderFactory _providerFactory;

    public MyChatService(IAIProviderFactory providerFactory)
    {
        _providerFactory = providerFactory;
    }

    public async Task<string> GetAIResponse(string prompt)
    {
        // Get best available provider (automatic fallback)
        var provider = await _providerFactory.GetAvailableProviderAsync();

        if (provider == null)
        {
            return "All AI providers are unavailable";
        }

        var request = new AIRequest
        {
            Prompt = prompt,
            SystemMessage = "You are a helpful assistant for UniHub",
            MaxTokens = 1024,
            Temperature = 0.7
        };

        var response = await provider.SendChatRequestAsync(request);
        return response.IsSuccess ? response.Content : response.ErrorMessage;
    }
}
```

## Testing

Test each provider individually:

```bash
# Build project
dotnet build

# Run tests
dotnet test

# Run API and test endpoints
dotnet run --project src/UniHub.API
curl -X POST "http://localhost:5000/api/v1/ai/chat" \
  -H "Content-Type: application/json" \
  -d '{"prompt": "Hello!"}'
```

## Troubleshooting

### "Rate limit exceeded"

- Wait 1 minute for quota to reset
- Or disable the provider and use fallback

### "API error (401)"

- Check your API key is correct
- Ensure no extra spaces in config

### "Empty response"

- Check the model name is correct
- Try a different provider

### "Request timeout"

- Increase TimeoutSeconds in config
- Check your internet connection

## Security Notes

⚠️ **Never commit API keys to git!**

- Add `appsettings.AI.json` to `.gitignore`
- Use environment variables in production
- Rotate keys regularly

## Links

- Groq: https://groq.com
- Google Gemini: https://ai.google.dev
- OpenRouter: https://openrouter.ai
