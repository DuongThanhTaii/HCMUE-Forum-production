# Pha 0 — Nền tảng (foundation)

Mục tiêu: **một luồng đăng nhập + gọi API Forum + exchange UEBot** ổn định trên dev và demo.

## 0.1 Auth và JWT

**Forum:**

- Người dùng đăng nhập (Azure AD và/hoặc local); API nhận **Bearer** trên các endpoint `[Authorize]`.
- Dynamic JWT / Azure setup đã nằm trong Identity module — UEBot phải dùng **cùng issuer/audience** như web FE (`frontend/.env` ↔ `appsettings`).

**UEBot:**

- Lấy access token Forum (flow giống SPA: redirect hoặc popup tùy stack desktop).
- Mọi request REST: header `Authorization: Bearer <forum_access_token>`.

## 0.2 Token exchange (đã implement)

**Endpoint Forum:** `POST /api/v1/integrations/uebot/exchange-token`

- **Auth:** bắt buộc đăng nhập Forum (controller `[Authorize]`).
- **Body:** không (server đọc claims user hiện tại).
- **Response thành công:** `ApiResponse<UEBotExchangeTokenResponse>` gồm:
  - `syncAccessToken` (tên property JSON theo contract serializer — kiểm tra `Program.cs` / naming camelCase)
  - `syncExpiresAt`
  - `syncUser` (optional)
  - `syncApiBaseUrl`

**Upstream:** Forum gọi HTTP tới:

- `Integrations:UEBot:BaseUrl` (mặc định `http://localhost:4010`)
- Path `Integrations:UEBot:ExchangePath` (mặc định `/integrations/forum/exchange`)
- Header `X-Integration-Secret` nếu `SharedSecret` được set.

**Lỗi thường gặp:**

| HTTP | Ý nghĩa |
|------|---------|
| `502` | Không tới được sync-api, parse lỗi, hoặc body rỗng — **không** liên quan AI module |
| `400` | Thiếu email trên claims |

## 0.3 Cấu hình (Forum)

Trong `appsettings.Development.json` (hoặc secret manager):

```json
"Integrations": {
  "UEBot": {
    "BaseUrl": "http://localhost:4010",
    "ExchangePath": "/integrations/forum/exchange",
    "SharedSecret": "<optional>",
    "SyncApiBaseUrl": "http://localhost:4010"
  }
}
```

Đảm bảo **sync-api thật sự listen** đúng port và route exchange.

## 0.4 CORS và HTTPS (dev)

- FE và API: cho phép origin dev (`localhost`, `127.0.0.1`, port Vite) — đã chỉnh trong `Program.cs` + appsettings.
- Tránh `UseHttpsRedirection` phá preflight trong Development (đã xử lý theo pipeline hiện tại).

## 0.5 Deep link chuẩn

Định nghĩa URL mở bài trên web (thay `{base}` bằng domain thật):

- `{base}/forum/...` hoặc route post detail hiện có trong `frontend` router.

UEBot chỉ cần **mở URL** trong browser; không nhân bản nội dung dài trong client trừ preview.

## 0.6 Tiêu chí hoàn thành Pha 0

- [ ] User đăng nhập → `GET /api/v1/users/me` (hoặc endpoint profile tương đương) **200**.
- [ ] `POST /api/v1/integrations/uebot/exchange-token` **200** khi sync-api chạy.
- [ ] Document sequence diagram (có thể dán vào issue/PR) — optional nhưng khuyến nghị.
