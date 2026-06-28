# Forum ↔ UEBot — Kế hoạch tích hợp chi tiết

Tài liệu này mô tả **phân tách trách nhiệm**, **các pha triển khai**, **contract API** và **checklist** để team implement đồng bộ giữa **UniHub Forum (API + web)** và **UEBot (desktop / sync-api)**.

## Mục lục

| File | Nội dung |
|------|----------|
| [01-architecture-and-boundaries.md](./01-architecture-and-boundaries.md) | Nguyên tắc, ranh giới, luồng dữ liệu |
| [02-phase-0-foundation.md](./02-phase-0-foundation.md) | Auth, token exchange, cấu hình, deep link |
| [03-phase-1-moderation-and-approval.md](./03-phase-1-moderation-and-approval.md) | Hàng chờ báo cáo, bài nháp, duyệt đăng |
| [04-phase-2-search-related-ai.md](./04-phase-2-search-related-ai.md) | Tìm kiếm, link bài liên quan, AI (optional) |
| [05-phase-3-notifications-reminders.md](./05-phase-3-notifications-reminders.md) | Thông báo, nhắc việc |
| [06-phase-4-drafts-composer-optional.md](./06-phase-4-drafts-composer-optional.md) | Soạn nháp từ UEBot (backlog) |
| [07-security-and-operations.md](./07-security-and-operations.md) | Bảo mật, audit, vận hành |
| [08-implementation-checklist.md](./08-implementation-checklist.md) | Task list có checkbox theo sprint |

## Điểm neo trong codebase (Forum)

- **Token exchange (Forum → sync-api UEBot):** `POST /api/v1/integrations/uebot/exchange-token` — `UEBotIntegrationController`
- **Cấu hình:** `Integrations:UEBot` — `UEBotIntegrationOptions` (`BaseUrl`, `ExchangePath`, `SharedSecret`, `SyncApiBaseUrl`)
- **Moderation:** `GET/POST /api/v1/mod/...` — `ModerationController` (permission `forum.reports.review`)
- **Đăng bài / publish:** `POST /api/v1/posts/{id}/publish` — `PostsController`
- **Envelope API:** xem [docs/api/README.md](../../api/README.md)
- **UEBot exchange:** [docs/api/integrations-fe-api.md](../../api/integrations-fe-api.md)
- **Assistant tools (related posts, summarize, …):** [docs/api/assistant-tools-fe-api.md](../../api/assistant-tools-fe-api.md)
- **Forum + moderation + search:** [docs/api/forum-fe-api.md](../../api/forum-fe-api.md)

## Tài liệu liên quan

- [docs/domain/INTEGRATION_CONTRACTS.md](../../domain/INTEGRATION_CONTRACTS.md)
- [.uebot/tasks-integration-master-plan.md](../../../.uebot/tasks-integration-master-plan.md) (nếu có trong repo)
- [docs/plans/2026-05-03-forum-moderation-scope-implementation.md](../2026-05-03-forum-moderation-scope-implementation.md)

---

**Trạng thái:** kế hoạch triển khai — cập nhật khi từng pha hoàn thành.
