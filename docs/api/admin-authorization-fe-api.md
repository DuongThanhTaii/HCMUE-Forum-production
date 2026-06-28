# Admin Authorization API (FE)

Tài liệu này dành cho màn **Admin tổng / phân quyền** phía FE.

## 1) Auth & conventions

- Base URL: `/api/v1`
- Auth: `Authorization: Bearer <token>`
- Auth required for all endpoints; **RBAC** trên từng endpoint (permission / role). Riêng **`/api/v1/admin/authorization/*`** yêu cầu permission **`admin.system.manage`** (`AuthorizationAdminController`).
- Quản lý role/user/badge (master APIs) vẫn theo quy ước **Admin** như các mục dưới.
- Content-Type: `application/json`
- **Tất cả response đều bọc theo envelope chung**:

```json
{
  "success": true,
  "data": {},
  "message": null,
  "error": null
}
```

- Với API chỉ trả thông báo (không có dữ liệu), `data = null`, ví dụ:

```json
{
  "success": true,
  "data": null,
  "message": "Role assigned successfully",
  "error": null
}
```

- Common error shape:

```json
{
  "success": false,
  "data": null,
  "message": null,
  "error": "Error message"
}
```

- Lưu ý: nhiều mẫu ở dưới mô tả phần **`data`** cho gọn; khi integrate FE hãy parse theo envelope bên trên.

---

## 2) Master data APIs

### 2.1 Get permissions list

- **GET** `/api/v1/permissions`
- Response `200`:

```json
[
  {
    "id": "9cf74b9e-9f5c-46dc-9e28-e196fa2d03f9",
    "code": "forum.post.create",
    "name": "Create forum post",
    "description": "Allows creating forum post",
    "module": "Forum",
    "resource": "Post",
    "action": "Create"
  }
]
```

### 2.2 Get roles list

- **GET** `/api/v1/roles`
- Response `200`:

```json
[
  {
    "id": "3f77f97d-b0d7-493f-befa-bf66640dfcc6",
    "name": "Moderator",
    "description": "Forum moderation role",
    "isDefault": false,
    "isSystemRole": false,
    "permissionCount": 12,
    "createdAt": "2026-03-24T08:00:00Z"
  }
]
```

### 2.1.1 Get permission by id

- **GET** `/api/v1/permissions/{id}`
- Response `200`:

```json
{
  "id": "9cf74b9e-9f5c-46dc-9e28-e196fa2d03f9",
  "code": "forum.post.create",
  "name": "Create forum post",
  "description": "Allows creating forum post",
  "module": "Forum",
  "resource": "Post",
  "action": "Create"
}
```

- `404`: failure envelope

### 2.3 Get users list

- **GET** `/api/v1/users`
- Response `200`:

```json
[
  {
    "id": "f7dd2ca4-7f4a-4cc7-a9f7-f0f343d27433",
    "email": "user@example.com",
    "fullName": "Nguyen Van A",
    "bio": "Student",
    "status": "Active",
    "badge": null,
    "createdAt": "2026-03-24T08:00:00Z"
  }
]
```

### 2.2.1 Get role by id

- **GET** `/api/v1/roles/{id}`
- Response `200`:

```json
{
  "id": "3f77f97d-b0d7-493f-befa-bf66640dfcc6",
  "name": "Moderator",
  "description": "Forum moderation role",
  "isDefault": false,
  "isSystemRole": false,
  "permissionCount": 12,
  "createdAt": "2026-03-24T08:00:00Z"
}
```

- `404`: failure envelope

---

## 3) Role management APIs

> Các API mutate role yêu cầu role Admin.

### 3.1 Create role

- **POST** `/api/v1/roles`
- Request:

```json
{
  "name": "ContentEditor",
  "description": "Can edit content"
}
```

- Response `201`:

```json
{
  "id": "2ea41b8d-1b6f-45d8-a856-7f40b7340b90",
  "name": "ContentEditor",
  "description": "Can edit content",
  "isDefault": false,
  "isSystemRole": false,
  "permissionCount": 0,
  "createdAt": "2026-03-24T09:00:00Z"
}
```

### 3.2 Update role

- **PUT** `/api/v1/roles/{roleId}`
- Request:

```json
{
  "name": "ContentEditor",
  "description": "Can edit and review content"
}
```

- Response `200`:

```json
{
  "message": "Role updated successfully"
}
```

### 3.3 Delete role

- **DELETE** `/api/v1/roles/{roleId}`
- Response `200`:

```json
{
  "message": "Role deleted successfully"
}
```

### 3.4 Assign permission to role

- **POST** `/api/v1/roles/{roleId}/permissions`
- Request:

```json
{
  "permissionId": "9cf74b9e-9f5c-46dc-9e28-e196fa2d03f9",
  "scopeType": "Global",
  "scopeValue": null
}
```

- Response `200`:

```json
{
  "message": "Permission assigned successfully"
}
```

### 3.5 Remove permission from role

- **DELETE** `/api/v1/roles/{roleId}/permissions/{permissionId}?scopeType=Global&scopeValue=`
- Response `200`:

```json
{
  "message": "Permission removed successfully"
}
```

---

## 4) User-role/badge management APIs

> Các API mutate user role/badge yêu cầu Admin.

### 4.1 Assign role to user

- **POST** `/api/v1/users/{userId}/roles`
- Request:

```json
{
  "roleId": "3f77f97d-b0d7-493f-befa-bf66640dfcc6"
}
```

- Response `200`:

```json
{
  "message": "Role assigned successfully"
}
```

### 4.2 Remove role from user

- **DELETE** `/api/v1/users/{userId}/roles/{roleId}`
- Response `200`:

```json
{
  "message": "Role removed successfully"
}
```

### 4.3 Assign badge to user

- **POST** `/api/v1/users/{userId}/badge`
- Request:

```json
{
  "badgeType": "Faculty",
  "badgeName": "Computer Science",
  "description": "Verified by admin"
}
```

- Response `200`:

```json
{
  "message": "Badge assigned successfully"
}
```

### 4.4 Remove badge from user

- **DELETE** `/api/v1/users/{userId}/badge`
- Response `200`:

```json
{
  "message": "Badge removed successfully"
}
```

### 4.5 Current user profile

#### Get current user

- **GET** `/api/v1/users/me`
- Response `200`:

```json
{
  "id": "f7dd2ca4-7f4a-4cc7-a9f7-f0f343d27433",
  "email": "user@example.com",
  "fullName": "Nguyen Van A",
  "bio": "Student",
  "status": "Active",
  "badge": null,
  "createdAt": "2026-03-24T08:00:00Z"
}
```

#### Update current user profile

- **PUT** `/api/v1/users/me/profile`
- Request:

```json
{
  "firstName": "Nguyen",
  "lastName": "Van A",
  "bio": "Student"
}
```

- Response `200`:

```json
{
  "message": "Profile updated successfully"
}
```

---

## 5) Dynamic authorization APIs (Admin)

Base route: `/api/v1/admin/authorization`

**Permission:** `admin.system.manage` cho toàn bộ nhóm API trong section này.

### 5.0 Authorization groups (danh mục nhóm)

#### List groups

- **GET** `/api/v1/admin/authorization/groups`
- **200**: `ApiResponse` — `data` là mảng `{ id, name, description, isActive, memberCount }`

### 5.1 User overrides

#### Get user overrides

- **GET** `/api/v1/admin/authorization/users/{userId}/overrides`
- Response `200`:

```json
[
  {
    "overrideId": "c8f4bbfa-c92a-4fe9-86e0-3cf39de8ef2e",
    "permissionId": "9cf74b9e-9f5c-46dc-9e28-e196fa2d03f9",
    "permissionCode": "forum.post.create",
    "scopeType": "Global",
    "scopeValue": null,
    "effect": "Deny",
    "reason": "Restricted for this user",
    "expiresAtUtc": null,
    "createdAtUtc": "2026-03-24T09:20:00Z",
    "updatedAtUtc": null,
    "isRevoked": false
  }
]
```

### 2.3.1 Get user by id

- **GET** `/api/v1/users/{id}`
- Response `200`:

```json
{
  "id": "f7dd2ca4-7f4a-4cc7-a9f7-f0f343d27433",
  "email": "user@example.com",
  "fullName": "Nguyen Van A",
  "bio": "Student",
  "status": "Active",
  "badge": null,
  "createdAt": "2026-03-24T08:00:00Z"
}
```

- `404`: failure envelope

#### Upsert user override

- **POST** `/api/v1/admin/authorization/users/{userId}/overrides`
- Request:

```json
{
  "permissionId": "9cf74b9e-9f5c-46dc-9e28-e196fa2d03f9",
  "scopeType": "Global",
  "scopeValue": null,
  "effect": "Deny",
  "reason": "Allow only selected APIs",
  "expiresAtUtc": null
}
```

- Response `200`:

```json
{
  "message": "User permission override upserted successfully"
}
```

#### Revoke user override

- **DELETE** `/api/v1/admin/authorization/users/{userId}/overrides?permissionId={permissionId}&scopeType=Global&scopeValue=`
- Response `200`:

```json
{
  "message": "User permission override revoked successfully"
}
```

### 5.2 Group overrides

#### Get group overrides

- **GET** `/api/v1/admin/authorization/groups/{groupId}/overrides`

#### Upsert group override

- **POST** `/api/v1/admin/authorization/groups/{groupId}/overrides`
- Request body: giống user override

#### Revoke group override

- **DELETE** `/api/v1/admin/authorization/groups/{groupId}/overrides?permissionId={permissionId}&scopeType=Global&scopeValue=`

> Response success cho các API group override tương tự user override (`message`).

### 5.3 Endpoint toggles

#### List endpoint toggles

- **GET** `/api/v1/admin/authorization/toggles`
- Response `200`:

```json
[
  {
    "endpointKey": "Api.Identity.AuthorizationAdmin.SetEndpointToggle",
    "isEnabled": true,
    "reason": null,
    "updatedBy": "admin-user",
    "updatedAtUtc": "2026-03-24T09:30:00Z",
    "version": 2
  }
]
```

#### Get one endpoint toggle

- **GET** `/api/v1/admin/authorization/toggles/{endpointKey}`
- Response `200`: 1 object cùng shape như trên.

#### Set endpoint toggle

- **PUT** `/api/v1/admin/authorization/toggles/{endpointKey}`
- Request:

```json
{
  "isEnabled": false,
  "reason": "Maintenance window"
}
```

- Response `200`: object `EndpointToggleResponse`.

### 5.4 Maintenance mode

Toggle hệ thống triển khai qua **endpoint toggle** key cố định `System.Maintenance.Mode`.

#### Get maintenance mode

- **GET** `/api/v1/admin/authorization/maintenance-mode`
- **200**: `ApiResponse<MaintenanceModeResponse>`

```json
{
  "isEnabled": false,
  "reason": null,
  "updatedBy": "system",
  "updatedAtUtc": "2026-05-10T12:00:00Z",
  "version": 1
}
```

#### Set maintenance mode

- **PUT** `/api/v1/admin/authorization/maintenance-mode`
- **Body**:

```json
{
  "isEnabled": true,
  "reason": "Scheduled maintenance"
}
```

- **200**: `ApiResponse<MaintenanceModeResponse>` — payload giống GET sau khi cập nhật.

### 5.5 Authorization audit logs

- **GET** `/api/v1/admin/authorization/audit-logs?userId=&endpointKey=&isSuccess=&fromUtc=&toUtc=&take=100`
- Response `200`:

```json
{
  "success": true,
  "data": [
    {
      "auditLogId": "8c4e5f67-f51b-4b80-b99b-42f1f891b478",
      "actorUserId": "f7dd2ca4-7f4a-4cc7-a9f7-f0f343d27433",
      "action": "EndpointToggle.Update",
      "targetType": "EndpointToggle",
      "targetKey": "Api.Identity.AuthorizationAdmin.SetEndpointToggle",
      "isSuccess": true,
      "detail": "Endpoint set to disabled",
      "occurredAtUtc": "2026-03-24T09:35:00Z"
    }
  ],
  "message": null,
  "error": null
}
```

---

## 6) User action logs API (terminal-like data source)

Base route: `/api/v1/admin/observability/user-actions`

### 6.1 Search logs

- **GET** `/api/v1/admin/observability/user-actions`
- Query params:
  - `actorUserId`, `correlationId`, `traceId`, `method`, `pathContains`
  - `minStatusCode`, `maxStatusCode`
  - `fromUtc`, `toUtc`
  - `viewType` = `Developer` | `Administrator` (default `Developer`)
  - `page` (default 1), `pageSize`

- Response `200`:

```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": "67e0a6a5b58d674d0c8f8fbe",
        "actorUserId": "user-123",
        "method": "PUT",
        "path": "/api/v1/admin/authorization/toggles/Api.Identity.AuthorizationAdmin.SetEndpointToggle",
        "queryString": "",
        "endpoint": "...AuthorizationAdminController.SetEndpointToggle...",
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
        "terminalLine": "[2026-03-24 08:12:11.165] [200] PUT /api/..."
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

---

## 7) FE implementation notes

- Ưu tiên gọi song song:
  - `/api/v1/permissions`, `/api/v1/roles`, `/api/v1/users` để load initial admin screen.
- Khi cập nhật phân quyền:
  - mutate (`POST/PUT/DELETE`) xong thì re-fetch danh sách liên quan.
- `viewType` cho log screen:
  - tab **Dev** => `Developer`
  - tab **Admin** => `Administrator`
- `terminalLine` đã format sẵn để render nhanh kiểu terminal; FE vẫn có full fields để custom UI.

---

## 8) Schemas

### `PermissionResponse`
- `id` (guid)
- `code` (string)
- `name` (string)
- `description` (string | null)
- `module` (string)
- `resource` (string)
- `action` (string)

### `RoleResponse`
- `id` (guid)
- `name` (string)
- `description` (string | null)
- `isDefault` (boolean)
- `isSystemRole` (boolean)
- `permissionCount` (int)
- `createdAt` (datetime)

### `CreateRoleRequest`
- `name` (string)
- `description` (string | null)

### `UpdateRoleRequest`
- `name` (string)
- `description` (string | null)

### `AssignPermissionRequest`
- `permissionId` (guid)
- `scopeType` (string)
- `scopeValue` (string | null)

### `UserResponse`
- `id` (guid)
- `email` (string)
- `fullName` (string)
- `bio` (string | null)
- `status` (string)
- `badge` (OfficialBadgeDto | null)
- `createdAt` (datetime)

### `OfficialBadgeDto`
- `type` (string)
- `name` (string)
- `description` (string | null)
- `emoji` (string)

### `AssignRoleRequest`
- `roleId` (guid)

### `AssignBadgeRequest`
- `badgeType` (string)
- `badgeName` (string)
- `description` (string | null)

### `UpdateProfileRequest`
- `firstName` (string)
- `lastName` (string)
- `bio` (string | null)

### `PermissionOverrideResponse`
- `overrideId` (guid)
- `permissionId` (guid)
- `permissionCode` (string)
- `scopeType` (string)
- `scopeValue` (string | null)
- `effect` (string)
- `reason` (string | null)
- `expiresAtUtc` (datetime | null)
- `createdAtUtc` (datetime)
- `updatedAtUtc` (datetime | null)
- `isRevoked` (boolean)

### `UpsertPermissionOverrideRequest`
- `permissionId` (guid)
- `scopeType` (string)
- `scopeValue` (string | null)
- `effect` (string)
- `reason` (string | null)
- `expiresAtUtc` (datetime | null)

### `RevokePermissionOverrideRequest`
- `permissionId` (guid)
- `scopeType` (string)
- `scopeValue` (string | null)

### `EndpointToggleResponse`
- `endpointKey` (string)
- `isEnabled` (boolean)
- `reason` (string | null)
- `updatedBy` (string)
- `updatedAtUtc` (datetime)
- `version` (int)

### `SetEndpointToggleRequest`
- `isEnabled` (boolean)
- `reason` (string | null)

### `MaintenanceModeResponse`
- `isEnabled` (boolean)
- `reason` (string | null)
- `updatedBy` (string)
- `updatedAtUtc` (datetime)
- `version` (int)

### `SetMaintenanceModeRequest`
- `isEnabled` (boolean)
- `reason` (string | null)

### `AuthorizationAuditLogResponse`
- `auditLogId` (guid)
- `actorUserId` (guid | null)
- `action` (string)
- `targetType` (string)
- `targetKey` (string | null)
- `isSuccess` (boolean)
- `detail` (string | null)
- `occurredAtUtc` (datetime)
