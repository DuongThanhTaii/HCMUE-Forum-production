# FE Roadmap — Modules B→F (Big Picture)

> Sau khi hoàn thành Plan A (Admin), lần lượt lập plan chi tiết cho từng module dưới đây.  
> Mỗi module = 1 session brainstorm + 1 spec chi tiết + implementation.

---

## B — Forum Completion

**Scope còn lại (PARTIAL → DONE):**

| Gap | Chi tiết |
|-----|----------|
| Create/Edit post | Form đăng bài: title, content (markdown/rich text), category, tags |
| API filter wired | `GET /api/v1/posts?categoryId=&tagId=&sort=latest|hot&page=` — hiện sidebar link không drive API |
| Categories list | `GET /api/v1/forum/categories` — sidebar hiện hardcode |
| Tags list | `GET /api/v1/forum/tags` |
| Delete post | Chỉ dành owner/Admin/Mod |
| Update post | Edit flow |
| Bookmark page | `/profile/bookmarks` — đã có bookmark API, chưa có trang |

**Existing docs:** `fe-07-forum.md`  
**Estimate:** ~6h

---

## C — Auth Completion

**Scope còn lại (PARTIAL → DONE):**

| Gap | Chi tiết |
|-----|----------|
| Logout API call | `POST /api/v1/auth/logout` — hiện Redux logout không gọi API |
| Forgot password | Page `/forgot-password` → `POST /api/v1/auth/forgot-password` |
| Reset password | Page `/reset-password?token=` → `POST /api/v1/auth/reset-password` |
| Profile settings | Page `/profile/settings` → `GET + PUT /api/v1/users/me/profile` |

**Existing docs:** `fe-02-auth-pages.md`, `fe-13-profile-settings.md`  
**Estimate:** ~3h

---

## D — Notification

**Scope (MISSING → DONE):**

| Feature | Chi tiết |
|---------|----------|
| Notification list | `GET /api/v1/notifications?page=&isRead=` |
| Mark as read | `PUT /api/v1/notifications/{id}/read` |
| Mark all read | `PUT /api/v1/notifications/read-all` |
| Delete notification | `DELETE /api/v1/notifications/{id}` |
| Real-time (SignalR) | Hub `/hubs/notifications` — push new notification badge |
| Notification bell | Top bar badge count + dropdown |

**Existing docs:** `fe-12-notifications.md`, `phase-10-notification.md`  
**API doc:** `docs/api/notification-fe-api.md`  
**Estimate:** ~5h (REST + SignalR)

---

## E — Chat (Real-time)

**Scope (MISSING → DONE):**

| Feature | Chi tiết |
|---------|----------|
| Conversation list | `GET /api/v1/chat/conversations` |
| DM create | `POST /api/v1/chat/conversations/direct` |
| Group create | `POST /api/v1/chat/conversations/group` |
| Message history | `GET /api/v1/chat/conversations/{id}/messages?page=` |
| Send message | `POST /api/v1/chat/conversations/{id}/messages` |
| Mark as read | `POST /api/v1/chat/messages/{id}/read` |
| Reactions | `POST /api/v1/chat/messages/{id}/reactions` |
| Channels | `GET /api/v1/chat/channels` + join/leave |
| Real-time (SignalR) | Hub `/hubs/chat` — live messages, typing indicator |
| AI Chat `/chat/ai` | `POST /api/v1/ai/chat` — xem docs/api/ai-fe-api.md |

**Existing docs:** `fe-10-chat.md`, `fe-11-unibot.md`, `phase-10-chat.md`  
**API docs:** `docs/api/chat-fe-api.md`, `docs/api/ai-fe-api.md`  
**Estimate:** ~14h (phức tạp nhất — SignalR + UX)

---

## F — Career Completion & Mod

**Scope còn lại:**

| Feature | Chi tiết |
|---------|----------|
| Job detail page | `GET /api/v1/jobs/{id}` — hiện chỉ có list |
| Apply for job | `POST /api/v1/jobs/{id}/apply` (nếu BE có) |
| Companies list | `GET /api/v1/companies` |
| Company detail | `GET /api/v1/companies/{id}` |
| Moderator reports | `/mod/reports` — `GET /api/v1/forum/reports` |
| Moderator approvals | `/mod/posts` — approve/reject documents (Learning) |

**Existing docs:** `fe-09-career.md`, `fe-14-mod-reports.md`, `fe-15-mod-approvals.md`  
**Estimate:** ~6h

---

## Thứ tự thực hiện khuyến nghị

```
A (Admin) → C (Auth completion) → D (Notification) → B (Forum) → F (Career+Mod) → E (Chat)
```

Lý do: A trước để có user/role management đầy đủ; C nhỏ và giải quyết UX gap; D cần SignalR nhẹ (không state phức tạp như Chat); B Forum cần category/tag API; F hoàn thiện career; E Chat cuối cùng vì phức tạp nhất.
