# Execute — Forum ↔ UEBot

Thư mục **execute** trỏ tới kế hoạch triển khai chi tiết (copy sprint checklist vào issue khi bắt đầu).

## API reference (đã đồng bộ backend)

- **[docs/api/README.md](../../api/README.md)** — mục lục
- **[docs/api/integrations-fe-api.md](../../api/integrations-fe-api.md)** — `exchange-token`
- **[docs/api/assistant-tools-fe-api.md](../../api/assistant-tools-fe-api.md)** — `/api/v1/assistant/tools/*`

## Kế hoạch đầy đủ

→ **[docs/plans/forum-uebot-integration/README.md](../../plans/forum-uebot-integration/README.md)**

## Bắt tay ngay

1. Đọc [02-phase-0-foundation.md](../../plans/forum-uebot-integration/02-phase-0-foundation.md) — exchange token + config.
2. Làm checklist [08-implementation-checklist.md](../../plans/forum-uebot-integration/08-implementation-checklist.md).
3. Moderation: [03-phase-1-moderation-and-approval.md](../../plans/forum-uebot-integration/03-phase-1-moderation-and-approval.md).

## Code anchor

- `src/UniHub.API/Controllers/UEBotIntegrationController.cs`
- `src/Modules/Forum/UniHub.Forum.Presentation/Controllers/ModerationController.cs`
