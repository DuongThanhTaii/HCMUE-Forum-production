# FE-04: (mod) Layout — Moderator Zone

| Property | Value |
|---|---|
| **ID** | FE-04 |
| **Branch** | `feature/FE-04-mod-layout` |
| **Commit** | `feat(fe/layout): implement moderator zone layout` |
| **Priority** | High |
| **Estimate** | 4h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-01, FE-02 |

---

## Objective

Layout cho Zone 2 (Moderator). Compact sidebar + permission guard: chỉ user có role Moderator/Admin mới vào được.

---

## Layout Skeleton

```
┌──────────────────────────────────────────────┐
│  MOD SIDEBAR (200px)  │  CONTENT             │
│ [U] Mod Panel         │                      │
│ ─────────────────     │  {children}          │
│ Reports        [12]   │                      │
│ Posts          [3]    │                      │
│ Approvals      [5]    │                      │
│ AI Content     [2]    │                      │
│ ─────────────────     │                      │
│ ← Back to Forum       │                      │
└──────────────────────────────────────────────┘
```

---

## Files to Create

```
frontend/src/app/[locale]/(mod)/
├── layout.tsx
├── mod/reports/page.tsx         (placeholder — FE-14)
├── mod/posts/page.tsx           (placeholder — FE-14)
├── mod/learning/approvals/page.tsx (placeholder — FE-15)
└── mod/content/page.tsx         (placeholder — FE-15)
frontend/src/components/shared/layouts/
└── ModSidebar.tsx
```

---

## Key Notes

- Permission check: `user.roles.includes('Moderator') || user.roles.includes('Admin')` → nếu không có → redirect `/` với toast "Bạn không có quyền truy cập".
- Sidebar hiện badge count: Reports pending, Approvals pending.
- Link "← Quay lại Forum" ở bottom sidebar.

---

## Acceptance Criteria

- [ ] User không có role Mod/Admin bị redirect
- [ ] Sidebar hiện pending counts
- [ ] Responsive (tablet compatible)
