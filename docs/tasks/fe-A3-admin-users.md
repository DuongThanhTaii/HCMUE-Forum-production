# FE-A3: Admin — User Management & Badge

| Property    | Value                                                     |
|-------------|-----------------------------------------------------------|
| **ID**      | FE-A3                                                     |
| **Branch**  | `feature/FE-A3-admin-users`                               |
| **Commit**  | `feat(fe/admin): implement user management and badge`     |
| **Priority**| High                                                      |
| **Estimate**| 4h                                                        |
| **Status**  | ⬜ NOT_STARTED                                            |
| **Depends** | FE-A1, FE-A2 (roles list reused)                          |
| **Supersedes** | fe-16-admin-users.md (partial — users section)         |

---

## API Endpoints (verified vs BE)

| Action | Method | Endpoint | Auth |
|--------|--------|----------|------|
| List users | GET | `/api/v1/users` | Bearer |
| Get user | GET | `/api/v1/users/{id}` | Bearer |
| Assign role to user | POST | `/api/v1/users/{userId}/roles` | Admin |
| Remove role from user | DELETE | `/api/v1/users/{userId}/roles/{roleId}` | Admin |
| Assign badge | POST | `/api/v1/users/{userId}/badge` | Admin |
| Remove badge | DELETE | `/api/v1/users/{userId}/badge` | Admin |

### Request bodies

**Assign role to user:**
```json
{ "roleId": "uuid" }
```

**Assign badge:**
```json
{
  "badgeType": "Faculty",
  "badgeName": "Computer Science",
  "description": "Verified by admin"
}
```

> ⚠️ Response `UserDto` includes `badge.emoji` (field không có trong doc cũ, nhưng có trong BE).

---

## Types (thêm vào `admin.types.ts`)

```typescript
export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  bio: string | null;
  status: 'Active' | 'Inactive' | 'Banned';
  badge: BadgeDto | null;
  createdAt: string;
}

export interface BadgeDto {
  type: string;
  name: string;
  description: string;
  emoji: string;        // ← field thực có trong BE response
}
```

---

## Pages

### `/admin/users` — User Table

```
[Search email/name...]  [Filter: All Roles ▾]  [Status ▾]
─────────────────────────────────────────────────────────
Avatar | Email          | Full Name   | Roles      | Badge | Status | Actions
──────   ─────────────   ───────────   ──────────   ─────   ──────   ───────
👤     | a@hcmue.edu.vn | Nguyễn A   | Admin, Mod  | —     | Active | ⋯
```

**Row actions (dropdown):**
- Assign Role → `AssignRoleModal`
- Remove Role → if has roles → select which to remove
- Assign Badge → `AssignBadgeModal`
- Remove Badge → if has badge → confirm dialog

**Filters (client-side after load):**
- Search: email or fullName
- Role: filter by assigned role name
- Status: Active / Inactive / Banned

---

## Modals

### `AssignRoleModal`
- Dropdown list roles từ `GET /api/v1/roles` (RTK cache từ FE-A2)
- Submit → `POST /users/{id}/roles`

### `AssignBadgeModal`
- `badgeType`: select (Faculty / Department / Club / ...)
- `badgeName`: text input
- `description`: text input
- Submit → `POST /users/{id}/badge`

---

## RTK Query (thêm vào `admin.api.ts`)

```typescript
// Endpoints:
getUsers, getUser,
assignRoleToUser, removeRoleFromUser,
assignBadge, removeBadge
```

Tag invalidation:
- assign/remove role/badge → invalidate `['User', id]` + `['Users']`

---

## Files to Create

```
frontend/src/features/admin/
├── components/
│   ├── AdminUsersPage.tsx      ← table + search + filter
│   ├── AssignRoleModal.tsx     ← role dropdown + confirm
│   └── AssignBadgeModal.tsx    ← badge form
└── hooks/
    └── useAdminUsers.ts        ← filter state, modal open/close
```

---

## Acceptance Criteria

- [ ] Users table tải và hiển thị avatar, email, roles, badge, status
- [ ] Search theo email/name hoạt động (client-side)
- [ ] Filter theo role hoạt động
- [ ] Assign role modal → submit → row cập nhật roles
- [ ] Remove role → đúng roleId → row cập nhật
- [ ] Assign badge modal → submit → badge xuất hiện
- [ ] Remove badge → confirm → badge biến mất
