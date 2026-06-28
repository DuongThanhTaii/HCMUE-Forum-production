# Assistant tools API (`ApiResponse` Envelope)

Các endpoint **gắn domain Forum** (post, comment, search) với AI — đặt tại **`UniHub.API`** (`AssistantToolsController`), khác namespace `/api/v1/ai/*` của module AI thuần.

## Base URL

- `/api/v1/assistant/tools`

## Auth

- **Bắt buộc** JWT Bearer.

## Rate limiting

- `[EnableRateLimiting("ai")]`

## Phụ thuộc AI

- Cần ít nhất một **LLM provider** cấu hình trong `AIProviders` (xem `appsettings.AI.example.json`). Nếu không có provider, các tool gọi LLM sẽ trả lỗi kiểu “AI service is temporarily unavailable” / failure envelope.

---

## `POST /api/v1/assistant/tools/summarize-post`

Tóm tắt một bài Forum kèm một phần comment.

### Body

```json
{
  "postId": "guid",
  "length": "medium"
}
```

- `length` (optional): `very-short` | `short` | `medium` | `long` | `detailed` — không khớp thì mặc định `medium`.

### `200` — `data`: `SummarizePostResponse`

| Field | Kiểu |
|-------|------|
| `postId` | guid |
| `summary` | string |
| `keyPoints` | string[] |
| `commentCount` | int |
| `generatedAt` | datetime |

### Lỗi

- `400` — tóm tắt thất bại / comment query lỗi
- `404` — không có post

---

## `POST /api/v1/assistant/tools/related-posts`

Gợi ý bài **liên quan** dựa trên `postId` (title + tags) hoặc `query` tự do; dùng `SearchPostsQuery` phía server.

### Body

```json
{
  "postId": "guid | null",
  "query": "string | null",
  "limit": 5
}
```

- Cần **`postId`** hoặc **`query`** không rỗng.
- `limit` clamp 1–20.

### `200` — `data`: `RelatedPostsResponse`

| Field | Kiểu |
|-------|------|
| `query` | string — query đã dùng nội bộ |
| `items` | `RelatedPostItem[]` |

**`RelatedPostItem`**

| Field | Mô tả |
|-------|--------|
| `id`, `title`, `slug` | Post |
| `searchRank` | number |
| `reason` | Giải thích ngắn (rank, tags) |
| `citationUrl` | Path kiểu `/forum/{postId}` để deep link |

---

## `POST /api/v1/assistant/tools/draft-reply`

Sinh bản nháp reply Markdown cho một post.

### Body

```json
{
  "postId": "guid",
  "intent": "answer",
  "tone": "friendly"
}
```

### `200` — `data`: `DraftReplyResponse`

- `postId`, `intent`, `tone`, `draftMarkdown`, `generatedAt`

---

## `POST /api/v1/assistant/tools/suggest-title-tags`

Gợi ý tiêu đề + tags từ title/content hiện có.

### Body

```json
{
  "title": "",
  "content": "",
  "maxTags": 5
}
```

- Ít nhất một trong `title`, `content` khác rỗng.
- `maxTags` clamp 1–8.

### `200` — `data`: `SuggestTitleTagsResponse`

- `suggestedTitle`, `suggestedTags[]`, `rationale`, `generatedAt`

---

## `POST /api/v1/assistant/tools/rewrite-content`

Viết lại nội dung theo style.

### Body

```json
{
  "title": "",
  "content": "required",
  "style": "clear"
}
```

### `200` — `data`: `RewriteContentResponse`

- `style`, `rewrittenContent`, `generatedAt`

---

## `POST /api/v1/assistant/tools/moderation-hint`

Gợi ý điều phối moderatio (an toàn / cần review / block) — **gợi ý**, quyết định cuối vẫn thuộc mod + API Forum.

### Body

```json
{ "postId": "guid" }
```

### `200` — `data`: `ModerationHintResponse`

| Field | Kiểu |
|-------|------|
| `postId` | guid |
| `isSafe` | bool |
| `requiresReview` | bool |
| `isBlocked` | bool |
| `recommendation` | string — ví dụ `allow` / `review` / `hide` |
| `reason` | string |
| `violations` | `ModerationViolationItem[]` |
| `generatedAt` | datetime |

**`ModerationViolationItem`:** `type`, `severity`, `confidence`, `description`

---

## FE đã tích hợp

File `frontend/src/features/assistant/api/assistant.api.ts` — các hook `useSummarizePostMutation`, `useRelatedPostsMutation`, v.v.

So sánh với module AI tổng quát: [ai-fe-api.md](./ai-fe-api.md).
