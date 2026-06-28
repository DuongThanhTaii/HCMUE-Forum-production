# FE-17: Admin Zone — Logs, Endpoint Toggles, Career Admin

| Property | Value |
|---|---|
| **ID** | FE-17 |
| **Branch** | `feature/FE-17-admin-logs` |
| **Commit** | `feat(fe/admin): implement system logs, endpoint toggles and career admin` |
| **Priority** | Medium |
| **Estimate** | 6h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-05 |

---

## API Endpoints

| Action | Endpoint |
|---|---|
| User action logs | GET `/api/v1/admin/logs/actions?userId=&endpoint=&from=&to=&page=` |
| Auth audit logs | GET `/api/v1/admin/logs/auth?page=` |
| Get endpoint toggles | GET `/api/v1/admin/endpoints` |
| Set endpoint toggle | POST `/api/v1/admin/endpoints` body: `{ key, isEnabled }` |
| Get pending companies | GET `/api/v1/career/companies?status=Pending` |
| Approve company | POST `/api/v1/career/companies/{id}/approve` |
| Reject company | POST `/api/v1/career/companies/{id}/reject` |
| All job postings | GET `/api/v1/career/jobs/admin?page=` |
| Close job | POST `/api/v1/career/jobs/{id}/close` |

---

## Pages

### `/admin/logs/actions` — User Action Logs

**Filter bar:** User ID / Email, Endpoint path, Date range picker, Method (GET/POST/PUT/DELETE)

**Table:**
| Timestamp | User | Method | Endpoint | Status | Duration |
|-----------|------|--------|----------|--------|----------|
| 14:23:01 | user@... | POST | /api/v1/forum/posts | 201 | 45ms |

- Color coding: GET=muted, POST=primary, DELETE=destructive, PUT=warning
- Status color: 2xx=success, 4xx=warning, 5xx=destructive
- Pagination: 50 per page

### `/admin/logs/auth` — Auth Audit Logs

| Timestamp | User | Action | Resource | Result |
|-----------|------|--------|----------|--------|
| 14:20:00 | admin | GRANT | forum.post.pin | ALLOWED |
| 14:15:00 | user123 | CHECK | forum.post.delete | DENIED |

Result color: ALLOWED=success, DENIED=destructive

### `/admin/endpoints` — Endpoint Toggles

```
┌────────────────────────────────────────────┐
│  Endpoint Toggles                          │
│  ──────────────────────────────────────    │
│  POST /api/v1/forum/posts        [ON  ●]  │
│  DELETE /api/v1/forum/posts/{id} [ON  ●]  │
│  POST /api/v1/career/jobs        [OFF ○]  │
│  ...                                       │
└────────────────────────────────────────────┘
```

Toggle switch với confirm dialog (destructive toggle-off cần confirm).

### `/admin/career/companies` — Company Approvals

List of pending company registrations:
| Company | Email | Submitted | Actions |
|---------|-------|-----------|---------|
| XYZ Corp | hr@xyz.com | 28/4 | [Approve] [Reject] |

### `/admin/career/jobs` — Job Moderation

Table of all jobs with Close button for each active job.

---

## Components

```
components/features/admin/
├── LogsTable.tsx              ← generic log table
├── ActionLogsFilters.tsx
├── EndpointToggleList.tsx
├── EndpointToggleRow.tsx      ← switch + confirm dialog
├── CompanyApprovalCard.tsx
└── JobAdminTable.tsx
```

---

## Acceptance Criteria

- [ ] User action logs load với filters
- [ ] Auth audit logs load
- [ ] Date range filter hoạt động
- [ ] Endpoint toggles hiện đúng trạng thái
- [ ] Toggle switch với confirm dialog
- [ ] Company approval/reject hoạt động
- [ ] Job close hoạt động
- [ ] Logs pagination hoạt động
