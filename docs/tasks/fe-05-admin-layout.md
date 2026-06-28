# FE-05: (admin) Layout — Admin Dashboard Zone

| Property | Value |
|---|---|
| **ID** | FE-05 |
| **Branch** | `feature/FE-05-admin-layout` |
| **Commit** | `feat(fe/layout): implement admin dashboard zone layout` |
| **Priority** | High |
| **Estimate** | 4h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-01, FE-02 |

---

## Objective

Layout cho Zone 3 (Admin). Top bar + left nav 180px. Permission guard: chỉ Admin role.

---

## Layout Skeleton

```
┌──────────────────────────────────────────────────────┐
│  TOP BAR: [Logo] Admin Panel    [User] [Logout]      │
├────────────────┬─────────────────────────────────────┤
│  ADMIN NAV     │  CONTENT                            │
│  (180px)       │                                     │
│                │  {children}                         │
│  Users         │                                     │
│  Roles         │                                     │
│  Permissions   │                                     │
│  Endpoints     │                                     │
│  Logs >        │                                     │
│    Actions     │                                     │
│    Auth        │                                     │
│  Career >      │                                     │
│    Companies   │                                     │
│    Jobs        │                                     │
└────────────────┴─────────────────────────────────────┘
```

---

## Files to Create

```
frontend/src/app/[locale]/(admin)/
├── layout.tsx
├── admin/users/page.tsx          (placeholder — FE-16)
├── admin/roles/page.tsx          (placeholder — FE-16)
├── admin/permissions/page.tsx    (placeholder — FE-16)
├── admin/endpoints/page.tsx      (placeholder — FE-17)
├── admin/logs/actions/page.tsx   (placeholder — FE-17)
├── admin/logs/auth/page.tsx      (placeholder — FE-17)
├── admin/career/companies/page.tsx (placeholder — FE-17)
└── admin/career/jobs/page.tsx    (placeholder — FE-17)
frontend/src/components/shared/layouts/
└── AdminLayout.tsx
```

---

## Key Notes

- Guard: `user.roles.includes('Admin')` only → nếu không có → redirect `/`.
- Top bar: logo + "Admin Panel" text + user dropdown + logout button.
- Left nav: collapsible groups (Logs >, Career >).

---

## Acceptance Criteria

- [ ] Non-Admin user bị redirect
- [ ] Top bar + left nav render đúng
- [ ] Collapsible nav groups hoạt động
- [ ] Active nav item highlighted
