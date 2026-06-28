# FE-14: Mod Zone — Reports Queue + Post Management

| Property | Value |
|---|---|
| **ID** | FE-14 |
| **Branch** | `feature/forum-moderation-scope` |
| **Commit** | `feat(fe/mod): reports queue i18n, links, 403 toast, pagination` |
| **Priority** | Medium |
| **Estimate** | 6h |
| **Status** | ✅ SCOPE WORK DONE (Phase 3 complete) |
| **Depends on** | FE-04 |

---

## API Endpoints (corrected — production routes)

| Action | Endpoint |
|---|---|
| Get reports (scoped) | `GET /api/v1/mod/reports?status=pending&pageNumber=1&pageSize=20` |
| Resolve report | `POST /api/v1/mod/reports/{id}/resolve` — body `{ action: "keep" \| "remove" }` |
| Get pending posts (scoped) | `GET /api/v1/mod/posts?pageNumber=1&pageSize=20` |
| Publish draft post | `POST /api/v1/posts/{id}/publish` |
| Pin post | `POST /api/v1/posts/{id}/pin` |
| Delete post (mod via resolve) | via `POST /api/v1/mod/reports/{id}/resolve` with `action: "remove"` |

> **Scope behaviour (server-side):**
> - **Admin** — sees all reports / can resolve all.
> - **Moderator** — only sees reports for categories in their `ModeratorIds`; resolving out-of-scope returns **HTTP 403**.

---

## Pages

### `/mod/reports` — Reports Queue

```
┌──────────────────────────────────────────────────────┐
│  Reports Queue                   Total: 12           │
│  [Pending] [Kept] [Removed]                          │
│  ─────────────────────────────────────────────────── │
│  [Bài viết] Spam                           2h ago    │
│  Nội dung: "Tuyển dụng XYZ..."                       │
│  [Xem bài] [Giữ lại] [Xóa]                          │
│  ─────────────────────────────────────────────────── │
│  [Bình luận] Quấy rối                      5h ago   │
│  Nội dung: "abc xyz..."                              │
│  [Xem bình luận] [Giữ lại] [Xóa]                    │
│  ─────────────────────────────────────────────────── │
│  < Trang 1 / 2 >                                     │
└──────────────────────────────────────────────────────┘
```

Actions:
- **Xem bài/comment:** link sang `/forum/{postId}` hoặc `/forum?commentId={id}`
- **Giữ lại:** resolve với `action: "keep"`
- **Xóa:** resolve với `action: "remove"` → soft-delete target
- **403 Forbidden toast:** hiện khi Moderator thao tác ngoài scope (màu amber)

### `/mod/posts` — Pending Posts (draft approval)

Table các draft post trong scope Moderator:
| Title | Author | Category | Date | Actions |
|-------|--------|----------|------|---------|
| ...   | ...    | ...      | ...  | [Publish] |

---

## Components (updated)

```
frontend/src/features/forum/
├── hooks/useModReportsPage.ts     ← pagination, reason label, item link, 403 detect
├── components/ModReportsPage.tsx  ← reason i18n, open link, 403 toast, pagination UI
├── api/forum.moderation.api.ts    ← correct /api/v1/mod/* routes (unchanged)
└── shared/i18n/locales/{en,vi}/mod.json  ← full reason map + pagination keys
```

---

## Acceptance Criteria

- [x] Reports queue hiện đúng pending reports (scoped theo category)
- [x] Reason hiện text i18n thay vì số nguyên
- [x] Link "Xem bài viết" / "Xem bình luận" navigate đúng
- [x] Lỗi 403 (out-of-scope) hiện toast màu amber thay vì generic error
- [x] Pagination hoạt động: trước/sau, ẩn khi chỉ 1 trang
- [ ] Filter reports theo type (post/comment) — optional polish
- [ ] Post management table với sorting — phase sau
- [ ] Pending report count hiện trong mod sidebar badge — phase sau
