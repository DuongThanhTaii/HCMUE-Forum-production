# Integrations API — UEBot (`ApiResponse` Envelope)

Tài liệu cho **trao đổi token** giữa Forum API và **UEBot sync-api** (server-to-server từ phía Forum + client gọi Forum).

## Base URL

- `/api/v1/integrations/uebot`

## Envelope

Giống [forum-fe-api.md](./forum-fe-api.md) (success / data / message / error).

## Auth

- **Bắt buộc** `Authorization: Bearer <Forum JWT>` — user phải đã đăng nhập Forum/Azure/local.

## Rate limiting

- Controller gắn `[EnableRateLimiting("integrations")]`.

---

## `POST /api/v1/integrations/uebot/exchange-token`

Đổi session Forum sang token sync UEBot: Forum đọc claims user hiện tại, gọi upstream **sync-api**, trả token cho client.

### Request

- Body: không có (hoặc bỏ trống).
- Server yêu cầu claim **email** (ClaimTypes.Email, `email`, `preferred_username`, `upn`).

### Response `200`

`data` kiểu `UEBotExchangeTokenResponse` (JSON camelCase mặc định ASP.NET):

| Field | Kiểu | Mô tả |
|-------|------|--------|
| `syncAccessToken` | string | Token dùng với sync-api / UEBot client |
| `syncExpiresAt` | string \| null | ISO datetime nếu upstream trả |
| `syncUser` | object \| null | `{ id, email, name? }` |
| `syncApiBaseUrl` | string | Base URL clients nên dùng khi gọi sync-api (từ config `SyncApiBaseUrl`) |

### Lỗi

| HTTP | Nguyên nhân |
|------|-------------|
| `400` | Không suy ra được email từ claims |
| `502` | Không kết nối được sync-api, upstream trả lỗi, body không parse được, hoặc `token` rỗng |

### Cấu hình server (Forum)

Section `Integrations:UEBot` — class `UEBotIntegrationOptions`:

| Key | Ý nghĩa |
|-----|---------|
| `BaseUrl` | Origin sync-api cho HTTP client (mặc định `http://localhost:4010`) |
| `ExchangePath` | Path trên sync-api (mặc định `/integrations/forum/exchange`) |
| `SharedSecret` | Gửi header `X-Integration-Secret` nếu có giá trị |
| `SyncApiBaseUrl` | Trả về cho client trong response (`syncApiBaseUrl`) |

### Upstream payload (Forum → sync-api)

Forum POST JSON:

```json
{
  "externalUserId": "<guid-string>",
  "email": "user@example.edu",
  "name": "Optional Name",
  "source": "forum"
}
```

Upstream phải trả JSON parse được thành `{ "token": "...", "expiresAt": "...", "user": { ... } }` (tên field cụ thể map trong `UEBotInternalExchangeResponse`).

---

## FE đã tích hợp

- RTK: `useExchangeUEBotTokenMutation` trong `frontend/src/features/assistant/api/assistant.api.ts`.
- `AssistantPage`: iframe UEBot + postMessage sau exchange (xem `VITE_UEBOT_WEBAPP_URL`, `VITE_UEBOT_SYNC_API_URL`).
