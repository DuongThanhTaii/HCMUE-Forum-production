# FE-12: Notification Center

| Property | Value |
|---|---|
| **ID** | FE-12 |
| **Branch** | `feature/FE-12-notifications` |
| **Commit** | `feat(fe/notifications): implement notification bell, dropdown and center` |
| **Priority** | Medium |
| **Estimate** | 5h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03 |

---

## API Endpoints

| Action | Endpoint |
|---|---|
| Get notifications | GET `/api/v1/notifications?page=1&pageSize=20` |
| Unread count | GET `/api/v1/notifications/unread-count` |
| Mark as read | POST `/api/v1/notifications/{id}/read` |
| Mark all read | POST `/api/v1/notifications/read-all` |
| Delete | DELETE `/api/v1/notifications/{id}` |
| Get preferences | GET `/api/v1/notifications/preferences` |
| Update preferences | PUT `/api/v1/notifications/preferences` |

**SignalR Hub:** `wss://api/hubs/notifications`  
Event: `ReceiveNotification` → push new notification, increment unread count

---

## Bell Dropdown (in Sidebar footer)

```
[Bell🔔] [23]  ← badge đỏ

Khi click → Popover:
┌─────────────────────────────────────────┐
│  Thông báo                [Mark all ✓]  │
│  ─────────────────────────────────────  │
│  [🟦] Forum: "User A đã trả lời bài..."│
│       2 phút trước         [×]          │
│  [📚] Learning: "Tài liệu được duyệt"  │
│       1 giờ trước          [×]          │
│  ...                                    │
│  ─────────────────────────────────────  │
│  [Xem tất cả thông báo →]              │
└─────────────────────────────────────────┘
```

- Max 10 items trong dropdown, link "Xem tất cả" → `/notifications`
- Click notification → navigate đến resource + mark as read
- Unread items: `bg-primary/5` background

---

## `/notifications` — Full Notification Page

Grouped by type:
- **Forum** (màu primary)
- **Learning** (màu success)
- **Career** (màu orange)
- **System** (màu muted)

Actions per item: Mark read / Delete  
Bulk action: "Mark all read"

---

## `/settings/notifications` — Preferences

Toggle switches per notification type:
- New comment on my post
- Vote on my post/comment
- Document approved/rejected
- Job application status
- New message (chat)
- System announcements

---

## Components

```
components/features/notification/
├── NotificationBell.tsx       ← bell icon + badge count
├── NotificationDropdown.tsx   ← popover with recent notifications
├── NotificationItem.tsx       ← single notification row
├── NotificationPage.tsx       ← full page grouped view
└── NotificationPreferences.tsx← toggle switches
hooks/realtime/
└── useNotificationHub.ts      ← SignalR for push notifications
```

---

## Acceptance Criteria

- [ ] Bell badge hiện đúng unread count
- [ ] Real-time update qua SignalR khi có notification mới
- [ ] Dropdown hiện 10 latest
- [ ] Mark as read khi click
- [ ] Mark all read button
- [ ] Delete single notification
- [ ] Full notification page với grouping
- [ ] Preferences page với toggle switches
