# API tài liệu cho Frontend & tích hợp

Tất cả endpoint dùng envelope `ApiResponse` (xem từng file). Base URL mặc định: `/api/v1`.

| Tài liệu | Phạm vi |
|----------|---------|
| [auth-fe-api.md](./auth-fe-api.md) | Đăng nhập, đăng ký, token |
| [admin-authorization-fe-api.md](./admin-authorization-fe-api.md) | RBAC, override, endpoint toggles, maintenance, audit |
| [user-action-logs-api.md](./user-action-logs-api.md) | Observability / user action logs (admin) |
| [forum-fe-api.md](./forum-fe-api.md) | Posts, comments, tags, **search**, **moderation** |
| [notification-fe-api.md](./notification-fe-api.md) | Inbox, preferences, **broadcast** |
| [chat-fe-api.md](./chat-fe-api.md) + [chat-api-endpoints.md](./chat-api-endpoints.md) + [chat-signalr-fe-contract.md](./chat-signalr-fe-contract.md) | Chat REST + SignalR |
| [learning-fe-api.md](./learning-fe-api.md) | Learning / documents |
| [career-fe-api.md](./career-fe-api.md) | Jobs, companies, applications |
| [ai-fe-api.md](./ai-fe-api.md) | Module AI tổng quát (`/api/v1/ai/...`) |
| [assistant-tools-fe-api.md](./assistant-tools-fe-api.md) | **Mới** — công cụ gắn Forum + AI (`/api/v1/assistant/tools/...`) |
| [integrations-fe-api.md](./integrations-fe-api.md) | **Mới** — **UEBot** token exchange, cấu hình server |

## Tích hợp ngoài SPA

- **UEBot / desktop:** xem [integrations-fe-api.md](./integrations-fe-api.md) và kế hoạch [../plans/forum-uebot-integration/README.md](../plans/forum-uebot-integration/README.md).
- **Biến môi trường:** [../ENVIRONMENT_VARIABLES.md](../ENVIRONMENT_VARIABLES.md), `frontend/.env.example` (`VITE_UEBOT_*`).
