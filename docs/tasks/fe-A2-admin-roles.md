# FE-A2: Admin — Roles & Permissions Management

| Property    | Value                                                           |
|-------------|-----------------------------------------------------------------|
| **ID**      | FE-A2                                                           |
| **Branch**  | `feature/FE-A2-admin-roles`                                     |
| **Commit**  | `feat(fe/admin): implement roles and permissions management`    |
| **Priority**| High                                                            |
| **Estimate**| 5h                                                              |
| **Status**  | ⬜ NOT_STARTED                                                  |
| **Depends** | FE-A1                                                           |
| **Supersedes** | fe-16-admin-users.md (partial — roles/perms section)         |

---

## API Endpoints (verified vs BE)

> Tất cả response bọc envelope `{ success, data, message, error }`.

| Action | Method | Endpoint | Auth |
|--------|--------|----------|------|
| List permissions | GET | `/api/v1/permissions` | Bearer |
| Get permission | GET | `/api/v1/permissions/{id}` | Bearer |
| List roles | GET | `/api/v1/roles` | Bearer |
| Get role | GET | `/api/v1/roles/{id}` | Bearer |
| Create role | POST | `/api/v1/roles` | Admin |
| Update role | PUT | `/api/v1/roles/{id}` | Admin |
| Delete role | DELETE | `/api/v1/roles/{id}` | Admin |
| Assign permission to role | POST | `/api/v1/roles/{id}/permissions` | Admin |
| Remove permission from role | DELETE | `/api/v1/roles/{id}/permissions/{permissionId}?scopeType=Global&scopeValue=` | Admin |

### Request bodies

**Create / Update role:**
```json
{ "name": "ContentEditor", "description": "Can edit content" }
```

**Assign permission to role:**
```json
{ "permissionId": "uuid", "scopeType": "Global", "scopeValue": null }
```

---

## Types (`admin.types.ts`)

```typescript
export interface PermissionDto {
  id: string;
  code: string;        // e.g. "forum.posts.create"
  name: string;
  description: string;
  module: string;
  resource: string;
  action: string;
}

export interface RoleDto {
  id: string;
  name: string;
  description: string;
  isDefault: boolean;
  isSystemRole: boolean;
  permissionCount: number;
  createdAt: string;
}
```

---

## Pages

### `/admin/roles` — Role Manager

**Layout:** 2-column (left: role list, right: permission grid)

```
┌─────────────────┬──────────────────────────────────────┐
│ Roles           │ Permissions for: Student             │
│ ─────────────── │ ─────────────────────────────────── │
│ [+ New Role]    │ FORUM                                │
│                 │  [✓] forum.posts.read                │
│ ● Admin         │  [✓] forum.posts.create              │
│ ○ Moderator     │  [ ] forum.posts.delete              │
│ ○ Lecturer      │ LEARNING                             │
│ ● Student       │  [✓] learning.documents.read         │
│                 │  [ ] learning.documents.create       │
└─────────────────┴──────────────────────────────────────┘
```

- Clicking a role loads its permissions (from `GET /roles/{id}` + compare `GET /permissions`)
- Toggling a checkbox → `POST /roles/{id}/permissions` or `DELETE /roles/{id}/permissions/{permId}`
- isSystemRole = true → disable delete
- [+ New Role] → inline modal (name + description)

---

## RTK Query Slice (`admin.api.ts` — roles section)

```typescript
// Endpoints:
getRoles, getRole, createRole, updateRole, deleteRole,
getPermissions, getPermission,
assignPermissionToRole, removePermissionFromRole
```

Tag-based invalidation:
- `createRole` / `deleteRole` / `updateRole` → invalidate `['Role']`
- `assign/remove` permission → invalidate `['Role', id]`

---

## Files to Create

```
frontend/src/features/admin/
├── api/
│   └── admin.api.ts               ← RTK Query (roles + permissions sections)
├── components/
│   ├── AdminRolesPage.tsx          ← 2-column layout
│   ├── RoleList.tsx                ← left panel with create button
│   ├── RolePermissionGrid.tsx      ← permission checkboxes grouped by module
│   └── CreateRoleModal.tsx         ← name + description form
├── hooks/
│   └── useAdminRoles.ts            ← selected role state, toggle handler
└── types/
    └── admin.types.ts              ← PermissionDto, RoleDto
```

---

## Acceptance Criteria

- [ ] Roles list loads và hiển thị đúng
- [ ] Chọn role → load permissions, đánh dấu assigned
- [ ] Toggle checkbox → assign / remove permission, optimistic update
- [ ] Create role modal → submit → role xuất hiện trong list
- [ ] Delete role (non-system) → confirm dialog → xóa
- [ ] isSystemRole=true → delete button disabled
