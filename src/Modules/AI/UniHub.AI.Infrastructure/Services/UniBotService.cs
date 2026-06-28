using UniHub.AI.Application.Abstractions;
using UniHub.AI.Application.DTOs;
using UniHub.AI.Application.Services;
using UniHub.AI.Domain.Entities;
using UniHub.AI.Infrastructure.Providers;

namespace UniHub.AI.Infrastructure.Services;

/// <summary>
/// UniBot service that combines FAQ knowledge base with AI
/// </summary>
public class UniBotService : IUniBotService
{
    private readonly IFAQService _faqService;
    private readonly IConversationService _conversationService;
    private readonly IAIProviderFactory _aiProviderFactory;
    
    // Thresholds for confidence scoring
    private const double HIGH_CONFIDENCE_THRESHOLD = 0.8;
    private const double LOW_CONFIDENCE_THRESHOLD = 0.3;
    
    public UniBotService(
        IFAQService faqService,
        IConversationService conversationService,
        IAIProviderFactory aiProviderFactory)
    {
        _faqService = faqService;
        _conversationService = conversationService;
        _aiProviderFactory = aiProviderFactory;
    }

    public async Task<ChatResponse> ChatAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get or create conversation
            var conversation = await GetOrCreateConversationAsync(request, cancellationToken);
            
            // Check if conversation was handed off to support
            if (conversation.HandedOffToSupport)
            {
                return new ChatResponse
                {
                    IsSuccess = true,
                    Message = "This conversation has been transferred to our support team. A human agent will assist you shortly.",
                    ConversationId = conversation.Id,
                    MessageId = Guid.NewGuid(),
                    SuggestHandoff = false
                };
            }
            
            // Add user message to conversation
            var userMessage = await _conversationService.AddMessageAsync(
                conversation.Id,
                MessageRole.User,
                request.Message,
                cancellationToken: cancellationToken);
            
            // Search FAQ knowledge base
            var relevantFAQs = await _faqService.SearchAsync(request.Message, maxResults: 3, cancellationToken);
            
            // Build AI prompt with FAQ context and conversation history
            var aiPrompt = await BuildAIPromptAsync(request, conversation, relevantFAQs, cancellationToken);
            
            // Get AI response
            var provider = await _aiProviderFactory.GetAvailableProviderAsync(cancellationToken);
            if (provider == null)
            {
                return new ChatResponse
                {
                    IsSuccess = false,
                    ErrorMessage = "AI service is temporarily unavailable. Please try again later.",
                    ConversationId = conversation.Id,
                    MessageId = Guid.NewGuid()
                };
            }
            
            var aiRequest = new AIRequest
            {
                Prompt = aiPrompt,
                SystemMessage = GetSystemMessage(),
                MaxTokens = 1024,
                Temperature = 0.7,
                ConversationHistory = request.IncludeHistory 
                    ? await BuildConversationHistoryAsync(conversation, request.MaxHistoryMessages)
                    : null
            };
            
            var aiResponse = await provider.SendChatRequestAsync(aiRequest, cancellationToken);
            
            if (!aiResponse.IsSuccess)
            {
                return new ChatResponse
                {
                    IsSuccess = false,
                    ErrorMessage = aiResponse.ErrorMessage ?? "Failed to get AI response",
                    ConversationId = conversation.Id,
                    MessageId = Guid.NewGuid()
                };
            }
            
            // Calculate confidence score based on FAQ matches
            double confidenceScore = CalculateConfidenceScore(relevantFAQs, request.Message);
            
            // Add AI response to conversation
            var assistantMessage = await _conversationService.AddMessageAsync(
                conversation.Id,
                MessageRole.Assistant,
                aiResponse.Content,
                sourceFAQId: relevantFAQs.FirstOrDefault()?.Id,
                confidenceScore: confidenceScore,
                cancellationToken: cancellationToken);
            
            // Update FAQ usage count if a FAQ was used
            if (relevantFAQs.Any())
            {
                await _faqService.IncrementUsageCountAsync(relevantFAQs.First().Id, cancellationToken);
            }
            
            // Determine if handoff to support is suggested
            bool suggestHandoff = ShouldSuggestHandoff(confidenceScore, conversation);
            
            // Generate suggested follow-up questions
            var suggestedQuestions = GenerateSuggestedQuestions(relevantFAQs);
            
            return new ChatResponse
            {
                IsSuccess = true,
                Message = aiResponse.Content,
                ConversationId = conversation.Id,
                MessageId = assistantMessage.Id,
                ConfidenceScore = confidenceScore,
                SourceFAQ = relevantFAQs.FirstOrDefault() != null 
                    ? MapToDto(relevantFAQs.First()) 
                    : null,
                SuggestedQuestions = suggestedQuestions,
                SuggestHandoff = suggestHandoff,
                HandoffReason = suggestHandoff 
                    ? "I'm not confident I can fully answer your question. Would you like to speak with a human support agent?" 
                    : null
            };
        }
        catch (Exception ex)
        {
            return new ChatResponse
            {
                IsSuccess = false,
                ErrorMessage = $"An error occurred: {ex.Message}",
                ConversationId = request.ConversationId ?? Guid.Empty,
                MessageId = Guid.NewGuid()
            };
        }
    }

    public async Task<ConversationDto?> GetConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        var conversation = await _conversationService.GetByIdAsync(conversationId, cancellationToken);
        return conversation != null ? MapToDto(conversation) : null;
    }

    public async Task<List<ConversationDto>> GetUserConversationsAsync(Guid userId, int skip = 0, int take = 20, 
        CancellationToken cancellationToken = default)
    {
        var conversations = await _conversationService.GetByUserIdAsync(userId, skip, take, cancellationToken);
        return conversations.Select(MapToDto).ToList();
    }

    public Task<bool> CloseConversationAsync(Guid conversationId, CancellationToken cancellationToken = default)
    {
        return _conversationService.CloseConversationAsync(conversationId, cancellationToken);
    }

    public Task MarkResponseHelpfulAsync(Guid messageId, bool isHelpful, CancellationToken cancellationToken = default)
    {
        return _conversationService.MarkMessageHelpfulAsync(messageId, isHelpful, cancellationToken);
    }

    public Task<bool> RequestHandoffAsync(Guid conversationId, string reason, CancellationToken cancellationToken = default)
    {
        return _conversationService.HandoffToSupportAsync(conversationId, reason, cancellationToken: cancellationToken);
    }

    // Private helper methods
    
    private async Task<Conversation> GetOrCreateConversationAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        if (request.ConversationId.HasValue)
        {
            var existing = await _conversationService.GetByIdAsync(request.ConversationId.Value, cancellationToken);
            if (existing != null)
                return existing;
        }
        
        if (!string.IsNullOrEmpty(request.SessionId))
        {
            var sessionConversation = await _conversationService.GetBySessionIdAsync(request.SessionId, cancellationToken);
            if (sessionConversation != null)
                return sessionConversation;
        }
        
        return await _conversationService.CreateAsync(request.UserId, request.SessionId, cancellationToken);
    }

    private async Task<string> BuildAIPromptAsync(ChatRequest request, Conversation conversation, 
        List<FAQItem> relevantFAQs, CancellationToken cancellationToken)
    {
        var prompt = $"User question: {request.Message}\n\n";
        
        if (relevantFAQs.Any())
        {
            prompt += "Relevant FAQ information:\n";
            foreach (var faq in relevantFAQs)
            {
                prompt += $"Q: {faq.Question}\nA: {faq.Answer}\n\n";
            }
            prompt += "Please provide a helpful response based on the FAQ information above. ";
            prompt += "If the FAQ doesn't fully answer the question, supplement with additional helpful information. ";
        }
        else
        {
            prompt += "No exact FAQ match found. ";
            prompt += "Please provide a helpful and informative response based on general university information. ";
        }
        
        prompt += "Be friendly, concise, and professional.";
        
        return prompt;
    }

    private async Task<List<ChatMessage>?> BuildConversationHistoryAsync(
        Conversation conversation, int maxMessages)
    {
        if (conversation.Messages.Count == 0)
            return null;
        
        return conversation.Messages
            .OrderByDescending(m => m.SentAt)
            .Take(maxMessages)
            .OrderBy(m => m.SentAt)
            .Select(m => new ChatMessage
            {
                Role = m.Role.ToString().ToLowerInvariant(),
                Content = m.Content
            })
            .ToList();
    }

    private string GetSystemMessage()
    {
        return "You are UniBot, a helpful AI assistant for HCMUE (Ho Chi Minh City University of Education). " +
               "Your role is to help students, faculty, and visitors with information about the university. " +
               "Be friendly, professional, and concise. " +
               "If you don't know something, admit it and suggest contacting the appropriate department or human support.";
    }

    private double CalculateConfidenceScore(List<FAQItem> faqs, string query)
    {
        if (!faqs.Any())
            return 0.2; // Low confidence when no FAQ match
        
        // Simple scoring based on number and quality of FAQ matches
        var topFAQ = faqs.First();
        var queryWords = query.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var matchCount = queryWords.Count(word => 
            topFAQ.Question.Contains(word, StringComparison.OrdinalIgnoreCase));
        
        var baseScore = Math.Min(1.0, 0.5 + (matchCount * 0.1));
        
        // Boost score based on FAQ priority and usage
        var priorityBoost = Math.Min(0.2, topFAQ.Priority * 0.02);
        
        return Math.Min(1.0, baseScore + priorityBoost);
    }

    private bool ShouldSuggestHandoff(double confidenceScore, Conversation conversation)
    {
        // Suggest handoff if:
        // 1. Confidence is very low
        // 2. Multiple messages with low confidence (user seems frustrated)
        
        if (confidenceScore < LOW_CONFIDENCE_THRESHOLD)
            return true;
        
        var recentLowConfidenceCount = conversation.Messages
            .Where(m => m.Role == MessageRole.Assistant && m.ConfidenceScore.HasValue)
            .OrderByDescending(m => m.SentAt)
            .Take(3)
            .Count(m => m.ConfidenceScore < 0.5);
        
        return recentLowConfidenceCount >= 2;
    }

    private List<string> GenerateSuggestedQuestions(List<FAQItem> faqs)
    {
        // Generate related questions from FAQs in same category
        var suggestions = new List<string>();
        
        if (faqs.Any())
        {
            var category = faqs.First().Category;
            suggestions.AddRange(faqs
                .Where(f => f.Category == category)
                .Take(3)
                .Select(f => f.Question));
        }
        
        return suggestions;
    }

    private FAQItemDto MapToDto(FAQItem faq)
    {
        return new FAQItemDto
        {
            Id = faq.Id,
            Question = faq.Question,
            Answer = faq.Answer,
            Category = faq.Category,
            Tags = faq.Tags,
            Priority = faq.Priority,
            UsageCount = faq.UsageCount,
            AverageRating = faq.AverageRating
        };
    }

    private ConversationDto MapToDto(Conversation conversation)
    {
        return new ConversationDto
        {
            Id = conversation.Id,
            UserId = conversation.UserId,
            SessionId = conversation.SessionId,
            Title = conversation.Title,
            Messages = conversation.Messages.Select(m => new ConversationMessageDto
            {
                Id = m.Id,
                Role = m.Role.ToString(),
                Content = m.Content,
                SourceFAQId = m.SourceFAQId,
                ConfidenceScore = m.ConfidenceScore,
                IsHelpful = m.IsHelpful,
                SentAt = m.SentAt
            }).ToList(),
            HandedOffToSupport = conversation.HandedOffToSupport,
            HandoffReason = conversation.HandoffReason,
            StartedAt = conversation.StartedAt,
            LastActiveAt = conversation.LastActiveAt,
            IsClosed = conversation.IsClosed
        };
    }
}
