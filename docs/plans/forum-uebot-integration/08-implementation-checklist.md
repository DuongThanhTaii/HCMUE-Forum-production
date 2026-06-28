# 08 — Checklist triển khai (theo sprint)

Dùng file này để tracking PR/issue. Đánh `[x]` khi xong.

---

## Sprint F0 — Foundation

- [x] API docs: `docs/api/README.md`, `integrations-fe-api.md`, `assistant-tools-fe-api.md`; cập nhật forum/admin/notification/ai/career
- [ ] Chuẩn hóa biến môi trường FE + API cho Azure/local giống nhau giữa web và UEBot dev
- [ ] Chạy sync-api UEBot tại `BaseUrl` đúng với `UEBotIntegrationOptions`
- [ ] `POST /api/v1/integrations/uebot/exchange-token` trả 200 (happy path)
- [ ] Document nội bộ: sequence login → bearer Forum → exchange (link tới `02-phase-0-foundation.md`)

---

## Sprint F1 — Moderation core

- [ ] UEBot: màn **Reports** — `GET /api/v1/mod/reports?status=pending`
- [ ] UEBot: **Resolve** — `POST /api/v1/mod/reports/{id}/resolve` + xử lý `409`/`403`
- [ ] UEBot: màn **Drafts** — `GET /api/v1/mod/posts`
- [ ] UEBot: **Publish** — `POST /api/v1/posts/{id}/publish` + kiểm tra role author/mod/admin
- [ ] QA: thao tác trên UEBot phản ánh ngay trên web Forum

---

## Sprint F2 — Search & links

- [ ] UEBot: ô search → `GET /api/v1/posts` (mở rộng query `search` nếu backlog được approve)
- [ ] Build URL post detail từ response (slug/id)
- [ ] Related: workaround category/tag **hoặc** implement `GET /api/v1/posts/{id}/related`

---

## Sprint F3 — Notifications & reminders

- [ ] UEBot: polling notification API + badge
- [ ] Deep link từ notification item
- [ ] (Optional) Reminder local + URL Forum

---

## Sprint F4 — AI & polish

- [ ] Cấu hình `AIProviders` trên API; xác nhận endpoint AI không trả “unavailable” trong dev
- [ ] UEBot: nút tóm tắt (nếu product cần) gọi Forum AI

---

## Cross-cutting (luôn áp dụng)

- [ ] Không commit secret; dùng User Secrets / CI vars
- [ ] Rate limit & logging không lộ token
- [ ] Cập nhật `docs/api/*.md` nếu thêm endpoint mới

---

## Tham chiếu nhanh endpoint

| Mục đích | Method | Path |
|----------|--------|------|
| Exchange | POST | `/api/v1/integrations/uebot/exchange-token` |
| Reports | GET | `/api/v1/mod/reports` |
| Resolve | POST | `/api/v1/mod/reports/{id}/resolve` |
| Drafts | GET | `/api/v1/mod/posts` |
| Publish | POST | `/api/v1/posts/{id}/publish` |
| List/search posts | GET | `/api/v1/posts` |
