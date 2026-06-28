# FE-A5: Admin — Endpoint Toggles, Auth Audit Logs, User Action Logs

| Property    | Value                                                              |
|-------------|--------------------------------------------------------------------|
| **ID**      | FE-A5                                                              |
| **Branch**  | `feature/FE-A5-admin-observability`                                |
| **Commit**  | `feat(fe/admin): implement toggles, audit logs and action logs`    |
| **Priority**| Medium                                                             |
| **Estimate**| 5h                                                                 |
| **Status**  | ⬜ NOT_STARTED                                                     |
| **Depends** | FE-A1                                                              |
| **Supersedes** | fe-17-admin-logs.md (routes were wrong throughout)              |

---

## API Endpoints (verified vs BE)

> ⚠️ Routes trong fe-17 cũ sai. Routes đúng dưới đây.

### Endpoint Toggles

| Action | Method | Endpoint |
|--------|--------|----------|
| List toggles | GET | `/api/v1/admin/authorization/toggles` |
| Get one toggle | GET | `/api/v1/admin/authorization/toggles/{endpointKey}` |
| Set toggle | PUT | `/api/v1/admin/authorization/toggles/{endpointKey}` |

**PUT body:**
```json
{ "isEnabled": false, "reason": "Maintenance" }
```

**Toggle response shape:**
```json
{
  "endpointKey": "Api.Identity.AuthorizationAdmin.SetEndpointToggle",
  "isEnabled": true,
  "reason": null,
  "updatedBy": "admin-user",
  "updatedAtUtc": "2026-04-29T09:30:00Z",
  "version": 2
}
```

### Auth Audit Logs

| Action | Method | Endpoint |
|--------|--------|----------|
| Search audit logs | GET | `/api/v1/admin/authorization/audit-logs?userId=&endpointKey=&isSuccess=&fromUtc=&toUtc=&take=100` |

### User Action Logs (Observability)

| Action | Method | Endpoint |
|--------|--------|----------|
| Search action logs | GET | `/api/v1/admin/observability/user-actions?actorUserId=&correlationId=&traceId=&method=&pathContains=&minStatusCode=&maxStatusCode=&fromUtc=&toUtc=&viewType=Developer&page=1&pageSize=100` |

> ⚠️ `pageSize` default thực tế là **100** (doc cũ ghi 50).  
> `viewType`: `Developer` | `Administrator`

---

## Types (thêm vào `admin.types.ts`)

```typescript
export interface EndpointToggleDto {
  endpointKey: string;
  isEnabled: boolean;
  reason: string | null;
  updatedBy: string;
  updatedAtUtc: string;
  version: number;
}

export interface AuditLogDto {
  auditLogId: string;
  actorUserId: string | null;
  action: string;
  targetType: string;
  targetKey: string;
  isSuccess: boolean;
  detail: string;
  occurredAtUtc: string;
}

export interface UserActionLogItemDto {
  id: string;
  actorUserId: string;
  method: string;
  path: string;
  statusCode: number;
  durationMs: number;
  startedAtUtc: string;
  result: string;
  terminalLine: string;
  // ... full fields per api doc
}

export interface UserActionLogsResponse {
  items: UserActionLogItemDto[];
  total: number;
  page: number;
  pageSize: number;
  viewType: string;
}
```

---

## Pages

### `/admin/toggles` — Endpoint Toggle List

```
Endpoint Toggles                          [Refresh]
──────────────────────────────────────────────────
Api.Identity.AuthorizationAdmin.Login       [ON  ●]
Api.Forum.Posts.Create                      [OFF ○]
Api.Learning.Documents.Upload               [ON  ●]
```

- Toggle switch → confirm dialog khi disable
- Dialog: "Disable [endpointKey]? Reason: [input]" → PUT
- `updatedBy` + `version` hiển thị trong tooltip

### `/admin/logs/audit` — Auth Audit Logs

**Filter bar:** userId, endpointKey, isSuccess (All/Yes/No), fromUtc, toUtc, take

```
Timestamp       | Actor         | Action                | Target              | Result
2026-04-29 12:00| admin@...     | EndpointToggle.Update | Api.Forum.Posts...  | ✅
2026-04-29 11:50| user@...      | Permission.Check      | forum.posts.create  | ❌
```

Color: `isSuccess=true` → green check, false → red cross.

### `/admin/logs/actions` — User Action Logs

**Filter bar:** actorUserId, pathContains, method, minStatusCode, maxStatusCode, fromUtc, toUtc, viewType tab

```
[Developer tab]  [Administrator tab]
────────────────────────────────────────────────────
Timestamp    | User     | Method | Path              | Status | Duration
12:00:01.165 | user@... | POST   | /api/v1/forum/... | 201    | 45ms
```

Color coding:
- Method: GET=muted, POST=blue, PUT=yellow, DELETE=red
- Status: 2xx=green, 4xx=yellow, 5xx=red

Pagination: page / pageSize (100 default, allow 25/50/100 select).

`terminalLine` field available → optional "Terminal View" toggle renders raw text lines.

---

## RTK Query (`admin.observability.api.ts`)

```typescript
// Endpoint toggles:
getToggles(), getToggle(endpointKey), setToggle(endpointKey, body)

// Audit logs:
getAuditLogs(params)

// User action logs:
getUserActionLogs(params)
```

---

## Files to Create

```
frontend/src/features/admin/
├── api/
│   └── admin.observability.api.ts     ← toggles + audit + action logs
├── components/
│   ├── AdminTogglesPage.tsx            ← toggle list with switch
│   ├── EndpointToggleRow.tsx           ← switch + confirm dialog
│   ├── AdminAuditLogsPage.tsx          ← auth audit log table + filters
│   ├── AdminActionLogsPage.tsx         ← user action log table + filters
│   └── LogsFilterBar.tsx               ← shared filter bar component
└── hooks/
    └── useAdminLogs.ts                 ← filter state, pagination
```

---

## Acceptance Criteria

- [ ] Toggles list tải đúng trạng thái isEnabled
- [ ] Toggle switch → confirm dialog → PUT → trạng thái cập nhật
- [ ] Audit logs tải với filter userId / isSuccess / date range
- [ ] Action logs tải với filter method / path / status / date range
- [ ] Developer vs Administrator viewType tab hoạt động
- [ ] Pagination: page chuyển đúng
- [ ] Status + method color coding đúng
