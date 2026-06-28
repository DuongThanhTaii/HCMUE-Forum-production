# Forum API for Frontend (`ApiResponse` Envelope)

## Base URL
- `/api/v1`

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

## Auth
- Anonymous: read/list/search endpoints.
- Required: create/update/delete/vote/bookmark/report/comment actions.
- Role: pin post requires `Admin` or `Moderator`.

## Posts

### `GET /api/v1/posts`
- **Auth**: anonymous
- **Query**: `pageNumber`, `pageSize`, `categoryId`, `type`, `status`
- **200**: `ApiResponse<PostListResponse>`

### `GET /api/v1/posts/{id}`
- **Auth**: anonymous
- **200**: `ApiResponse<PostResponse>`
- **404**: failure envelope

### `POST /api/v1/posts`
- **Auth**: required
- **Body**: `CreatePostRequest` (`title`, `content`, `type`, `categoryId`, `tags`)
- **201**: `ApiResponse<{ postId }>` with message `Post created successfully`

### `PUT /api/v1/posts/{id}`
- **Auth**: required
- **Body**: `UpdatePostRequest` (`title?`, `content?`, `categoryId?`, `tags?`)
- **200**: `ApiResponse<null>` with message `Post updated successfully`

### `DELETE /api/v1/posts/{id}`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Post deleted successfully`

### `POST /api/v1/posts/{id}/publish`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Post published successfully`

### `POST /api/v1/posts/{id}/pin`
- **Auth**: required (Admin/Moderator)
- **200**: `ApiResponse<null>` with message `Post pin state updated successfully`

### `POST /api/v1/posts/{id}/vote`
- **Auth**: required
- **Body**: `{ voteType }` where `voteType` = `1` (upvote) or `2` (downvote)
- **200**: `ApiResponse<null>` with message `Post voted successfully`

### `GET /api/v1/posts/{id}/comments`
- **Auth**: anonymous
- **Query**: `pageNumber`, `pageSize`
- **200**: `ApiResponse<CommentListResponse>`

### `POST /api/v1/posts/{id}/bookmark`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Post bookmarked successfully`

### `DELETE /api/v1/posts/{id}/bookmark`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Post unbookmarked successfully`

### `POST /api/v1/posts/{id}/report`
- **Auth**: required
- **Body**: `{ reason, description? }`
- **201**: `ApiResponse<{ reportId }>` with message `Post reported successfully`

## Comments

### `POST /api/v1/comments/posts/{postId}`
- **Auth**: required
- **Body**: `{ content, parentCommentId? }`
- **201**: `ApiResponse<{ commentId }>` with message `Comment created successfully`

### `PUT /api/v1/comments/{id}`
- **Auth**: required
- **Body**: `UpdateCommentRequest` (`content`)
- **200**: `ApiResponse<null>` with message `Comment updated successfully`

### `DELETE /api/v1/comments/{id}`
- **Auth**: required
- **200**: `ApiResponse<null>` with message `Comment deleted successfully`

### `POST /api/v1/comments/{id}/vote`
- **Auth**: required
- **Body**: `{ voteType }` where `voteType` = `1` (upvote) or `2` (downvote)
- **200**: `ApiResponse<null>` with message `Comment voted successfully`

### `POST /api/v1/comments/{id}/accept?postId=`
- **Auth**: required
- **Query**: `postId` (required)
- **200**: `ApiResponse<null>` with message `Answer accepted successfully`

### `POST /api/v1/comments/{id}/report`
- **Auth**: required
- **Body**: `{ reason, description? }`
- **201**: `ApiResponse<{ reportId }>` with message `Comment reported successfully`

## Tags

### `GET /api/v1/tags`
- **Auth**: anonymous
- **Query**: `pageNumber`, `pageSize`, `searchTerm`, `orderByUsage`
- **200**: `ApiResponse<TagListResponse>`

### `GET /api/v1/tags/popular?count=`
- **Auth**: anonymous
- **Query**: `count` (optional, default 10)
- **200**: `ApiResponse<List<PopularTagResponse>>`

## Search

### `GET /api/v1/search?q=`
- **Auth**: anonymous
- **Query**: `q` (required), `categoryId`, `postType`, `tags` (comma-separated), `pageNumber`, `pageSize`
- **200**: `ApiResponse<SearchPostsResponse>`

## Notes

- FE hiện map `authorName` theo fallback từ `authorId` nếu API comments chưa trả profile join.
- Cache refresh dùng RTK Query invalidation tags (`ForumPost`, `Comment`) cho các luồng comment/vote/bookmark/report để đồng bộ list/detail.

## Schemas

### `PostListResponse`
- `posts` (PostResponse[])
- `totalCount` (int)
- `pageNumber` (int)
- `pageSize` (int)
- `totalPages` (int)
- `hasPreviousPage` (boolean)
- `hasNextPage` (boolean)

### `PostResponse`
- `id` (guid)
- `title` (string)
- `content` (string)
- `slug` (string)
- `type` (int)
- `status` (int)
- `authorId` (guid)
- `categoryId` (guid | null)
- `tags` (string[])
- `voteScore` (int)
- `commentCount` (int)
- `isPinned` (boolean)
- `createdAt` (datetime)
- `updatedAt` (datetime | null)
- `publishedAt` (datetime | null)

### `CreatePostRequest`
- `title` (string)
- `content` (string)
- `type` (int)
- `categoryId` (guid | null)
- `tags` (string[] | null)

### `UpdatePostRequest`
- `title` (string | null)
- `content` (string | null)
- `categoryId` (guid | null)
- `tags` (string[] | null)

### `CommentListResponse`
- `comments` (CommentResponse[])
- `totalCount` (int)
- `pageNumber` (int)
- `pageSize` (int)
- `totalPages` (int)
- `hasPreviousPage` (boolean)
- `hasNextPage` (boolean)

### `CommentResponse`
- `id` (guid)
- `postId` (guid)
- `authorId` (guid)
- `content` (string)
- `parentCommentId` (guid | null)
- `voteScore` (int)
- `isAcceptedAnswer` (boolean)
- `createdAt` (datetime)
- `updatedAt` (datetime | null)

### `AddCommentRequest`
- `content` (string)
- `parentCommentId` (guid | null)

### `UpdateCommentRequest`
- `content` (string)

### `TagResponse`
- `name` (string)
- `postCount` (int)

### `PopularTagResponse`
- `name` (string)
- `postCount` (int)
- `popularityScore` (number)

### `VoteRequest`
- `voteType` (int)

### `ReportRequest`
- `reason` (int)
- `description` (string | null)

---

## Moderation Routes (`/api/v1/mod`)

> **Auth:** permission **`forum.reports.review`** (RBAC). Controller dùng `[RequirePermission("forum.reports.review")]`, không chỉ kiểm tra tên role.
>
> **Phạm vi:** Admin xử lý toàn hệ thống. Moderator lọc theo category được gán (`Category.ModeratorIds`). *Hiện tại* nếu moderator chưa được gán category, backend có hotfix không lọc rỗng — sau khi quy trình gán mod ổn định có thể thắt chặt (xem `ModerationController.ResolveScopedCategoryIdsAsync`).

### `GET /api/v1/mod/reports`

- **Auth**: user có `forum.reports.review`
- **Query**: `status` (`pending` | `resolved_keep` | `resolved_remove`), `pageNumber`, `pageSize`
- **200**: `ApiResponse<ModerationReportListResponse>`

**Moderator scope behaviour:**
- **Admin** — returns all reports across all categories.
- **Moderator** — returns only reports for posts/comments whose `CategoryId` is in the categories where the moderator appears in `Category.ModeratorIds`.
- Moderator with **no** category assignments receives an **empty** list.

---

### `POST /api/v1/mod/reports/{id}/resolve`

- **Auth**: user có `forum.reports.review`
- **Body**: `ResolveModerationReportRequest` (`action: "keep" | "remove"`)
- **200**: `ApiResponse<null>` with message `Report resolved successfully`
- **403**: Moderator attempted to resolve a report outside their category scope, or the reported target no longer exists.
- **404**: Report not found.
- **409**: Report already resolved.

**Action semantics:**
| `action` | Effect |
|----------|--------|
| `"keep"` | Marks report resolved; content remains |
| `"remove"` | Marks report resolved; soft-deletes the post or comment |

---

### `GET /api/v1/mod/posts`

- **Auth**: user có `forum.reports.review`
- **Query**: `pageNumber`, `pageSize`
- **200**: `ApiResponse<PostListResponse>` — draft posts only, scoped to moderator's categories.

---

## Schemas (Moderation)

### `ModerationReportListResponse`
- `reports` (ModerationReportResponse[])
- `totalCount` (int)
- `pageNumber` (int)
- `pageSize` (int)
- `totalPages` (int)
- `hasPreviousPage` (boolean)
- `hasNextPage` (boolean)

### `ModerationReportResponse`
- `id` (int)
- `reportedItemId` (guid)
- `reportedItemType` (int) — `1` = Post, `2` = Comment
- `reporterId` (guid)
- `reason` (int) — see `ReportReason` enum: `1` Spam, `2` Hate speech, `3` Harassment, `4` Misinformation, `5` Inappropriate content, `6` Other
- `description` (string | null)
- `status` (int) — `0` Pending, `1` Resolved, `2` Dismissed
- `createdAt` (datetime)
- `reviewedAt` (datetime | null)
- `reviewedBy` (guid | null)
- `resolutionDecision` (`"keep"` | `"remove"` | null)
- `titlePreview` (string | null) — trimmed title if target is a Post
- `contentPreview` (string | null) — trimmed content of Post or Comment
- `isTargetDeleted` (boolean) — `true` if the reported entity was deleted before/after report creation

### `ResolveModerationReportRequest`
- `action` (`"keep"` | `"remove"`)

