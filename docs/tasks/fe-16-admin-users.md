# FE-16: Admin Zone — Users, Roles, Permissions

| Property | Value |
|---|---|
| **ID** | FE-16 |
| **Branch** | `feature/FE-16-admin-users` |
| **Commit** | `feat(fe/admin): implement user management, roles and permission overrides` |
| **Priority** | Medium |
| **Estimate** | 8h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-05 |

---

## API Endpoints

| Action | Endpoint |
|---|---|
| Get all users | GET `/api/v1/users` |
| Get user | GET `/api/v1/users/{id}` |
| Assign role | POST `/api/v1/users/{id}/roles` |
| Remove role | DELETE `/api/v1/users/{id}/roles/{roleId}` |
| Assign badge | POST `/api/v1/users/{id}/badge` |
| Remove badge | DELETE `/api/v1/users/{id}/badge` |
| Get all roles | GET `/api/v1/roles` |
| Create role | POST `/api/v1/roles` |
| Update role | PUT `/api/v1/roles/{id}` |
| Delete role | DELETE `/api/v1/roles/{id}` |
| Assign permission to role | POST `/api/v1/roles/{id}/permissions` |
| Remove permission from role | DELETE `/api/v1/roles/{id}/permissions/{permId}` |
| Get user permission overrides | GET `/api/v1/admin/permissions/users/{userId}` |
| Upsert user override | POST `/api/v1/admin/permissions/users/{userId}` |
| Revoke user override | DELETE `/api/v1/admin/permissions/users/{userId}/{permId}` |
| Get group overrides | GET `/api/v1/admin/permissions/groups/{groupId}` |
| Upsert group override | POST `/api/v1/admin/permissions/groups/{groupId}` |
| Revoke group override | DELETE `/api/v1/admin/permissions/groups/{groupId}/{permId}` |

---

## Pages

### `/admin/users` — User Management

**Table (TanStack Table):**
| Avatar | Email | Full Name | Role(s) | Badge | Status | Actions |
|--------|-------|-----------|---------|-------|--------|---------|
| [img] | a@hcmue.edu.vn | Nguyễn A | Admin, Mod | [Dept] | Active | [Edit] |

- Search by email/name
- Filter by role
- Bulk select + bulk assign role
- Row actions: [Assign Role] [Assign Badge] [View Profile]

**Assign Role Modal:** Select role from dropdown → confirm  
**Assign Badge Modal:** Select badge type + name + description → confirm

### `/admin/roles` — Role Management

**Left panel:** Role list  
**Right panel:** Selected role → permissions list

- Create new role button → modal (name + description)
- Edit role name
- Delete role (with confirmation)
- Per role: toggle permission checkboxes

### `/admin/permissions` — Permission Overrides

**Tab 1: Per User**
- Search user → load their overrides
- Add override: permission + Allow/Deny
- Remove override button

**Tab 2: Per Group/Role**
- Select group → load overrides
- Same add/remove pattern

---

## Components

```
components/features/admin/
├── UserManagementTable.tsx    ← TanStack Table + actions
├── AssignRoleModal.tsx
├── AssignBadgeModal.tsx
├── RoleList.tsx               ← left panel
├── RolePermissions.tsx        ← permission checkboxes
├── CreateRoleModal.tsx
├── PermissionOverridesTab.tsx ← per user/group
└── StatsCard.tsx              ← summary stats cards
```

---

## Acceptance Criteria

- [ ] Users table load với search + filter
- [ ] Assign/remove role hoạt động
- [ ] Assign/remove badge hoạt động
- [ ] Roles list CRUD hoạt động
- [ ] Permission toggle per role
- [ ] Per-user override add/remove
- [ ] Per-group override add/remove
