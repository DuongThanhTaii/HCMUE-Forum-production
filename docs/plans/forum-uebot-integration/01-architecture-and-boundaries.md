# 01 — Kiến trúc và ranh giới

## 1.1 Vai trò từng phía

| Thành phần | Trách nhiệm chính |
|------------|-------------------|
| **Forum (UniHub API + DB)** | Dữ liệu gốc: bài viết, báo cáo, trạng thái duyệt, quyền, audit phía server, REST + SignalR cho web |
| **UEBot client** | UX nhanh, thông báo OS, deep link, gọi tool (lịch, trình duyệt…); **không** là nguồn sự thật cho quyết định mod |
| **UEBot sync-api** | Nhận exchange token từ Forum (qua proxy Forum), cấp token để client gọi lại Forum |

## 1.2 Nguyên tắc thiết kế

1. **Mọi hành động có hiệu lực pháp lý / nội dung** (duyệt bài, gỡ nội dung, đăng chính thức) phải đi qua **Forum API** với **JWT người dùng** (sau khi đăng nhập Azure/local và có bearer hợp lệ).
2. **Token exchange** chỉ để đồng bộ danh tính giữa Forum và hệ sinh thái UEBot; không thay thế authorization trên Forum.
3. **LLM / Copilot** trong UEBot có thể đề xuất hành động, nhưng **server Forum** mới quyết định cuối (permission + command handler).
4. **Deep link:** URL Forum (`https://.../forum/posts/{slug-or-id}`) là định danh ổn định để mở từ UEBot sang trình duyệt.

## 1.3 Luồng tổng quát (ASCII)

```
[User] → [UEBot UI]
           │
           ├─► Login Azure / local (giống hoặc parallel với web)
           │
           ├─► Bearer Forum JWT ──► GET/POST /api/v1/... (moderation, posts, search)
           │
           └─► POST /api/v1/integrations/uebot/exchange-token
                    │
                    └─► Forum proxy ──► sync-api POST .../integrations/forum/exchange
                              │
                              └─► Response: SyncAccessToken + SyncApiBaseUrl
                                    (dùng cho client ↔ sync-api / tính năng nội bộ UEBot, không thay JWT Forum)
```

## 1.4 Phạm vi “module” theo pha

| Module | Forum | UEBot |
|--------|-------|-------|
| **Integration / exchange** | Controller + options + rate limit `integrations` | Gọi exchange sau login; lưu sync token an toàn |
| **Moderation inbox** | Đã có `/api/v1/mod/reports`, `/api/v1/mod/posts` | UI queue + resolve + mở web preview |
| **Publish pipeline** | `POST /api/v1/posts/{id}/publish` | Nút “duyệt đăng” gọi API (mod/admin/author theo rule hiện tại) |
| **Search & related** | `GET /api/v1/posts` + query (đã có); related có thể mở rộng | Ô search + danh sách + mở link |
| **Notifications** | Notification API + SignalR (web) | Optional: mirror unread qua polling |
| **AI summary** | Module AI + `AIProviders` trong config | Gọi endpoint Forum AI nếu có; lỗi “unavailable” là cấu hình provider, không phải exchange |

## 1.5 Phi chức năng

- **Idempotency:** resolve report — đã có xử lý conflict (`409` AlreadyResolved); client UEBot nên hiển thị message thân thiện.
- **Pagination:** mọi list (`reports`, `posts` pending, `posts` search) dùng `pageNumber` / `pageSize` thống nhất với FE hiện tại.
- **Versioning:** giữ prefix `/api/v1`; mọi breaking change sau này → `/api/v2` hoặc header version.
