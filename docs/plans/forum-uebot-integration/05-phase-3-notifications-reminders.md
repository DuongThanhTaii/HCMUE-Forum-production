# Pha 3 — Thông báo & nhắc việc

Mục tiêu: người dùng **không bỏ lỡ** phản hồi quan trọng; có thể **gắn nhắc** với bài / deadline.

## 3.1 Thông báo in-app (Forum)

Tham khảo API hiện có: [docs/api/notification-fe-api.md](../../api/notification-fe-api.md).

**UEBot (MVP):**

- **Polling** `unread` count / danh sách gần đây theo chu kỳ (vd 60s) khi app foreground.
- Hiển thị badge; tap → deep link tới entity (post, comment…) nếu API trả `link` hoặc đủ field để dựng URL.

**Nâng cao:** đồng bộ push OS — cần bridge riêng (Windows/macOS), không bắt buộc cho demo đầu.

## 3.2 SignalR (web)

Web Forum có thể đã dùng SignalR cho realtime — UEBot **không bắt buộc** dùng SignalR ngay; ưu tiên REST đơn giản.

Nếu sau này cần realtime UEBot:

- Xác định hub endpoint + auth (Bearer / query `access_token` theo pattern đã có cho hubs).

## 3.3 Nhắc việc (reminder)

**Option A — không đụng backend:**

- UEBot tạo **local calendar event** / reminder với title + deep link URL Forum.

**Option B — có metadata server:**

- Forum API lưu `Reminder` gắn `userId`, `postId`, `fireAt` — cần spec & migration.

Khuyến nghị: **Option A** cho MVP demo; Option B khi có yêu cầu đa thiết bị đồng bộ.

## 3.4 Tiêu chí hoàn thành Pha 3

- [ ] (MVP) User xem được danh sách thông báo gần đây từ API Forum trên UEBot.
- [ ] (MVP) Tap thông báo mở đúng bài trên browser.
- [ ] (Optional) Reminder local có deep link.
