# 📋 INTEGRATION CONTRACTS

> **Định nghĩa contracts cho communication giữa các bounded contexts**
>
> DTOs, Domain Events, và Integration Events được sử dụng để trao đổi data giữa modules.

---

## 📋 OVERVIEW

Integration contracts là **published language** giữa các bounded contexts:
- **Shared DTOs**: Data structures được share qua API calls
- **Domain Events**: Events phát sinh trong context (in-process)
- **Integration Events**: Events cross contexts (async messaging)

**Principles**:
1. ✅ **Stable contracts**: Avoid breaking changes
2. ✅ **Backward compatible**: Support versioning
3. ✅ **Minimal coupling**: Only share what's necessary
4. ✅ **Self-contained**: Include all needed data

---

## 🎯 CONTRACT TYPES

### 1. Shared DTOs (Synchronous)

**Purpose**: Request-response data exchange  
**Location**: `UniHub.Contracts` project  
**Usage**: API calls, in-process queries

### 2. Domain Events (In-Process)

**Purpose**: Internal context events  
**Location**: Each context's Domain layer  
**Usage**: MediatR handlers within same context

### 3. Integration Events (Cross-Context)

**Purpose**: Async communication between contexts  
**Location**: `UniHub.Contracts` project  
**Usage**: Message bus (future), MediatR (current)

---

## 👤 IDENTITY CONTEXT CONTRACTS

### Shared DTOs

#### UserDto

```csharp
namespace UniHub.Contracts.Identity;

/// <summary>
/// Full user information for display across contexts
/// </summary>
public sealed record UserDto
{
    public required Guid Id { get; init; }
    public required string Email { get; init; }
    public required string DisplayName { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? AvatarUrl { get; init; }
    public OfficialBadgeDto? OfficialBadge { get; init; }
    public required List<string> Roles { get; init; }
    public required bool IsVerified { get; init; }
    public DateTime CreatedAt { get; init; }
}
```

#### UserSummaryDto

```csharp
namespace UniHub.Contracts.Identity;

/// <summary>
/// Lightweight user info for lists, participants, etc.
/// </summary>
public sealed record UserSummaryDto
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public string? AvatarUrl { get; init; }
    public bool IsVerified { get; init; }
}
```

#### OfficialBadgeDto

```csharp
namespace UniHub.Contracts.Identity;

/// <summary>
/// Official organization badge (University, Department, etc.)
/// </summary>
public sealed record OfficialBadgeDto
{
    public required string Name { get; init; }
    public required string Icon { get; init; }
    public string? Description { get; init; }
    public string? Color { get; init; }
}
```

#### PermissionDto

```csharp
namespace UniHub.Contracts.Identity;

/// <summary>
/// Permission information for authorization
/// Format: {Module}.{Entity}.{Action} (e.g., "Forum.Post.Delete")
/// </summary>
public sealed record PermissionDto
{
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required string Module { get; init; }
    public string? Description { get; init; }
}
```

#### RoleDto

```csharp
namespace UniHub.Contracts.Identity;

/// <summary>
/// Role with permissions
/// </summary>
public sealed record RoleDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required List<PermissionDto> Permissions { get; init; }
}
```

---

### Domain Events

```csharp
namespace UniHub.Domain.Identity.Events;

/// <summary>
/// User successfully registered
/// </summary>
public sealed record UserRegisteredEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime RegisteredAt
) : IDomainEvent;

/// <summary>
/// User profile updated (name, avatar, etc.)
/// </summary>
public sealed record UserProfileUpdatedEvent(
    Guid UserId,
    DateTime UpdatedAt
) : IDomainEvent;

/// <summary>
/// User email verified
/// </summary>
public sealed record UserEmailVerifiedEvent(
    Guid UserId,
    string Email,
    DateTime VerifiedAt
) : IDomainEvent;

/// <summary>
/// Role assigned to user
/// </summary>
public sealed record RoleAssignedEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime AssignedAt
) : IDomainEvent;

/// <summary>
/// Role removed from user
/// </summary>
public sealed record RoleRemovedEvent(
    Guid UserId,
    Guid RoleId,
    string RoleName,
    DateTime RemovedAt
) : IDomainEvent;

/// <summary>
/// Official badge assigned to user
/// </summary>
public sealed record OfficialBadgeAssignedEvent(
    Guid UserId,
    string BadgeName,
    DateTime AssignedAt
) : IDomainEvent;
```

---

### Integration Events

```csharp
namespace UniHub.Contracts.Identity.Events;

/// <summary>
/// Published when new user registers (for welcome notification)
/// </summary>
public sealed record UserRegisteredIntegrationEvent(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime RegisteredAt
) : IIntegrationEvent;

/// <summary>
/// Published when user profile changes (for cache invalidation)
/// </summary>
public sealed record UserProfileChangedIntegrationEvent(
    Guid UserId,
    string? NewDisplayName,
    string? NewAvatarUrl,
    DateTime ChangedAt
) : IIntegrationEvent;
```

---

## 💬 FORUM CONTEXT CONTRACTS

### Shared DTOs

#### PostDto

```csharp
namespace UniHub.Contracts.Forum;

/// <summary>
/// Full post information
/// </summary>
public sealed record PostDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Content { get; init; }
    public required string ContentHtml { get; init; }
    public required UserSummaryDto Author { get; init; }
    public required CategoryDto Category { get; init; }
    public required List<TagDto> Tags { get; init; }
    public required int VoteCount { get; init; }
    public required int CommentCount { get; init; }
    public required int ViewCount { get; init; }
    public required PostStatus Status { get; init; }
    public required bool IsConfession { get; init; }
    public required bool IsPinned { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
```

#### PostSummaryDto

```csharp
namespace UniHub.Contracts.Forum;

/// <summary>
/// Lightweight post info for lists
/// </summary>
public sealed record PostSummaryDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required UserSummaryDto Author { get; init; }
    public required string CategoryName { get; init; }
    public required int VoteCount { get; init; }
    public required int CommentCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

#### CategoryDto

```csharp
namespace UniHub.Contracts.Forum;

/// <summary>
/// Forum category
/// </summary>
public sealed record CategoryDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public required int PostCount { get; init; }
}
```

#### CommentDto

```csharp
namespace UniHub.Contracts.Forum;

/// <summary>
/// Comment on a post
/// </summary>
public sealed record CommentDto
{
    public required Guid Id { get; init; }
    public required Guid PostId { get; init; }
    public required string Content { get; init; }
    public required string ContentHtml { get; init; }
    public required UserSummaryDto Author { get; init; }
    public Guid? ParentCommentId { get; init; }
    public required int VoteCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
```

#### TagDto

```csharp
namespace UniHub.Contracts.Forum;

/// <summary>
/// Tag for categorizing posts
/// </summary>
public sealed record TagDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public required int PostCount { get; init; }
}
```

---

### Domain Events

```csharp
namespace UniHub.Domain.Forum.Events;

/// <summary>
/// Post created (draft)
/// </summary>
public sealed record PostCreatedEvent(
    Guid PostId,
    Guid AuthorId,
    Guid CategoryId,
    string Title,
    bool IsConfession,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Post published (visible to public)
/// </summary>
public sealed record PostPublishedEvent(
    Guid PostId,
    Guid AuthorId,
    Guid CategoryId,
    string Title,
    DateTime PublishedAt
) : IDomainEvent;

/// <summary>
/// Post edited
/// </summary>
public sealed record PostEditedEvent(
    Guid PostId,
    Guid EditorId,
    DateTime EditedAt
) : IDomainEvent;

/// <summary>
/// Post deleted
/// </summary>
public sealed record PostDeletedEvent(
    Guid PostId,
    Guid DeletedById,
    DateTime DeletedAt
) : IDomainEvent;

/// <summary>
/// Comment added to post
/// </summary>
public sealed record CommentAddedEvent(
    Guid CommentId,
    Guid PostId,
    Guid AuthorId,
    Guid? ParentCommentId,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Post voted (upvote/downvote)
/// </summary>
public sealed record PostVotedEvent(
    Guid PostId,
    Guid UserId,
    VoteType VoteType,
    DateTime VotedAt
) : IDomainEvent;

/// <summary>
/// Post pinned by moderator
/// </summary>
public sealed record PostPinnedEvent(
    Guid PostId,
    Guid PinnedById,
    DateTime PinnedAt
) : IDomainEvent;
```

---

### Integration Events

```csharp
namespace UniHub.Contracts.Forum.Events;

/// <summary>
/// Published when post is published (for notifications)
/// </summary>
public sealed record PostPublishedIntegrationEvent(
    Guid PostId,
    Guid AuthorId,
    Guid CategoryId,
    string Title,
    string Preview,
    List<Guid> MentionedUserIds,
    DateTime PublishedAt
) : IIntegrationEvent;

/// <summary>
/// Published when comment is added (for notifications)
/// </summary>
public sealed record CommentAddedIntegrationEvent(
    Guid CommentId,
    Guid PostId,
    Guid AuthorId,
    Guid PostAuthorId,
    string CommentPreview,
    Guid? ParentCommentId,
    DateTime CreatedAt
) : IIntegrationEvent;
```

---

## 📚 LEARNING CONTEXT CONTRACTS

### Shared DTOs

#### DocumentDto

```csharp
namespace UniHub.Contracts.Learning;

/// <summary>
/// Learning document
/// </summary>
public sealed record DocumentDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required UserSummaryDto Uploader { get; init; }
    public required CourseDto Course { get; init; }
    public required FacultyDto Faculty { get; init; }
    public required string FileUrl { get; init; }
    public required string FileName { get; init; }
    public required long FileSize { get; init; }
    public required string FileType { get; init; }
    public required DocumentStatus Status { get; init; }
    public UserSummaryDto? Reviewer { get; init; }
    public required int DownloadCount { get; init; }
    public required double AverageRating { get; init; }
    public required int RatingCount { get; init; }
    public required DateTime UploadedAt { get; init; }
    public DateTime? ApprovedAt { get; init; }
}
```

#### CourseDto

```csharp
namespace UniHub.Contracts.Learning;

/// <summary>
/// Academic course/subject
/// </summary>
public sealed record CourseDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required FacultyDto Faculty { get; init; }
    public string? Description { get; init; }
    public required int Credits { get; init; }
}
```

#### FacultyDto

```csharp
namespace UniHub.Contracts.Learning;

/// <summary>
/// Academic faculty/department
/// </summary>
public sealed record FacultyDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Code { get; init; }
    public string? Description { get; init; }
}
```

---

### Domain Events

```csharp
namespace UniHub.Domain.Learning.Events;

/// <summary>
/// Document submitted for approval
/// </summary>
public sealed record DocumentSubmittedEvent(
    Guid DocumentId,
    Guid UploaderId,
    Guid CourseId,
    string Title,
    DateTime SubmittedAt
) : IDomainEvent;

/// <summary>
/// Document approved by moderator
/// </summary>
public sealed record DocumentApprovedEvent(
    Guid DocumentId,
    Guid UploaderId,
    Guid ApproverId,
    DateTime ApprovedAt
) : IDomainEvent;

/// <summary>
/// Document rejected
/// </summary>
public sealed record DocumentRejectedEvent(
    Guid DocumentId,
    Guid UploaderId,
    Guid RejectedById,
    string Reason,
    DateTime RejectedAt
) : IDomainEvent;

/// <summary>
/// Document downloaded
/// </summary>
public sealed record DocumentDownloadedEvent(
    Guid DocumentId,
    Guid DownloadedById,
    DateTime DownloadedAt
) : IDomainEvent;

/// <summary>
/// Document rated
/// </summary>
public sealed record DocumentRatedEvent(
    Guid DocumentId,
    Guid RatedById,
    int Rating,
    DateTime RatedAt
) : IDomainEvent;
```

---

### Integration Events

```csharp
namespace UniHub.Contracts.Learning.Events;

/// <summary>
/// Published when document is approved (for notification)
/// </summary>
public sealed record DocumentApprovedIntegrationEvent(
    Guid DocumentId,
    Guid UploaderId,
    Guid ApproverId,
    string Title,
    string CourseCode,
    DateTime ApprovedAt
) : IIntegrationEvent;

/// <summary>
/// Published when document is rejected (for notification)
/// </summary>
public sealed record DocumentRejectedIntegrationEvent(
    Guid DocumentId,
    Guid UploaderId,
    string Title,
    string Reason,
    DateTime RejectedAt
) : IIntegrationEvent;
```

---

## 💬 CHAT CONTEXT CONTRACTS

### Shared DTOs

#### ConversationDto

```csharp
namespace UniHub.Contracts.Chat;

/// <summary>
/// Chat conversation (direct or group)
/// </summary>
public sealed record ConversationDto
{
    public required Guid Id { get; init; }
    public required ConversationType Type { get; init; }
    public string? Name { get; init; }
    public string? AvatarUrl { get; init; }
    public required List<UserSummaryDto> Participants { get; init; }
    public MessageDto? LastMessage { get; init; }
    public required int UnreadCount { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? LastActivityAt { get; init; }
}
```

#### MessageDto

```csharp
namespace UniHub.Contracts.Chat;

/// <summary>
/// Chat message
/// </summary>
public sealed record MessageDto
{
    public required Guid Id { get; init; }
    public required Guid ConversationId { get; init; }
    public required UserSummaryDto Sender { get; init; }
    public required string Content { get; init; }
    public MessageType Type { get; init; } = MessageType.Text;
    public List<AttachmentDto>? Attachments { get; init; }
    public Guid? ReplyToMessageId { get; init; }
    public required DateTime SentAt { get; init; }
    public required bool IsRead { get; init; }
}
```

#### ChannelDto

```csharp
namespace UniHub.Contracts.Chat;

/// <summary>
/// Public chat channel (e.g., class channels)
/// </summary>
public sealed record ChannelDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required string Slug { get; init; }
    public string? Description { get; init; }
    public string? Icon { get; init; }
    public required int MemberCount { get; init; }
    public required DateTime CreatedAt { get; init; }
}
```

---

### Domain Events

```csharp
namespace UniHub.Domain.Chat.Events;

/// <summary>
/// Message sent in conversation
/// </summary>
public sealed record MessageSentEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    string Content,
    DateTime SentAt
) : IDomainEvent;

/// <summary>
/// Conversation created
/// </summary>
public sealed record ConversationCreatedEvent(
    Guid ConversationId,
    ConversationType Type,
    List<Guid> ParticipantIds,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Participant added to conversation
/// </summary>
public sealed record ParticipantAddedEvent(
    Guid ConversationId,
    Guid ParticipantId,
    Guid AddedById,
    DateTime AddedAt
) : IDomainEvent;

/// <summary>
/// User mentioned in message
/// </summary>
public sealed record UserMentionedEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid MentionedUserId,
    Guid MentionedById,
    DateTime MentionedAt
) : IDomainEvent;
```

---

### Integration Events

```csharp
namespace UniHub.Contracts.Chat.Events;

/// <summary>
/// Published when message is sent (for notifications)
/// </summary>
public sealed record MessageSentIntegrationEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid SenderId,
    string SenderName,
    string ContentPreview,
    List<Guid> RecipientIds,
    DateTime SentAt
) : IIntegrationEvent;

/// <summary>
/// Published when user is mentioned (for notifications)
/// </summary>
public sealed record UserMentionedIntegrationEvent(
    Guid MessageId,
    Guid ConversationId,
    Guid MentionedUserId,
    Guid MentionedById,
    string MentionedByName,
    string MessagePreview,
    DateTime MentionedAt
) : IIntegrationEvent;
```

---

## 💼 CAREER CONTEXT CONTRACTS

### Shared DTOs

#### JobPostingDto

```csharp
namespace UniHub.Contracts.Career;

/// <summary>
/// Job posting
/// </summary>
public sealed record JobPostingDto
{
    public required Guid Id { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public required CompanyDto Company { get; init; }
    public required string Location { get; init; }
    public required JobType JobType { get; init; }
    public required string SalaryRange { get; init; }
    public required List<string> Requirements { get; init; }
    public required List<string> Benefits { get; init; }
    public required List<TagDto> Tags { get; init; }
    public required int ApplicationCount { get; init; }
    public required JobStatus Status { get; init; }
    public required DateTime PostedAt { get; init; }
    public required DateTime ExpiresAt { get; init; }
}
```

#### CompanyDto

```csharp
namespace UniHub.Contracts.Career;

/// <summary>
/// Company profile
/// </summary>
public sealed record CompanyDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? LogoUrl { get; init; }
    public string? Description { get; init; }
    public string? Website { get; init; }
    public required string Industry { get; init; }
    public required int EmployeeCount { get; init; }
    public required bool IsVerified { get; init; }
}
```

#### ApplicationDto

```csharp
namespace UniHub.Contracts.Career;

/// <summary>
/// Job application
/// </summary>
public sealed record ApplicationDto
{
    public required Guid Id { get; init; }
    public required Guid JobPostingId { get; init; }
    public required string JobTitle { get; init; }
    public required UserSummaryDto Applicant { get; init; }
    public required string CoverLetter { get; init; }
    public required string ResumeUrl { get; init; }
    public required ApplicationStatus Status { get; init; }
    public required DateTime AppliedAt { get; init; }
    public DateTime? ReviewedAt { get; init; }
}
```

---

### Domain Events

```csharp
namespace UniHub.Domain.Career.Events;

/// <summary>
/// Job posting published
/// </summary>
public sealed record JobPostedEvent(
    Guid JobId,
    Guid CompanyId,
    string Title,
    List<string> Tags,
    DateTime PostedAt
) : IDomainEvent;

/// <summary>
/// Application submitted
/// </summary>
public sealed record ApplicationSubmittedEvent(
    Guid ApplicationId,
    Guid JobId,
    Guid ApplicantId,
    DateTime SubmittedAt
) : IDomainEvent;

/// <summary>
/// Application status changed
/// </summary>
public sealed record ApplicationStatusChangedEvent(
    Guid ApplicationId,
    Guid ApplicantId,
    ApplicationStatus OldStatus,
    ApplicationStatus NewStatus,
    DateTime ChangedAt
) : IDomainEvent;
```

---

### Integration Events

```csharp
namespace UniHub.Contracts.Career.Events;

/// <summary>
/// Published when job is posted (for notifications to matching users)
/// </summary>
public sealed record JobPostedIntegrationEvent(
    Guid JobId,
    Guid CompanyId,
    string Title,
    string Description,
    List<string> Tags,
    DateTime PostedAt
) : IIntegrationEvent;

/// <summary>
/// Published when application status changes (for applicant notification)
/// </summary>
public sealed record ApplicationStatusChangedIntegrationEvent(
    Guid ApplicationId,
    Guid ApplicantId,
    Guid JobId,
    string JobTitle,
    ApplicationStatus NewStatus,
    DateTime ChangedAt
) : IIntegrationEvent;
```

---

## 🔔 NOTIFICATION CONTEXT CONTRACTS

### Shared DTOs

#### NotificationDto

```csharp
namespace UniHub.Contracts.Notification;

/// <summary>
/// Notification sent to user
/// </summary>
public sealed record NotificationDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Title { get; init; }
    public required string Body { get; init; }
    public required NotificationType Type { get; init; }
    public Dictionary<string, string>? Data { get; init; }
    public required bool IsRead { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
}
```

#### NotificationPreferenceDto

```csharp
namespace UniHub.Contracts.Notification;

/// <summary>
/// User notification preferences
/// </summary>
public sealed record NotificationPreferenceDto
{
    public required Guid UserId { get; init; }
    public required bool EmailEnabled { get; init; }
    public required bool PushEnabled { get; init; }
    public required bool InAppEnabled { get; init; }
    public required Dictionary<string, bool> CategoryPreferences { get; init; }
}
```

---

### Domain Events

```csharp
namespace UniHub.Domain.Notification.Events;

/// <summary>
/// Notification created
/// </summary>
public sealed record NotificationCreatedEvent(
    Guid NotificationId,
    Guid UserId,
    NotificationType Type,
    DateTime CreatedAt
) : IDomainEvent;

/// <summary>
/// Notification read
/// </summary>
public sealed record NotificationReadEvent(
    Guid NotificationId,
    Guid UserId,
    DateTime ReadAt
) : IDomainEvent;
```

---

### Integration Events

```csharp
namespace UniHub.Contracts.Notification.Events;

/// <summary>
/// Request to send notification to single user
/// </summary>
public sealed record SendNotificationCommand(
    Guid UserId,
    string Title,
    string Body,
    NotificationType Type,
    Dictionary<string, string>? Data = null
) : IIntegrationEvent;

/// <summary>
/// Request to send notification to multiple users
/// </summary>
public sealed record SendBulkNotificationCommand(
    List<Guid> UserIds,
    string Title,
    string Body,
    NotificationType Type,
    Dictionary<string, string>? Data = null
) : IIntegrationEvent;
```

---

## 🤖 AI CONTEXT CONTRACTS

### Shared DTOs

#### ChatCompletionDto

```csharp
namespace UniHub.Contracts.AI;

/// <summary>
/// AI chatbot completion
/// </summary>
public sealed record ChatCompletionDto
{
    public required string Response { get; init; }
    public required string Provider { get; init; }
    public required int TokensUsed { get; init; }
    public required DateTime CompletedAt { get; init; }
}
```

#### ModerationResultDto

```csharp
namespace UniHub.Contracts.AI;

/// <summary>
/// Content moderation result
/// </summary>
public sealed record ModerationResultDto
{
    public required bool IsFlagged { get; init; }
    public required List<string> Categories { get; init; }
    public required double ConfidenceScore { get; init; }
    public string? Reason { get; init; }
}
```

---

## 🔧 COMMON CONTRACTS

### Result Pattern

```csharp
namespace UniHub.Contracts.Common;

/// <summary>
/// Generic result wrapper
/// </summary>
public sealed record Result<T>
{
    public required bool IsSuccess { get; init; }
    public T? Value { get; init; }
    public Error? Error { get; init; }

    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value,
        Error = null
    };

    public static Result<T> Failure(Error error) => new()
    {
        IsSuccess = false,
        Value = default,
        Error = error
    };
}

/// <summary>
/// Error details
/// </summary>
public sealed record Error(
    string Code,
    string Message,
    ErrorType Type = ErrorType.Failure
);

public enum ErrorType
{
    Failure,
    Validation,
    NotFound,
    Conflict,
    Unauthorized
}
```

---

### Pagination

```csharp
namespace UniHub.Contracts.Common;

/// <summary>
/// Paged result wrapper
/// </summary>
public sealed record PagedResult<T>
{
    public required List<T> Items { get; init; }
    public required int PageNumber { get; init; }
    public required int PageSize { get; init; }
    public required int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

/// <summary>
/// Pagination query parameters
/// </summary>
public sealed record PagedQuery
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SortBy { get; init; }
    public string? SortOrder { get; init; } = "desc";
}
```

---

## 📐 CONTRACT DESIGN PRINCIPLES

### 1. **Stability Over Flexibility**

❌ **Bad**: Frequent breaking changes
```csharp
// Version 1
public record UserDto(Guid Id, string Name);

// Version 2 - BREAKING CHANGE
public record UserDto(Guid Id, string FirstName, string LastName);
```

✅ **Good**: Additive changes only
```csharp
// Version 1
public record UserDto
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
}

// Version 2 - NON-BREAKING
public record UserDto
{
    public required Guid Id { get; init; }
    public required string DisplayName { get; init; }
    public string? FirstName { get; init; } // NEW: Optional
    public string? LastName { get; init; }  // NEW: Optional
}
```

---

### 2. **Self-Contained DTOs**

❌ **Bad**: Requires multiple API calls
```csharp
public record PostDto
{
    public Guid AuthorId { get; init; } // Client must fetch author separately
}
```

✅ **Good**: Includes necessary data
```csharp
public record PostDto
{
    public UserSummaryDto Author { get; init; } // All needed info included
}
```

---

### 3. **Minimal Coupling**

❌ **Bad**: Exposing internal details
```csharp
public record UserDto
{
    public string PasswordHash { get; init; } // NEVER expose this!
    public string RefreshToken { get; init; } // Internal detail
}
```

✅ **Good**: Only public-facing data
```csharp
public record UserDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; }
    public bool IsVerified { get; init; }
}
```

---

### 4. **Versioning Strategy**

When breaking changes are unavoidable:

```csharp
namespace UniHub.Contracts.Identity.V1;
public record UserDto { ... }

namespace UniHub.Contracts.Identity.V2;
public record UserDto { ... }
```

API versioning:
```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
public class UsersController : ControllerBase
{
    [HttpGet("{id}")]
    [MapToApiVersion("1.0")]
    public async Task<V1.UserDto> GetV1(Guid id) { ... }
    
    [HttpGet("{id}")]
    [MapToApiVersion("2.0")]
    public async Task<V2.UserDto> GetV2(Guid id) { ... }
}
```

---

## 🔄 EVENT-DRIVEN COMMUNICATION

### MediatR Configuration (Current - Monolith)

```csharp
// Publishing domain event
public class Post : AggregateRoot
{
    public void Publish()
    {
        AddDomainEvent(new PostPublishedEvent(Id, AuthorId, CategoryId, Title, DateTime.UtcNow));
        Status = PostStatus.Published;
    }
}

// Handler in Forum Context
public class PostPublishedEventHandler : INotificationHandler<PostPublishedEvent>
{
    public Task Handle(PostPublishedEvent @event, CancellationToken ct)
    {
        // Update read model
        // Invalidate cache
        // Publish integration event
    }
}

// Handler in Notification Context
public class PostPublishedNotificationHandler : INotificationHandler<PostPublishedIntegrationEvent>
{
    public Task Handle(PostPublishedIntegrationEvent @event, CancellationToken ct)
    {
        // Send notifications to subscribers
    }
}
```

---

### Message Bus (Future - Microservices)

```csharp
// RabbitMQ / Azure Service Bus
public class PostPublishedIntegrationEventHandler : IIntegrationEventHandler<PostPublishedIntegrationEvent>
{
    private readonly IMessageBus _messageBus;
    
    public async Task Handle(PostPublishedIntegrationEvent @event)
    {
        // Publish to message bus
        await _messageBus.PublishAsync(@event, "forum.post.published");
    }
}

// Subscriber in Notification Service
public class NotificationSubscriber
{
    [Topic("forum.post.published")]
    public async Task OnPostPublished(PostPublishedIntegrationEvent @event)
    {
        // Send notifications
    }
}
```

---

## 📚 REFERENCES

- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/)
- [Result Pattern - Vladimir Khorikov](https://enterprisecraftsmanship.com/)
- [Event-Driven Architecture - Martin Fowler](https://martinfowler.com/articles/201701-event-driven.html)
- [GLOSSARY.md](./GLOSSARY.md)
- [BOUNDED_CONTEXTS.md](./BOUNDED_CONTEXTS.md)
- [CONTEXT_MAP.md](./CONTEXT_MAP.md)

---

## 🔄 VERSIONING

| Version | Date       | Changes                        | Author |
| ------- | ---------- | ------------------------------ | ------ |
| 1.0     | 2026-02-04 | Initial integration contracts  | Agent  |

---

_Last Updated: 2026-02-04_
