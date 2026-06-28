# Plan: Phân quyền động — UI/UX Pro Max, Endpoint toggles & Xử lý Ghi đè quyền

**Date:** 2026-05-03  
**Mục tiêu:** Định hình UI/UX **production-ready** cho màn **Roles / Permissions** và **Observability → Endpoint toggles**. Áp dụng pattern *Data-Dense + Drill-Down* từ `ui-ux-pro-max` để tối ưu hóa không gian hiển thị lượng lớn dữ liệu phân quyền. Đồng bộ với **permission-based plan** (`docs/plans/2026-05-03-observability-permission-based-production.md`).

---

## A. Bản chất “Ghi đè quyền” (Permission Overrides) vs Endpoint Toggles

Cần phân biệt rõ ràng 2 khái niệm này trên UI để người quản trị không bị nhầm lẫn:

| Tiêu chí | Ghi đè quyền (Permission Overrides) | Bật/Tắt API (Endpoint Toggles) |
|---|---|---|
| **Phạm vi** | Cấp độ **User / Group / Role**. | Cấp độ **Toàn hệ thống (Global)**. |
| **Bản chất** | Cấp thêm (Grant) hoặc Tước bỏ (Deny) một quyền (VD: `forum.posts.delete`) của một đối tượng cụ thể. | Đóng băng/Mở khóa toàn bộ một endpoint API (VD: `/api/v1/learning/courses`). |
| **Thứ tự ưu tiên** | **User Override → Group Override → Role Permission**. User Override luôn có quyền quyết định cuối cùng. | Tắt API sẽ **chặn tất cả request**, bất kể User/Role có permission hay không. |
| **Vị trí UI** | `/admin/overrides/users`, `/admin/overrides/groups`, `/admin/roles` | `/admin/system/toggles` (Yêu cầu quyền `observability.endpoint-toggles.manage`) |

> [!NOTE]
> PermissionChecker phía Backend luôn đánh giá theo thứ tự: Override -> Role cache. Do đó, đây là cơ chế "Ghi đè động".

---

## B. Bug 500 `DbUpdateConcurrencyException` khi `POST /roles/{id}/permissions`

**Nguyên nhân:**
Hàm `RoleRepository.GetByIdAsync` **không load** relationship `Role.Permissions` (thiếu `.Include()`). Do đó, khi BE kiểm tra, nó lầm tưởng Role chưa có quyền nào và tiếp tục thực hiện lệnh **assign**. Khi EF Core tiến hành lưu thay đổi, sự xung đột dữ liệu sinh ra lỗi `DbUpdateConcurrencyException`. Đồng thời cache cũng lưu sai danh sách rỗng.

**Cách khắc phục:**
1. Thêm `.Include(r => r.Permissions)` vào `GetByIdAsync` và `GetAllAsync`.
2. `GET /api/v1/roles/{id}` phải trả về chi tiết mảng **`permissions[]`** để FE map đúng trạng thái của các checkbox.

**Hướng Retest:**
- Truy cập UI sửa Role Moderator.
- Check/Uncheck một số quyền và Lưu. Không còn lỗi 500.
- Xóa cache/đăng xuất rồi vào lại, kiểm tra danh sách quyền thực tế có được áp dụng đúng không.

---

## C. UX/UI Design System (Áp dụng UI/UX Pro Max)

Theo phân tích từ `ui-ux-pro-max` đối với từ khóa `admin dashboard rbac settings data-heavy`:

- **Pattern:** Data-Dense + Drill-Down (Nhiều dữ liệu, cho phép đào sâu).
- **Style:** Minimal padding, grid layout, maximum data visibility. Không gian làm việc tiết kiệm nhưng dễ nhìn.
- **Màu sắc:** Xanh Primary (`#1E40AF`) cho tín hiệu an toàn, Cam/Amber (`#F59E0B`) cho các cảnh báo / overriding (nguy cơ cao).
- **Typography:** Fira Sans cho văn bản chung (để dễ đọc), Fira Code cho các key/mã kỹ thuật (endpoint keys, permission codes).
- **Hiệu ứng:** Hover tooltips, highlight dòng khi hover, filter animation mượt mà.

---

## D. Kế hoạch UI: Màn hình Phân Quyền (Dynamic RBAC)

Vì số lượng quyền rất lớn, UI không thể là một danh sách dài thò lò ("trôi AI").

### 1. Phân nhóm (Grouping) & Accordion
- Parse chuỗi `permission.code` (VD: `forum.posts.create`) hoặc từ module BE trả về để nhóm theo **Cấp 1 (Module)**: `Forum`, `Chat`, `Learning`, `Identity`.
- Sử dụng **Accordion (Collapsible)**: Mặc định đóng (collapsed) để người dùng có cái nhìn tổng quan.
- **Chỉ báo trên tiêu đề Accordion:** Hiển thị badge số lượng quyền đã bật trên tổng số. VD: `Forum (3/12)`.
- Khi expand, danh sách các quyền sẽ hiện ra dưới dạng Grid Data-Dense.

### 2. Trạng thái Checkbox & Tooltip
- Nguồn dữ liệu từ mảng `permissions[]` trong `GET /roles/{id}`.
- Có tooltip giải thích ngắn cho từng permission.
- Sử dụng phông chữ **Fira Code** cho mã quyền (VD: `forum.reports.review`) để mang lại cảm giác kỹ thuật, rõ ràng.
- Với **System Role** (Role hệ thống), toàn bộ checkbox ở trạng thái **Disabled** (Khóa cứng không cho sửa).

---

## E. Kế hoạch UI: Màn hình Endpoint Toggles

Chỉ những User có permission `observability.endpoint-toggles.read` mới thấy màn này.

### 1. Danh sách Toggles
- Fetch từ API `GET /api/v1/admin/authorization/toggles`.
- **Nhóm theo Prefix/Module:** Tự động cắt segment đầu tiên của `endpointKey` (VD: `UniHub.Forum`) để gộp nhóm.
- Tiêu đề nhóm hiển thị số lượng: `Đang bật: 15/15`.

### 2. Dòng thao tác (Row)
- Mỗi endpoint là một hàng có **Switch toggle**.
- Key của endpoint dùng phông `Fira Code`, cắt bớt chữ (truncate) nếu quá dài và hiển thị Full Key trong Hover Tooltip.
- Khi người dùng bấm toggle chuyển từ On sang **Off (Tắt)**:
  - Hiển thị **Confirm Modal** cảnh báo nguy cơ: *"Bạn có chắc chắn muốn TẮT API này? Toàn bộ hệ thống sẽ không thể gọi đến endpoint này."*
  - Modal sử dụng màu Amber (`#F59E0B`) để báo động risk.
- Xử lý Loading (spinner) riêng rẽ trên từng hàng khi gọi lệnh `PUT`.

---

## F. Definition of Done (DoD)

- [ ] Lỗi 500 khi gán Role được fix dứt điểm. Trả về đúng `permissions[]`.
- [ ] Giao diện Permission được nhóm theo Accordion, hiển thị dạng Grid tối ưu không gian (Data-Dense).
- [ ] Có giải thích ngắn gọn, rõ ràng về "Ghi đè quyền".
- [ ] Màn Endpoint Toggles có chia nhóm theo prefix.
- [ ] Bật/tắt endpoint có Modal confirm chặn thao tác nhầm lẫn.
- [ ] Phân quyền bảo vệ màn hình Toggles dựa trên bảng kế hoạch `observability-permission-based-production.md`.
