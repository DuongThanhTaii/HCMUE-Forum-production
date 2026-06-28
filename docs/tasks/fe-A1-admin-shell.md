# FE-A1: Admin Shell — Layout, Guard, Sidebar, Router

| Property    | Value                                                    |
|-------------|----------------------------------------------------------|
| **ID**      | FE-A1                                                    |
| **Branch**  | `feature/FE-A1-admin-shell`                              |
| **Commit**  | `feat(fe/admin): implement admin shell layout and guard` |
| **Priority**| High                                                     |
| **Estimate**| 3h                                                       |
| **Status**  | ⬜ NOT_STARTED                                           |
| **Depends** | Auth slice (roles populated from `GET /api/v1/users/me`) |
| **Supersedes** | fe-05-admin-layout.md (routes updated)               |

---

## Objective

Dựng shell Admin Zone: layout, role guard, sidebar nav, và wire placeholder routes cho A2–A5. Không có business logic ở bước này.

---

## Layout

```
┌──────────────────────────────────────────────────────┐
│  TOP BAR: [Logo] Admin Panel          [User] [Back]  │
├──────────────┬───────────────────────────────────────┤
│  ADMIN NAV   │  CONTENT AREA ({children})            │
│  (200px)     │                                       │
│              │                                       │
│  Users       │                                       │
│  Roles       │                                       │
│  Permissions │                                       │
│  ─────────── │                                       │
│  Overrides ▾ │                                       │
│    Per User  │                                       │
│    Per Group │                                       │
│  ─────────── │                                       │
│  Toggles     │                                       │
│  ─────────── │                                       │
│  Logs ▾      │                                       │
│    Actions   │                                       │
│    Auth Audit│                                       │
└──────────────┴───────────────────────────────────────┘
```

---

## Routes to add in `router.tsx`

```
/admin                     → redirect → /admin/users
/admin/users               → AdminUsersPage        (FE-A3)
/admin/roles               → AdminRolesPage        (FE-A2)
/admin/permissions         → AdminRolesPage (alias, tab)
/admin/overrides/users     → AdminOverridesPage    (FE-A4)
/admin/overrides/groups    → AdminOverridesPage    (FE-A4, tab)
/admin/toggles             → AdminTogglesPage      (FE-A5)
/admin/logs/actions        → AdminActionLogsPage   (FE-A5)
/admin/logs/audit          → AdminAuditLogsPage    (FE-A5)
```

All routes wrapped under `<AdminGuard>` (check role === "Admin").

---

## Files to Create

```
frontend/src/features/admin/
├── components/
│   ├── AdminLayout.tsx         ← shell layout wrapper
│   └── AdminSidebar.tsx        ← nav with collapsible groups
frontend/src/app/guards/
└── AdminGuard.tsx              ← redirect non-Admin → /home
```

---

## `AdminGuard` Logic

```typescript
// if user.roles does not include "Admin" → navigate("/home")
// reads from auth.slice: selectUserRoles (already in auth.slice.ts)
```

---

## Acceptance Criteria

- [ ] Non-Admin → redirect `/home`
- [ ] Admin → AdminLayout renders with sidebar
- [ ] All `/admin/*` routes show correct placeholder or page
- [ ] Active nav item highlighted
- [ ] "Back" button → `/home`
