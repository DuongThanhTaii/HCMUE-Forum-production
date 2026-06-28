# FE-A4: Admin — Dynamic Permission Overrides (User & Group)

| Property    | Value                                                           |
|-------------|-----------------------------------------------------------------|
| **ID**      | FE-A4                                                           |
| **Branch**  | `feature/FE-A4-admin-overrides`                                 |
| **Commit**  | `feat(fe/admin): implement dynamic permission overrides`        |
| **Priority**| Medium                                                          |
| **Estimate**| 4h                                                              |
| **Status**  | ⬜ NOT_STARTED                                                  |
| **Depends** | FE-A1, FE-A2 (permissions list)                                 |
| **Supersedes** | fe-16-admin-users.md (override section — routes were wrong) |

---

## API Endpoints (verified vs BE)

> ⚠️ Routes trong fe-16 cũ sai (`/api/v1/admin/permissions/...`).  
> Routes đúng là `/api/v1/admin/authorization/...`.

### User Overrides

| Action | Method | Endpoint |
|--------|--------|----------|
| Get user overrides | GET | `/api/v1/admin/authorization/users/{userId}/overrides` |
| Upsert user override | POST | `/api/v1/admin/authorization/users/{userId}/overrides` |
| Revoke user override | DELETE | `/api/v1/admin/authorization/users/{userId}/overrides?permissionId={id}&scopeType=Global&scopeValue=` |

### Group Overrides

| Action | Method | Endpoint |
|--------|--------|----------|
| Get group overrides | GET | `/api/v1/admin/authorization/groups/{groupId}/overrides` |
| Upsert group override | POST | `/api/v1/admin/authorization/groups/{groupId}/overrides` |
| Revoke group override | DELETE | `/api/v1/admin/authorization/groups/{groupId}/overrides?permissionId={id}&scopeType=Global&scopeValue=` |

### Request body (upsert):
```json
{
  "permissionId": "uuid",
  "scopeType": "Global",
  "scopeValue": null,
  "effect": "Allow",
  "reason": "Granted for special access",
  "expiresAtUtc": null
}
```

---

## Types (thêm vào `admin.types.ts`)

```typescript
export interface PermissionOverrideDto {
  overrideId: string;
  permissionId: string;
  permissionCode: string;
  scopeType: string;
  scopeValue: string | null;
  effect: 'Allow' | 'Deny';
  reason: string | null;
  expiresAtUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string | null;
  isRevoked: boolean;
}
```

---

## Pages

### `/admin/overrides` — Tabs: Per User | Per Group

```
[● Per User]  [○ Per Group]
────────────────────────────────────────────────
Search user: [email hoặc tên...]  [Load]

User: Nguyễn Văn A (a@hcmue.edu.vn)
──────────────────────────────────────────────────
Permission              | Scope  | Effect | Expires | Actions
forum.posts.create      | Global | Deny   | —       | [Revoke]
learning.documents.read | Global | Allow  | 30/5    | [Revoke]

[+ Add Override]
```

**Add Override form (inline panel hoặc modal):**
- Permission: dropdown từ `GET /api/v1/permissions`
- Effect: Allow | Deny (radio)
- Reason: text input
- Expires: date picker (optional)

**Revoke:** DELETE với query params `permissionId + scopeType=Global + scopeValue=`

**Tab Per Group:** Thay user search bằng group/role selector.  
(Lưu ý: `groupId` là UUID của một user-group — cần load group list nếu BE có endpoint. Nếu chưa có, chỉ implement Per User trước.)

---

## RTK Query (`admin.observability.api.ts` hoặc thêm vào `admin.api.ts`)

```typescript
// User overrides:
getUserOverrides(userId), upsertUserOverride(userId, body), revokeUserOverride(userId, params)
// Group overrides:
getGroupOverrides(groupId), upsertGroupOverride(groupId, body), revokeGroupOverride(groupId, params)
```

---

## Files to Create

```
frontend/src/features/admin/
├── components/
│   ├── AdminOverridesPage.tsx    ← tabs Per User / Per Group
│   ├── UserOverridesPanel.tsx    ← search user + override table
│   ├── GroupOverridesPanel.tsx   ← select group + override table
│   └── AddOverrideForm.tsx       ← permission + effect + reason + expiry
└── hooks/
    └── useAdminOverrides.ts      ← active tab, selected user/group state
```

---

## Acceptance Criteria

- [ ] Tab Per User: tìm user → load overrides
- [ ] Overrides hiển thị đúng permissionCode, effect, scopeType
- [ ] Add override → submit → xuất hiện trong list
- [ ] Revoke override → confirm → biến mất khỏi list
- [ ] Tab Per Group: load group overrides (nếu API hỗ trợ)
- [ ] Expired overrides được highlight
