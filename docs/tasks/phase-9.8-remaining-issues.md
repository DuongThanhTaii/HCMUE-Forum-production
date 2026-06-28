# Phase 9.8 — Remaining Issues Before Frontend

> **Date**: 2026-02-09  
> **Status**: Pending  
> **Backend Readiness**: ~95% (137 endpoints, 1,241 tests passing)

---

## Summary

Backend đã sẵn sàng cho frontend với 137 endpoints, 2 SignalR hubs, JWT auth, CORS, rate limiting, và ProblemDetails error handling. Tuy nhiên còn **5 vấn đề** cần lưu ý/fix.

---

## Issues

### 1. ⚠️ MEDIUM — JobPostingsController filter chưa hoạt động

**File**: `src/Modules/Career/UniHub.Career.Presentation/Controllers/JobPostingsController.cs`

**Mô tả**: Hai endpoint `GET /api/v1/jobs` và `GET /api/v1/jobs/search` nhận query params `JobType` và `ExperienceLevel` từ frontend nhưng **không parse** — giá trị luôn là `null` với comment `// TODO: Parse from string`.

**Ảnh hưởng**: Lọc job theo loại việc làm và mức kinh nghiệm sẽ không hoạt động.

**Fix**: Parse string thành enum `JobType` và `ExperienceLevel` bằng `Enum.TryParse()`.

---

### 2. ⚠️ MEDIUM — Error response không nhất quán

**Files**: Tất cả controllers trong `src/Modules/*/Presentation/Controllers/`

**Mô tả**: Có 2 format lỗi khác nhau:

- **Business errors** (từ `Result.Failure`): Controllers trả `new { error = result.Error.Message }` → JSON: `{ "error": "..." }`
- **Exceptions** (từ `GlobalExceptionHandler`): Trả RFC 7807 ProblemDetails → JSON: `{ "type": "...", "title": "...", "status": 400, "detail": "...", "traceId": "..." }`

**Ảnh hưởng**: Frontend phải handle 2 format lỗi khác nhau, tăng complexity.

**Fix**: Chuẩn hóa tất cả controller error responses sang ProblemDetails format bằng helper method trong `BaseApiController`.

---

### 3. ⚠️ LOW — Document download chưa trả file thực tế

**File**: `src/Modules/Learning/UniHub.Learning.Presentation/Controllers/DocumentsController.cs`

**Mô tả**: `POST /api/v1/documents/{id}/download` chỉ track download event và trả `{ message: "Document download tracked successfully" }`. Không có endpoint nào thực sự trả file bytes hoặc presigned URL cho frontend.

**Ảnh hưởng**: Chức năng download tài liệu chưa hoạt động thực tế.

**Fix**: Thêm endpoint trả `FileContentResult` hoặc redirect đến presigned URL từ storage (local/S3/Azure Blob).

---

### 4. 💡 INFO — Courses/Faculties GET thiếu explicit auth attribute

**Files**:

- `src/Modules/Learning/UniHub.Learning.Presentation/Controllers/CoursesController.cs`
- `src/Modules/Learning/UniHub.Learning.Presentation/Controllers/FacultiesController.cs`

**Mô tả**: Các GET endpoints không có `[Authorize]` hay `[AllowAnonymous]`. Vì `Program.cs` không set `FallbackPolicy`, chúng vẫn **public** — nhưng thiếu explicit attribute khiến intent không rõ ràng.

**Fix**: Thêm `[AllowAnonymous]` vào các GET endpoints cho rõ ràng.

---

### 5. 💡 INFO — DELETE endpoint dùng `[FromBody]`

**File**: `src/Modules/Learning/UniHub.Learning.Presentation/Controllers/CoursesController.cs`

**Mô tả**: `DELETE /api/v1/courses/{id}` nhận `[FromBody] DeleteCourseRequest`. HTTP DELETE với request body là non-standard — một số HTTP clients, proxies, hoặc API gateways có thể strip body.

**Ảnh hưởng**: Có thể gặp vấn đề với một số HTTP client (Axios cần config đặc biệt cho DELETE body).

**Fix**: Chuyển sang dùng route params hoặc query params thay vì body.

---

## Priority

| #   | Issue                          | Priority | Block Frontend?                                |
| --- | ------------------------------ | -------- | ---------------------------------------------- |
| 1   | JobType/ExperienceLevel filter | MEDIUM   | Không — có thể build UI trước, fix filter sau  |
| 2   | Error response format          | MEDIUM   | Không — frontend handle cả 2 format            |
| 3   | Document download              | LOW      | Partial — UI download button sẽ chưa hoạt động |
| 4   | Missing `[AllowAnonymous]`     | INFO     | Không                                          |
| 5   | DELETE with body               | INFO     | Không — Axios hỗ trợ nếu config đúng           |

---

## Conclusion

**Không có issue nào block frontend development.** Tất cả đều có thể fix song song khi build frontend. Recommend fix issue #1 và #2 trước vì ảnh hưởng đến UX nhiều nhất.
