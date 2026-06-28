using UniHub.AI.Application.DTOs;

namespace UniHub.AI.Application.Services;

/// <summary>
/// Service for content moderation using AI
/// </summary>
public interface IContentModerationService
{
    /// <summary>
    /// Moderate content for toxicity, spam, and other violations
    /// </summary>
    Task<ModerationResponse> ModerateAsync(ModerationRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Quick check if content is safe (simplified version)
    /// </summary>
    Task<bool> IsSafeAsync(string content, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detect spam content
    /// </summary>
    Task<bool> IsSpamAsync(string content, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Detect toxic content
    /// </summary>
    Task<bool> IsToxicAsync(string content, CancellationToken cancellationToken = default);
}
