# User Action Logs API

## Purpose
Endpoint để truy vấn log hành động user phục vụ điều tra/audit (đang dùng cho admin UI).

## Endpoint
`GET /api/v1/admin/observability/user-actions`

## Authorization
- Auth required (JWT Bearer).

## Query Parameters
- `actorUserId` (optional)
- `correlationId` (optional)
- `traceId` (optional)
- `method` (optional, ví dụ `GET`, `POST`)
- `pathContains` (optional)
- `minStatusCode` (optional)
- `maxStatusCode` (optional)
- `fromUtc` (optional, ISO-8601)
- `toUtc` (optional, ISO-8601)
- `viewType` (optional: `Developer` | `Administrator`, default `Developer`)
- `page` (default: 1)
- `pageSize` (default: theo config, có max clamp)

## Sample Request
```http
GET /api/v1/admin/observability/user-actions?actorUserId=user-123&viewType=Developer&fromUtc=2026-03-20T00:00:00Z&toUtc=2026-03-24T23:59:59Z&page=1&pageSize=50
```

## Sample Response
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "67e0a6a5b58d674d0c8f8fbe",
        "actorUserId": "user-123",
        "method": "PUT",
        "path": "/api/v1/admin/authorization/toggles/Api.Identity.Users.AssignRole",
        "queryString": "",
        "endpoint": "UniHub.Identity.Presentation.Controllers.AuthorizationAdminController.SetEndpointToggle (UniHub.Identity.Presentation)",
        "statusCode": 200,
        "durationMs": 42,
        "traceId": "00-...",
        "correlationId": "6f9a2d4d8e5e4f68a2d6d9fcb9c9b4f1",
        "remoteIp": "127.0.0.1",
        "userAgent": "Mozilla/5.0",
        "scheme": "https",
        "host": "localhost:5001",
        "startedAtUtc": "2026-03-24T08:12:11.123Z",
        "completedAtUtc": "2026-03-24T08:12:11.165Z",
        "result": "Success",
        "exceptionType": null,
        "exceptionMessage": null,
        "terminalLine": "[2026-03-24 08:12:11.165] [200] PUT /api/v1/admin/authorization/toggles/... endpoint=\"...\" actor=user-123 duration=42ms trace=00-... corr=..."
      }
    ],
    "total": 124,
    "page": 1,
    "pageSize": 50,
    "viewType": "Developer",
    "availableViewTypes": ["Developer", "Administrator"],
    "persistToMongo": true,
    "mongoCollectionName": "user_action_logs"
  },
  "message": null,
  "error": null
}
```

## Notes
- Log retention dùng TTL index theo `RetentionDays`.
- Truy vấn mặc định sort theo `CompletedAtUtc` giảm dần (mới nhất trước).
- Nếu persistence tắt (`PersistToMongo=false`) endpoint vẫn trả hợp lệ nhưng danh sách rỗng.
- `Developer` view trả full technical detail (query/ip/user-agent/exception message).
- `Administrator` view ẩn bớt dữ liệu kỹ thuật nhạy cảm để phù hợp dashboard vận hành.

## Schemas

### `UserActionLogSearchResponse`
- `items` (UserActionLogItemResponse[])
- `total` (long)
- `page` (int)
- `pageSize` (int)
- `viewType` (string)
- `availableViewTypes` (string[])
- `persistToMongo` (boolean)
- `mongoCollectionName` (string)

### `UserActionLogItemResponse`
- `id` (string)
- `actorUserId` (string)
- `method` (string)
- `path` (string)
- `queryString` (string)
- `endpoint` (string)
- `statusCode` (int)
- `durationMs` (long)
- `traceId` (string)
- `correlationId` (string)
- `remoteIp` (string)
- `userAgent` (string)
- `scheme` (string)
- `host` (string)
- `startedAtUtc` (datetime)
- `completedAtUtc` (datetime)
- `result` (string)
- `exceptionType` (string | null)
- `exceptionMessage` (string | null)
- `terminalLine` (string)
