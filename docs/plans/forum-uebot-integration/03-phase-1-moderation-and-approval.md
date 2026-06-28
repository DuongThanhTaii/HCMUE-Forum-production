# Pha 1 — Moderation & duyệt đăng

Mục tiêu demo: **moderator xử lý báo cáo và/hoặc duyệt bài nháp** trực tiếp từ UEBot, trạng thái đồng bộ với web Forum.

## 1.1 Permission

Controller moderation yêu cầu permission **`forum.reports.review`** (class-level `ModerationController`).

UEBot chỉ hiển thị menu mod sau khi:

- JWT chứa permission tương ứng **hoặc**
- Gọi thử API và xử lý `403` (ẩn tính năng).

## 1.2 API đã có — Hàng chờ báo cáo

**`GET /api/v1/mod/reports`**

| Query | Ý nghĩa |
|-------|---------|
| `status` | `pending` (mặc định), `resolved_keep`, `resolved_remove`, hoặc để trống |
| `pageNumber`, `pageSize` | Phân trang |

**Response:** envelope `success`; `data` gồm `reports[]`, `totalCount`, `pageNumber`, … Mỗi report có preview `titlePreview`, `contentPreview`, `reportedItemType`, `reportedItemId`, v.v.

## 1.3 API đã có — Xử lý báo cáo

**`POST /api/v1/mod/reports/{id}/resolve`**

- Body: `ResolveModerationReportRequest` — field **`action`** (chuỗi, server trim + lowercase) theo command handler.
- Tham khảo DTO: `ResolveModerationReportRequest` trong presentation layer.

**Lỗi có thể:** `404` NotFound, `409` Conflict (đã resolve), `403` Forbidden (ngoài scope category).

**UEBot:** sau resolve, refresh list hoặc remove item khỏi queue cục bộ.

## 1.4 API đã có — Bài chờ duyệt (draft)

**`GET /api/v1/mod/posts`**

- Trả về **posts có status Draft** trong phạm vi moderator (filter category — logic scope trong controller).
- Dùng cho “inbox duyệt đăng” trước khi public.

**Payload:** tương tự list posts (`posts[]` + pagination meta).

## 1.5 Publish — đưa bài ra công khai

**`POST /api/v1/posts/{id}/publish`**

- `[Authorize]` — **không** gắn `RequirePermission` riêng trên action publish trong snippet hiện tại.
- Server xác định actor: `Admin` / `Moderator` / `Author` qua `PublishPostCommand`.

**UEBot flow đề xuất:**

1. Mod chọn bài từ `GET /api/v1/mod/posts`.
2. (Optional) Mở `GET /api/v1/posts/{id}` để xem đầy đủ.
3. Gọi `POST /api/v1/posts/{id}/publish`.
4. Thông báo thành công + deep link tới bài đã public.

## 1.6 UI/UX UEBot (thử nghiệm)

- Tab **Reports** và tab **Pending drafts** (hai inbox).
- Preview ngắn + nút “Mở trên web”.
- Tránh auto-resolve hàng loạt không xác nhận.

## 1.7 Việc có thể làm thêm (không chặn MVP)

| Việc | Phía Forum | Phía UEBot |
|------|------------|------------|
| Real-time queue | SignalR topic `mod` (spec sau) | Subscribe hoặc polling 30s |
| Audit log hiển thị | `user-action-logs` nếu có API | Chỉ đọc cho admin |

## 1.8 Tiêu chí hoàn thành Pha 1

- [ ] User có `forum.reports.review` xem được `GET /api/v1/mod/reports`.
- [ ] Resolve một report thành công; web Forum phản ánh đúng trạng thái.
- [ ] List draft + publish một bài từ UEBot; bài hiển thị public trên web.
