# FE-06: Home Feed

| Property | Value |
|---|---|
| **ID** | FE-06 |
| **Branch** | `feature/FE-06-home-feed` |
| **Commit** | `feat(fe/home): implement home feed with pinned posts and stats` |
| **Priority** | High |
| **Estimate** | 5h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03 |

---

## Objective

Trang chủ hiện latest posts từ Forum (pinned trước), quick stats, popular tags sidebar, job deadline widget.

---

## Layout

```
┌──────────────────────────────────┬──────────────────┐
│  MAIN FEED                       │  RIGHT SIDEBAR   │
│                                  │                  │
│  📌 Pinned posts (top 3)         │  Popular Tags    │
│  ─────────────────────           │  #học-bổng [42]  │
│  Latest posts (paginated)        │  #thực-tập [38]  │
│  [PostCard][PostCard]...         │  ...             │
│                                  │                  │
│                                  │  Deadline Jobs   │
│                                  │  [Job expiring]  │
│                                  │  [Job expiring]  │
└──────────────────────────────────┴──────────────────┘
```

---

## API Calls

| Data | Endpoint |
|---|---|
| Pinned posts | `GET /api/v1/forum/posts?isPinned=true&pageSize=3` |
| Latest posts | `GET /api/v1/forum/posts?sort=newest&pageSize=20` |
| Popular tags | `GET /api/v1/forum/tags/popular?limit=10` |
| Expiring jobs | `GET /api/v1/career/jobs?sortBy=deadline&pageSize=5` |

---

## Components

### `PostCard.tsx` (shared across Forum pages)

```tsx
interface PostCardProps {
  post: {
    id: string;
    title: string;
    excerpt: string;         // first 150 chars of content
    author: { name: string; avatar?: string; badge?: OfficialBadge };
    tags: Tag[];
    voteCount: number;
    commentCount: number;
    isPinned: boolean;
    createdAt: string;
  };
}
```

Card layout:
```
┌─────────────────────────────────────────┐
│ [Avatar] AuthorName  [Badge?]  2h ago   │
│                                         │
│ Title của bài viết (font-heading 16px)  │
│ Excerpt text (2 lines, truncate)...     │
│                                         │
│ [#tag1] [#tag2]      ▲ 24  💬 8        │
└─────────────────────────────────────────┘
```

- Pinned posts: thêm `border-l-4 border-primary` + icon ghim
- Hover: `shadow-card-hover cursor-pointer transition-shadow`
- Click → navigate `/forum/[id]`

### `PopularTagsSidebar.tsx`

Tag list với post count. Click → `/forum?tag=tagName`.

### `ExpiringJobsWidget.tsx`

Top 5 job sắp hết hạn. Hiện badge "Hết hạn trong X ngày" màu đỏ nếu < 3 ngày.

---

## Acceptance Criteria

- [ ] Pinned posts hiện trên cùng với border primary
- [ ] Posts load đúng thứ tự newest
- [ ] Infinite scroll hoặc Load more button (TanStack Query `useInfiniteQuery`)
- [ ] Popular tags sidebar clickable → filter forum
- [ ] Empty state khi không có posts
- [ ] Loading skeleton (Shadcn `Skeleton`) khi đang fetch
