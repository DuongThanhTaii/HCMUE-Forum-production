# Production plan: Observability & system monitoring — permission-based (B)

**Date:** 2026-05-03  
**Goal:** Tách rõ **“giám sát / vận hành hệ thống”** khỏi **“quản trị vai trò & người dùng”** bằng **permission**, siết **BE + FE**, sẵn sàng production.  
**Phương án:** **B — Permission-based** (không dựa vào một role cố định `Admin` cho mọi màn observability).

**Kèm theo (UI admin + toggles + override):** `docs/plans/2026-05-03-admin-rbac-dynamic-ui-endpoint-toggles.md` — nhóm quyền theo module, endpoint toggles có cụm + switch trên FE, và mô tả **ghi đè quyền** vs **role**.

---

## 1. Executive summary

| Hiện trạng | Rủi ro / hạn chế |
|------------|------------------|
| `UserActionLogsController` chỉ `[Authorize]` — bất kỳ user đăng nhập nào cũng có thể gọi API nếu biết URL. | **Rủi ro bảo mật nghiêm trọng.** |
| `AuthorizationAdminController` cả class `[Authorize(Roles = "Admin")]` — toggles/audit/override cùng một lớp. | **Ops** và **Identity admin** không tách được. |
| JWT chỉ chứa **roles**, không chứa **permission codes** (`JwtService`). | FE chỉ gate được bằng `AdminGuard` (role), không khớp model permission trong DB. |
| `IdentitySeed` tạo permissions trong DB nhưng **không gán RolePermission** trong snippet seed cổ điển — quyền thực tế có thể đến từ bulk seed / tay. | Cần **data migration rõ ràng** khi thêm permission mới. |

**Trạng thái đích:** Mọi endpoint observability & endpoint toggle đều yêu cầu **permission cụ thể**; FE ẩn route/menu theo **effective permissions** của user; có **API** để client lấy permissions (sau login / refresh); mặc định **chỉ** user có quyền mới thấy dữ liệu nhạy cảm (`viewType=Developer`).

---

## 2. Permission catalog (đề xuất — chốt trong PR implement)

Quy ước mã: `{module}.{resource}.{action}` (đồng nhất `IdentitySeed`).

### 2.1 Observability (HTTP / middleware logs)

| Code | Ý nghĩa |
|------|---------|
| `observability.user-actions.read` | Đọc log hành động user; cho phép `viewType=Administrator` (đã redact query/IP/UA/exception chi tiết). |
| `observability.user-actions.read-sensitive` | Đọc log với `viewType=Developer` (đầy đủ kỹ thuật). **Chỉ** team vận hành / security. |

### 2.2 Authorization subsystem (toggles + audit)

| Code | Ý nghĩa |
|------|---------|
| `observability.endpoint-toggles.read` | GET toggles (danh sách / theo key). |
| `observability.endpoint-toggles.manage` | PUT toggle (bật/tắt endpoint). |
| `observability.authorization-audit.read` | GET authorization audit logs (`/audit-logs`). |

### 2.3 Identity administration (giữ tách biệt)

Các API override user/group trong `AuthorizationAdminController` **không** phải observability — gán permission identity hiện có hoặc tách rõ:

| Code | Ý nghĩa |
|------|---------|
| `identity.authorization.overrides.read` | GET overrides. |
| `identity.authorization.overrides.manage` | POST/DELETE overrides. |

*(Nếu muốn tối giản phase 1: giữ overrides chỉ cho role Admin qua policy `identity.admin.*` — nhưng plan production-ready nên **tách permission** như trên.)*

### 2.4 Legacy compatibility

- `admin.system.manage`: có thể **coi là wildcard** trong handler (optional): nếu user có permission này → grant tất cả observability + identity admin. Giảm break cho DB đã gán sẵn. **Deadline:** sau 1–2 release chỉ dùng granular permissions.

---

## 3. Backend — kiến trúc

### 3.1 Authorization handler (ASP.NET Core)

1. Implement **`IAuthorizationHandler`** + requirement kiểu `PermissionRequirement(string permissionCode)`.
2. Đăng ký **`PermissionAuthorizationHandler`** dùng **`IPermissionChecker.HasPermissionAsync(UserId, code)`** (đã có cache Redis/in-memory).
3. Đăng ký policies động hoặc named policies, ví dụ:
   - `"Permission:observability.user-actions.read"`
   - `"Permission:observability.user-actions.read-sensitive"`

**Lưu ý production:** Handler phải resolve `UserId` từ `ClaimTypes.NameIdentifier` giống các controller hiện tại; fail closed → **403**.

### 3.2 Áp policy lên controller / action

| Target | Thay đổi |
|--------|----------|
| `UserActionLogsController.Search` | `[Authorize(Policy = "...read")]` tối thiểu. Trong action: nếu query `viewType == Developer` và **không** có `...read-sensitive` → **403** hoặc **force downgrade** sang `Administrator` (khuyến nghị: **403** để tránh “lộ” qua nhầm UI). |
| `AuthorizationAdminController` | **Bỏ** `[Authorize(Roles = "Admin")]` ở class level; gắn từng nhóm action: overrides → identity permissions; `toggles` GET → `endpoint-toggles.read`; PUT → `manage`; `audit-logs` → `authorization-audit.read`. |

### 3.3 Các controller Admin khác (Users, Roles, Permissions)

- Rà soát `[Authorize(Roles = "Admin")]` → chuyển dần sang policy `identity.users.*`, `identity.roles.*` đã có trong seed hoặc map `admin.system.manage`.

*(Có thể làm **song song phase 2** nếu scope phase 1 chỉ observability.)*

---

## 4. Backend — dữ liệu & migration

1. **Thêm rows** vào bảng `Permissions` cho các code mới (EF migration hoặc script SQL idempotent `IF NOT EXISTS`).
2. **Gán RolePermission:**
   - Role **Admin** (seed): gán **full** observability + identity (giữ tương đương hiện tại).
   - Tạo role optional **`SystemOperator`** hoặc **`ObservabilityViewer`** (tùy tổ chức): chỉ `observability.*.read` (không `manage`, không `overrides`).
3. **Môi trường đã deploy:** migration **data-only** chạy trong pipeline hoặc job một lần; log số permission đã insert.

---

## 5. Backend — API cho FE: effective permissions

**Vấn đề:** JWT không có permission claims → FE không gate được mà không gọi server.

**Giải pháp production:**

1. Thêm **`GET /api/v1/users/me/permissions`** (hoặc `/api/v1/auth/me/effective-permissions`):
   - Auth bắt buộc.
   - Trả về `{ permissionCodes: string[] }` (hoặc `{ permissions: [{ code, module }] }`).
   - Implement qua **permission resolution** tái sử dụng logic `PermissionChecker` / cache.
2. **Optional (phase 2):** Thêm permission codes vào JWT tại login/refresh để giảm round-trip — cân nhắc **size token** và **invalidation** khi đổi role (ưu tiên **API me/permissions** + RTK Query cache invalidation sau assign role).

---

## 6. Frontend — routing & UX

### 6.1 Guard

1. Thêm **`RequirePermission`** (hoặc `PermissionGuard`) nhận một hoặc nhiều `permissionCode`, logic OR/AND rõ ràng.
2. Nguồn permission: RTK Query **`useGetMyPermissionsQuery`** gọi endpoint mới; cache trong Redux hoặc context auth.
3. **Thay thế dần** `AdminGuard` (role === Admin) cho các route:
   - `/admin/users`, `/admin/roles`, … → policy identity permissions hoặc role Admin tạm thời.
   - `/admin/logs/actions`, `/admin/logs/audit`, `/admin/toggles` → **permission** observability.

### 6.2 Navigation (`AdminLayout` / sidebar)

- Ẩn mục menu nếu không có permission (không chỉ disable).
- **Developer view** toggle trên `AdminActionLogsPage`: chỉ hiện nếu có `observability.user-actions.read-sensitive`.

### 6.3 Lỗi 403

- Toast + redirect an toàn (`/home` hoặc `/unauthorized`).

---

## 7. Tài liệu & OpenAPI

1. Cập nhật **`docs/api/user-action-logs-api.md`**: Authorization = permission, mô tả `viewType` + permission bắt buộc.
2. Thêm **`docs/api/me-permissions-fe-api.md`** (ngắn).
3. Scalar/OpenAPI: XML comments trên controller mới.

---

## 8. Bảo mật & vận hành (production checklist)

| Hạng mục | Ghi chú |
|----------|---------|
| **Least privilege** | Không gán `read-sensitive` cho role rộng. |
| **Rate limiting** | Áp dụng policy rate limit cho `GET user-actions` (đã có infrastructure rate limit trong `Program.cs` — mở rộng route group). |
| **PII** | Giữ redact trong `Administrator` view; không log body request vào Mongo (đã tùy middleware — rà lại). |
| **Audit của audit** | Khi đọc log nhạy cảm, có thể ghi thêm **authorization audit** (optional phase 2). |
| **Config** | `Observability:UserActionLogging` — bật Mongo TTL, max page size production. |

---

## 9. Kiểm thử

| Loại | Nội dung |
|------|----------|
| **Unit** | Handler: có/không permission; `viewType` Developer bị chặn. |
| **Integration** | Gọi API với user chỉ có `read` — Developer → 403; Administrator → 200. |
| **E2E (optional)** | Playwright: user không có quyền không thấy menu logs. |

---

## 10. Lộ trình triển khai (thứ tự khuyến nghị)

| Phase | Nội dung | Độ ưu tiên |
|-------|----------|------------|
| **P0** | Migration permission + `PermissionAuthorizationHandler` + siết `UserActionLogsController` + `viewType` enforcement | Phát hành ngay |
| **P1** | Tách policy `AuthorizationAdminController`; endpoint `me/permissions`; FE guards + menu | Cùng sprint |
| **P2** | Thu hẹp `AdminGuard` trên identity routes; JWT claims optional; wildcard `admin.system.manage` deprecation notice | Sprint sau |
| **P3** | Role seed `ObservabilityViewer`; audit meta-logging | Theo nhu cầu |

---

## 11. Definition of Done

- [ ] Không còn endpoint observability chỉ `[Authorize]` chung chung.
- [ ] `viewType=Developer` bị chặn nếu thiếu `observability.user-actions.read-sensitive`.
- [ ] FE không dựa vào role `Admin` đơn thuần cho logs/toggles/audit.
- [ ] Tài liệu API và migration đã ghi.
- [ ] Kiểm thử tự động tối thiểu cho handler + một integration test.

---

## 12. Tham chiếu code hiện tại

- `src/UniHub.API/Controllers/UserActionLogsController.cs`
- `src/Modules/Identity/UniHub.Identity.Presentation/Controllers/AuthorizationAdminController.cs`
- `src/Modules/Identity/UniHub.Identity.Infrastructure/Authorization/PermissionChecker.cs`
- `src/Modules/Identity/UniHub.Identity.Infrastructure/Authentication/JwtService.cs`
- `src/Shared/UniHub.Infrastructure/Persistence/Seeding/IdentitySeed.cs`
- `frontend/src/app/guards/AdminGuard.tsx`
- `frontend/src/features/admin/observability/*`

---

## 13. Liên quan task khác

- Cập nhật **`docs/tasks/fe-17-admin-logs.md`** (endpoint thực tế đã khác prefix).
- Không trộn với **forum moderation scope** (`docs/plans/2026-05-03-forum-moderation-scope-implementation.md`) — có thể làm song song hai nhánh.

---

**Branch gợi ý:** `feature/observability-permission-based`
