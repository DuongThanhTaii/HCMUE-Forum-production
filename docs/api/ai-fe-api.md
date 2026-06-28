# AI API for Frontend (`ApiResponse` Envelope)

## Liên quan: Assistant tools (Forum)

Các endpoint **tích hợp post/search + AI** nằm dưới **`/api/v1/assistant/tools`** (controller trong host API), rate limit `ai`, JWT bắt buộc — xem **[assistant-tools-fe-api.md](./assistant-tools-fe-api.md)**.

---

## Base URL
- `/api/v1/ai`

## Auth
- Required (JWT Bearer) for all endpoints.

## Envelope
```json
{
  "success": true,
  "data": {},
  "message": "optional success message",
  "error": null
}
```

Error sample:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "error": "Human readable message"
}
```

## Chat

### `POST /api/v1/ai/chat`
- **Body**: `ChatRequest` (requires `message`)
- **200**: `ApiResponse<ChatResponse>`
- **400**: failure envelope (`Message is required`, validation errors)

### `GET /api/v1/ai/conversations?userId=`
- **Query**: `userId` (required, GUID)
- **200**: `ApiResponse<IReadOnlyList<Conversation>>`
- **400**: failure envelope (missing/invalid userId)

### `GET /api/v1/ai/conversations/{id}`
- **200**: `ApiResponse<Conversation>`
- **404**: failure envelope

### `DELETE /api/v1/ai/conversations/{id}`
- **200**: `ApiResponse<null>` with message `Conversation deleted successfully`
- **404**: failure envelope

## Content Moderation

### `POST /api/v1/ai/moderate`
- **Body**: `ModerationRequest` (requires `content`)
- **200**: `ApiResponse<ModerationResponse>`

### `GET /api/v1/ai/moderate/check?content=`
- **Query**: `content` (required)
- **200**: `ApiResponse<{ isSafe, riskScore, isBlocked, requiresReview }>`

## Smart Search

### `GET /api/v1/ai/search`
- **Query**: `q` (required), `type`, `category`, `page`, `pageSize`, `suggestions`
- **200**: `ApiResponse<SearchResponse>`

### `POST /api/v1/ai/search`
- **Body**: `SearchRequest`
- **200**: `ApiResponse<SearchResponse>`

### `GET /api/v1/ai/search/suggestions?q=&limit=`
- **Query**: `q` (required), `limit` (optional)
- **200**: `ApiResponse<{ query, suggestions }>`

### `GET /api/v1/ai/search/understand?q=`
- **Query**: `q` (required)
- **200**: `ApiResponse<QueryUnderstanding>`

## Summarization

### `POST /api/v1/ai/summarize`
- **Body**: `SummarizationRequest` (requires `content`)
- **200**: `ApiResponse<SummarizationResponse>`

### `POST /api/v1/ai/summarize/keypoints`
- **Body**: `{ content, maxPoints }`
- **200**: `ApiResponse<{ keyPoints }>`

### `GET /api/v1/ai/summarize/detect-language?content=`
- **Query**: `content` (required)
- **200**: `ApiResponse<{ language }>`

### `DELETE /api/v1/ai/summarize/cache?cacheKey=`
- **Query**: `cacheKey` (optional)
- **200**: `ApiResponse<null>` with message `Cache cleared successfully`

## Schemas

### `ChatRequest`
- `message` (string)
- `conversationId` (guid | null, optional)
- `userId` (guid | null, optional)
- `sessionId` (string | null, optional)
- `includeHistory` (boolean, optional; default `true`)
- `maxHistoryMessages` (int, optional; default `10`)

### `ChatResponse`
- `isSuccess` (boolean)
- `message` (string)
- `conversationId` (guid)
- `messageId` (guid)
- `confidenceScore` (number | null)
- `sourceFAQ` (FAQItemDto | null)
- `suggestedQuestions` (string[])
- `suggestHandoff` (boolean)
- `handoffReason` (string | null)
- `errorMessage` (string | null)
- `timestamp` (datetime)

### `FAQItemDto`
- `id` (guid)
- `question` (string)
- `answer` (string)
- `category` (string)
- `tags` (string[])
- `priority` (int)
- `usageCount` (int)
- `averageRating` (number | null)

### `Conversation`
- `id` (guid)
- `userId` (guid | null)
- `sessionId` (string | null)
- `title` (string | null)
- `messages` (ConversationMessage[])
- `handedOffToSupport` (boolean)
- `handoffReason` (string | null)
- `supportAgentId` (guid | null)
- `startedAt` (datetime)
- `lastActiveAt` (datetime)
- `isClosed` (boolean)
- `closedAt` (datetime | null)

### `ConversationMessage`
- `id` (guid)
- `conversationId` (guid)
- `role` (string)
- `content` (string)
- `sourceFAQId` (guid | null)
- `confidenceScore` (number | null)
- `isHelpful` (boolean | null)
- `sentAt` (datetime)

### `ModerationRequest`
- `content` (string)
- `contentType` (string enum)
- `userId` (guid | null)
- `context` (string | null)

### `ModerationResponse`
- `isSuccess` (boolean)
- `isSafe` (boolean)
- `requiresReview` (boolean)
- `isBlocked` (boolean)
- `confidenceScore` (number)
- `violations` (ModerationViolation[])
- `reason` (string | null)
- `errorMessage` (string | null)
- `timestamp` (datetime)

### `ModerationViolation`
- `type` (string)
- `severity` (number)
- `confidence` (number)
- `description` (string)

### `SearchRequest`
- `query` (string)
- `searchType` (string enum)
- `category` (string | null)
- `tags` (string[] | null)
- `startDate` (datetime | null)
- `endDate` (datetime | null)
- `userId` (string | null)
- `page` (int)
- `pageSize` (int)
- `includeSuggestions` (boolean)
- `minRelevanceScore` (number)
- `language` (string | null)

### `SearchResponse`
- `results` (SearchResult[])
- `totalCount` (int)
- `page` (int)
- `pageSize` (int)
- `totalPages` (int)
- `suggestions` (string[])
- `queryUnderstanding` (QueryUnderstanding | null)
- `processingTimeMs` (long)
- `timestamp` (datetime)

### `SearchResult`
- `id` (string)
- `contentType` (string)
- `title` (string)
- `snippet` (string)
- `relevanceScore` (number)
- `url` (string)
- `author` (string | null)
- `createdAt` (datetime)
- `category` (string | null)
- `tags` (string[] | null)
- `viewCount` (int | null)
- `metadata` (object | null)

### `QueryUnderstanding`
- `originalQuery` (string)
- `expandedQuery` (string)
- `intent` (string)
- `entities` (string[])
- `language` (string)
- `suggestedCorrection` (string | null)

### `SummarizationRequest`
- `content` (string)
- `documentUrl` (string | null)
- `title` (string | null)
- `documentType` (string enum)
- `length` (string enum)
- `targetLanguage` (string | null)
- `sourceLanguage` (string | null)
- `maxTokens` (int | null)
- `enableCaching` (boolean)
- `userId` (guid | null)

### `SummarizationResponse`
- `isSuccess` (boolean)
- `summary` (string)
- `detectedLanguage` (string | null)
- `keyPoints` (string[])
- `originalLength` (int)
- `summaryLength` (int)
- `compressionRatio` (number)
- `tokensUsed` (int | null)
- `fromCache` (boolean)
- `cacheKey` (string | null)
- `errorMessage` (string | null)
- `timestamp` (datetime)
- `processingTimeMs` (long)

### `KeyPointsRequest`
- `content` (string)
- `maxPoints` (int)
