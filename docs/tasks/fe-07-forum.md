# FE-07: Forum Module

| Property | Value |
|---|---|
| **ID** | FE-07 |
| **Branch** | `feature/FE-07-forum` |
| **Commit** | `feat(fe/forum): implement forum post list, detail, create, edit, voting, comments` |
| **Priority** | Critical |
| **Estimate** | 16h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03, FE-06 |

---

## API Endpoints

| Action | Method | Endpoint |
|---|---|---|
| List posts | GET | `/api/v1/forum/posts?page=1&pageSize=20&sort=newest&tag=xxx&search=xxx` |
| Get post | GET | `/api/v1/forum/posts/{id}` |
| Create post | POST | `/api/v1/forum/posts` |
| Update post | PUT | `/api/v1/forum/posts/{id}` |
| Delete post | DELETE | `/api/v1/forum/posts/{id}` |
| Publish post | POST | `/api/v1/forum/posts/{id}/publish` |
| Pin post | POST | `/api/v1/forum/posts/{id}/pin` |
| Vote post | POST | `/api/v1/forum/posts/{id}/vote` body: `{ type: "Up" \| "Down" }` |
| Bookmark | POST | `/api/v1/forum/posts/{id}/bookmark` |
| Unbookmark | DELETE | `/api/v1/forum/posts/{id}/bookmark` |
| Report post | POST | `/api/v1/forum/posts/{id}/report` |
| Get comments | GET | `/api/v1/forum/posts/{id}/comments` |
| Add comment | POST | `/api/v1/forum/posts/{id}/comments` |
| Update comment | PUT | `/api/v1/forum/comments/{id}` |
| Delete comment | DELETE | `/api/v1/forum/comments/{id}` |
| Vote comment | POST | `/api/v1/forum/comments/{id}/vote` |
| Accept answer | POST | `/api/v1/forum/comments/{id}/accept` |
| Report comment | POST | `/api/v1/forum/comments/{id}/report` |
| Search posts | GET | `/api/v1/forum/posts/search?q=xxx` |
| Get tags | GET | `/api/v1/forum/tags` |
| Popular tags | GET | `/api/v1/forum/tags/popular` |
| Bookmarked posts | GET | `/api/v1/forum/posts/bookmarked` |

---

## Pages

## Tailwind Layout Trees

### PostCard tree

```
PostCard [group relative flex flex-col rounded-xl border border-border bg-card shadow-card cursor-pointer hover:shadow-card-hover transition-shadow duration-200 overflow-hidden]
├── PinStripe? [absolute left-0 top-0 bottom-0 w-1 bg-primary rounded-l-xl]   ← chỉ khi isPinned
├── CardHeader [flex items-start gap-3 p-4 pb-2]
│   ├── Avatar [w-8 h-8 rounded-full flex-shrink-0 overflow-hidden]
│   ├── AuthorMeta [flex-1 min-w-0]
│   │   ├── AuthorRow [flex items-center gap-1.5 flex-wrap]
│   │   │   ├── AuthorName [text-sm font-medium text-foreground truncate]
│   │   │   └── BadgeChip? [inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded text-[10px] bg-primary/10 text-primary font-medium]
│   │   └── Timestamp [text-xs text-muted]
│   └── PinIcon? [w-4 h-4 text-primary flex-shrink-0 mt-0.5]
├── CardBody [px-4 pb-3]
│   ├── Title [text-base font-heading font-semibold text-foreground line-clamp-2 mb-1.5 group-hover:text-primary transition-colors]
│   └── Excerpt [text-sm text-muted line-clamp-2 leading-relaxed]
└── CardFooter [flex items-center justify-between px-4 py-2.5 border-t border-border/60]
    ├── TagList [flex items-center gap-1.5 flex-wrap min-w-0]
    │   └── Tag (×n) [px-2 py-0.5 rounded-sm text-[11px] bg-primary/8 text-primary/80 truncate max-w-[100px]]
    └── Stats [flex items-center gap-3 text-muted text-xs shrink-0]
        ├── VoteStat [flex items-center gap-1]
        │   ├── ArrowUpIcon [w-3 h-3]
        │   └── Count [tabular-nums]
        └── CommentStat [flex items-center gap-1]
            ├── MessageIcon [w-3 h-3]
            └── Count [tabular-nums]
```

### Post Detail tree

```
PostDetailPage [flex gap-6 max-w-screen-xl mx-auto]
├── PostMain [flex-1 min-w-0]
│   ├── BackButton [flex items-center gap-1.5 text-sm text-muted hover:text-foreground cursor-pointer mb-4 transition-colors]
│   ├── PostCard [rounded-xl border border-border bg-card p-6]
│   │   ├── PostHeader [flex items-start gap-3 mb-4]
│   │   │   ├── VoteColumn [flex flex-col items-center gap-1 mr-2 shrink-0]
│   │   │   │   ├── VoteUp [w-8 h-8 rounded-lg flex items-center justify-center cursor-pointer transition-colors]
│   │   │   │   │   ├── [voted]   [bg-primary/15 text-primary]
│   │   │   │   │   └── [default] [text-muted hover:bg-muted/60 hover:text-foreground]
│   │   │   │   ├── VoteCount [text-sm font-semibold tabular-nums text-foreground]
│   │   │   │   └── VoteDown [w-8 h-8 rounded-lg flex items-center justify-center cursor-pointer transition-colors]
│   │   │   │       ├── [voted]   [bg-destructive/15 text-destructive]
│   │   │   │       └── [default] [text-muted hover:bg-muted/60 hover:text-foreground]
│   │   │   └── PostMeta [flex-1 min-w-0]
│   │   │       ├── TitleRow [flex items-start justify-between gap-3 mb-2]
│   │   │       │   ├── Title [text-xl font-heading font-bold text-foreground flex-1]
│   │   │       │   └── ActionMenu [shrink-0]                          ← edit/delete/pin
│   │   │       └── AuthorRow [flex items-center gap-2 text-sm]
│   │   │           ├── Avatar [w-6 h-6 rounded-full]
│   │   │           ├── AuthorName [font-medium text-foreground]
│   │   │           ├── BadgeChip? [same as PostCard]
│   │   │           ├── Separator [text-muted] ← "·"
│   │   │           └── Timestamp [text-muted]
│   │   ├── PostContent [prose prose-sm max-w-none py-4 border-t border-border/60]    ← rendered Markdown
│   │   └── PostFooter [flex items-center gap-3 pt-4 border-t border-border/60]
│   │       ├── TagList [flex items-center gap-1.5 flex-wrap flex-1]
│   │       ├── BookmarkButton [flex items-center gap-1.5 text-sm text-muted hover:text-primary cursor-pointer transition-colors px-2 py-1 rounded-lg hover:bg-primary/8]
│   │       └── ReportButton [flex items-center gap-1.5 text-sm text-muted hover:text-destructive cursor-pointer transition-colors px-2 py-1 rounded-lg hover:bg-destructive/8]
│   └── CommentSection [mt-6]
│       ├── CommentsHeader [flex items-center gap-2 mb-4]
│       │   ├── Title [text-base font-heading font-semibold]           ← "N Comments"
│       │   └── Divider [flex-1 h-px bg-border]
│       ├── CommentCard (×n) [flex gap-3 py-4 border-b border-border/60 last:border-0]
│       │   ├── Avatar [w-8 h-8 rounded-full flex-shrink-0]
│       │   └── CommentBody [flex-1 min-w-0]
│       │       ├── CommentHeader [flex items-center gap-2 mb-1.5]
│       │       │   ├── AuthorName [text-sm font-medium]
│       │       │   ├── AcceptedBadge? [inline-flex items-center gap-1 text-[11px] bg-success/10 text-success px-1.5 py-0.5 rounded font-medium]
│       │       │   └── Timestamp [text-xs text-muted ml-auto]
│       │       ├── Content [text-sm text-foreground leading-relaxed]
│       │       └── CommentActions [flex items-center gap-1 mt-2]
│       │           ├── VoteCompact [flex items-center gap-1 text-xs text-muted]
│       │           ├── AcceptButton? [text-xs text-muted hover:text-success cursor-pointer]   ← chỉ OP
│       │           ├── EditButton? [text-xs text-muted hover:text-foreground cursor-pointer]  ← chỉ author/mod
│       │           └── ReportButton [text-xs text-muted hover:text-destructive cursor-pointer]
│       └── AddCommentBox [mt-4 flex gap-3]
│           ├── Avatar [w-8 h-8 rounded-full flex-shrink-0]
│           └── CommentForm [flex-1]
│               ├── Textarea [w-full rounded-xl border border-border bg-muted/30 px-3 py-2 text-sm resize-none focus:ring-2 focus:ring-primary/30 focus:border-primary transition-all]
│               └── FormFooter [flex justify-end mt-2]
│                   └── SubmitButton [px-4 py-1.5 rounded-lg bg-primary text-primary-foreground text-sm font-medium hover:bg-primary-hover transition-colors cursor-pointer]
└── PostSidebar [w-64 shrink-0 hidden xl:block]
    └── PopularTagsCard [rounded-xl border border-border bg-card p-4]
        ├── CardTitle [text-sm font-heading font-semibold mb-3]
        └── TagList [space-y-1]
            └── TagRow (×n) [flex items-center justify-between py-1 cursor-pointer hover:text-primary transition-colors]
                ├── TagName [text-sm text-foreground]
                └── PostCount [text-xs text-muted tabular-nums]
```

---

### `/forum` — Post List

**Filters/Sort bar:**
```
[Latest] [Hot] [Unanswered] [Bookmarked]     Sort: [Newest ▼]
```

**Search:** Input → debounce 300ms → `GET /api/v1/forum/posts/search?q=xxx`

**Right sidebar:** Popular tags + Tag filter chips

**Pagination:** Infinite scroll với `useInfiniteQuery`.

### `/forum/[id]` — Post Detail

Layout:
```
┌─────────────────────────────────┐
│  ← Back   Title                 │
├──┬──────────────────────────────┤
│  │  [Avatar] Author  Badge  Time│
│V │  Post content (Markdown)     │
│O │                              │
│T │  [🔖 Bookmark] [⚑ Report]   │
│E │  Tags: [#tag1] [#tag2]       │
│  ├──────────────────────────────┤
│  │  COMMENTS (N)               │
│  │  [CommentCard]               │
│  │  [CommentCard — accepted ✓]  │
│  │                              │
│  │  [Add comment textarea]      │
└──┴──────────────────────────────┘
```

**Vote component (left rail):**
```
  ▲ (up)
  24
  ▼ (down)
```
- Optimistic update: increment immediately, rollback on error
- Current user's vote highlighted (primary color)

**CommentCard:**
- Author + time + content
- Vote up/down (compact)
- "Chấp nhận câu trả lời" button (chỉ hiện với OP) → green checkmark khi accepted
- Edit/Delete (chỉ author hoặc mod)
- Report button

### `/forum/create` — Create Post

**Form:**
- Title (required, max 200)
- Content (Markdown editor với preview toggle)
- Tags (multi-select autocomplete từ `GET /api/v1/forum/tags`)
- Type: Discussion / Q&A
- Submit → `POST /api/v1/forum/posts` → redirect `/forum/[newId]`

**Markdown editor:** Dùng `@uiw/react-md-editor` hoặc `@tiptap/react` (tiptap recommended cho rich text).

### `/forum/[id]/edit` — Edit Post

Same form nhưng pre-filled. Submit → `PUT /api/v1/forum/posts/{id}`.

---

## Components to Create

```
components/features/forum/
├── PostCard.tsx           ← (tái dụng từ FE-06)
├── PostList.tsx           ← list + filters + infinite scroll
├── PostDetail.tsx         ← full post view
├── VoteButtons.tsx        ← up/down vote với optimistic update
├── CommentSection.tsx     ← comment list + add comment
├── CommentCard.tsx        ← single comment
├── CreatePostForm.tsx     ← Markdown editor form
├── PostFilters.tsx        ← Latest/Hot/Unanswered tabs
├── TagPicker.tsx          ← multi-select tag input
├── BookmarkButton.tsx     ← toggle bookmark
├── ReportDialog.tsx       ← report reason modal
└── PostActionMenu.tsx     ← edit/delete/pin (author/mod)
hooks/api/forum/
├── usePosts.ts
├── usePost.ts
├── useCreatePost.ts
├── useUpdatePost.ts
├── useVotePost.ts
├── useBookmark.ts
├── useComments.ts
├── useAddComment.ts
├── useVoteComment.ts
└── useTags.ts
```

---

## Key Implementation Notes

- **Optimistic updates:** Vote count update immediately. Dùng TanStack Query `optimisticUpdate` + `onError` rollback.
- **Markdown render:** Dùng `react-markdown` + `rehype-highlight` cho syntax highlighting trong post detail.
- **Tag autocomplete:** Debounce 200ms khi search tags.
- **Infinite scroll:** `useInfiniteQuery` + `IntersectionObserver` để load thêm khi scroll đến bottom.

---

## Acceptance Criteria

- [ ] Post list load với pagination
- [ ] Filter Latest/Hot/Unanswered hoạt động
- [ ] Tag filter hoạt động
- [ ] Search tìm được bài viết
- [ ] Vote up/down với optimistic update
- [ ] Bookmark toggle và hiện trong tab Bookmarked
- [ ] Create post với Markdown editor
- [ ] Tag picker hoạt động
- [ ] Comment list load đúng
- [ ] Add/Edit/Delete comment
- [ ] Vote comment
- [ ] Accept answer (chỉ OP, hiện green badge)
- [ ] Report dialog gửi được
- [ ] Loading skeletons khi fetch
- [ ] Empty state khi không có kết quả
