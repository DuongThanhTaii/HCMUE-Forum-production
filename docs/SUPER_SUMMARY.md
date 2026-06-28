# 🚀 UNIHUB SUPER SUMMARY (DOMAIN-FIRST)

> Last Updated: 2026-03-20  
> Scope: Tổng quan toàn project theo domain + trạng thái triển khai thực tế trong code

---

## 1) TL;DR

UniHub là nền tảng cộng đồng đại học cho HCMUE, thiết kế theo **DDD + Modular Monolith + CQRS**.

- Backend nền tảng đã khá cứng: **Identity, Forum, Learning, Chat, Career, Notification** đã hoàn thiện phase cốt lõi.
- **AI module** đã hoàn thành logic provider/feature chính, còn thiếu lớp API endpoints.
- **Frontend** hiện mới ở mức scaffold (skeleton), chưa vào implementation theo business flows.
- Tiến độ theo status hiện tại: **99/131 tasks (~75.6%)**.

---

## 2) Product Vision & Core Value

UniHub tập trung vào 1 hệ sinh thái thống nhất cho trường đại học:

- **Trao đổi học thuật & cộng đồng**: Forum, Q&A, thảo luận.
- **Học liệu & workflow duyệt tài liệu**: Learning Hub.
- **Nghề nghiệp sinh viên**: Career Hub.
- **Kết nối real-time**: Chat 1:1, nhóm, channel.
- **Thông báo đa kênh**: Push/Email/In-app.
- **Trợ lý AI**: chatbot, moderation, summarization, smart search.

---

## 3) Technical Architecture Snapshot

### Kiến trúc chính

- **Backend**: .NET 10, ASP.NET Core, MediatR, FluentValidation, Serilog, SignalR.
- **Data**: PostgreSQL + MongoDB + Redis.
- **Style**: Modular Monolith với 4 layer/module:
  - `Presentation` (Controllers/DTO)
  - `Application` (Commands/Queries/Handlers)
  - `Domain` (Entities/Aggregates/VO/Events)
  - `Infrastructure` (Repositories/External integrations)

### Strategic DDD Map

- **Core domains**: Identity, Forum, Learning, Chat
- **Supporting domains**: Career, Notification
- **Generic domain**: AI

### Integration style giữa domains

- **Identity** đóng vai trò upstream nền tảng (User/Role/Permission).
- **Notification** tiêu thụ domain events từ nhiều module.
- **AI** nhận dữ liệu qua ACL-style boundary từ Forum/Learning/Chat.

---

## 4) Domain-by-Domain Summary

## 🔐 Identity (Core)

**Mục tiêu domain**
- Xác thực, phân quyền, session/token, official badge.

**Tính năng chính**
- Register/Login, JWT + Refresh token flow.
- Dynamic roles & permissions (RBAC).
- Official account/badge, user/role/permission management.

**Dấu hiệu implement trong code**
- Controllers hiện có: `AuthController`, `UsersController`, `RolesController`, `PermissionsController`.

**Trạng thái**
- Phase 3: **DONE (12/12)**.

---

## 📝 Forum (Core)

**Mục tiêu domain**
- Post/Comment/Vote/Tag/Category + moderation/reporting.

**Tính năng chính**
- CRUD posts/comments, vote system.
- Tag/search/bookmark/report.
- Feed và filter theo category/tag.

**Dấu hiệu implement trong code**
- Controllers hiện có: `PostsController`, `CommentsController`, `TagsController`, `SearchController`.

**Trạng thái**
- Phase 4: **DONE (12/12)**.

---

## 📚 Learning (Core)

**Mục tiêu domain**
- Quản lý học liệu và luồng duyệt tài liệu theo workflow.

**Tính năng chính**
- Upload/search/download tracking.
- Approval workflow (event sourcing cho approval flow).
- Course/faculty/document rating/moderator assignment.

**Dấu hiệu implement trong code**
- Controllers hiện có: `DocumentsController`, `CoursesController`, `FacultiesController`.

**Trạng thái**
- Phase 5: **DONE (12/12)**.

---

## 💬 Chat (Core)

**Mục tiêu domain**
- Giao tiếp real-time cho sinh viên/nhóm/lớp/cộng đồng.

**Tính năng chính**
- Conversations/channels/messages.
- SignalR real-time + Redis backplane.
- File/reaction/moderation primitives.

**Dấu hiệu implement trong code**
- Controllers hiện có: `ConversationsController`, `ChannelsController`, `MessagesController`.
- `Program.cs` đã map hub chat.

**Trạng thái**
- Phase 6: **DONE (12/12)**.

---

## 💼 Career (Supporting)

**Mục tiêu domain**
- Job lifecycle: đăng tuyển, ứng tuyển, recruiter workflows.

**Tính năng chính**
- Company profile & recruiter management.
- Job posting/search/matching.
- Application submission/status flow.

**Dấu hiệu implement trong code**
- Controllers hiện có: `CompaniesController`, `JobPostingsController`, `ApplicationsController`, `RecruitersController`.

**Trạng thái**
- Phase 7: **DONE (12/12)**.

---

## 🔔 Notification (Supporting)

**Mục tiêu domain**
- Unified notification orchestration từ cross-domain events.

**Tính năng chính**
- In-app + Email + Web Push.
- Notification templates & preferences.
- Event handlers từ Forum/Learning/Chat/Career/Identity.

**Dấu hiệu implement trong code**
- Controller hiện có: `NotificationsController`.

**Trạng thái**
- Phase 8: **DONE (8/8)**.

---

## 🤖 AI (Generic)

**Mục tiêu domain**
- Trợ lý AI và lớp intelligent features cho hệ thống.

**Tính năng chính (đã có ở service layer)**
- Provider abstraction + provider rotation (Groq/Gemini/OpenRouter).
- UniBot FAQ/chat logic.
- Content moderation.
- Document summarization.
- Smart search.

**Dấu hiệu implement trong code**
- `UniHub.AI.Application`: interfaces/DTO/services contracts đã có.
- `UniHub.AI.Infrastructure`: provider implementations/config đã có.
- `UniHub.AI.Presentation`: hiện **chưa có controller business**, mới có `Class1.cs` placeholder.
- `Program.cs`: chưa add AI controller application part.

**Trạng thái**
- Phase 9: **IN_PROGRESS (6/7)**.
- Task chưa xong: **`TASK-100 AI API Endpoints`**.

---

## 5) Frontend Snapshot

Theo `phase-10.md`, FE target là full app (auth/main/admin routes, Zustand, Query, forms, PWA).

**Thực tế hiện tại**
- `frontend/src/app/page.tsx` vẫn là template mặc định Next.js.
- Chưa có structure feature routes theo domain như kế hoạch.
- Phase 10 vẫn **0/14**.

---

## 6) Execution Status (Project-level)

Theo `docs/tasks/STATUS.md`:

- **Hoàn thành 100%**: Phase 0 → Phase 8
- **Đang làm**: Phase 9 (AI) = 6/7
- **Chưa bắt đầu**: Phase 10, 11, 12
- **Tổng**: `99/131`, còn `32` tasks

---

## 7) Current Strengths

- Kiến trúc backend rõ ràng, domain boundaries tách tốt.
- CQRS + Result pattern nhất quán trong application layer.
- Module coverage rộng, test footprint lớn ở backend.
- Có context map/domain docs khá đầy đủ để scale team.

---

## 8) Main Gaps / Bottlenecks

- **Gap #1 (blocking AI completion)**: thiếu AI presentation endpoints (`TASK-100`).
- **Gap #2 (blocking product delivery)**: frontend chưa vào domain implementation.
- **Gap #3 (go-live readiness)**: phase testing optimization + deployment chưa bắt đầu.

---

## 9) Suggested Next Direction Options (để bạn chọn)

### Option A — “Close Backend First”
1. Hoàn tất `TASK-100` (AI API endpoints).
2. Chuẩn hóa API contract cho AI + swagger.
3. Chốt backend phase 9 = 7/7 trước khi FE build mạnh.

### Option B — “Frontend-First Delivery”
1. Dựng FE skeleton theo `phase-10` (auth/main/admin route groups).
2. Kết nối lần lượt 3 flows có value cao: Auth → Forum feed → Chat basic.
3. Song song chỉ làm AI API tối thiểu để unblock UX.

### Option C — “Vertical Slice MVP”
1. Chọn 1 slice hoàn chỉnh end-to-end (vd: Forum + Auth + Notification).
2. Làm xong cả BE endpoint + FE UI + test + deploy cho slice đó.
3. Lặp cho Learning/Career/AI.

---

## 10) One-page Call-to-Action

Nếu mục tiêu là ra bản usable nhanh nhất cho user thật:

- **Ngắn hạn (1-2 tuần)**: chọn Option C với slice `Auth + Forum + Chat basic`.
- **Song song**: finish `TASK-100` để khóa phase AI.
- **Sau đó**: mở rộng slice theo Learning/Career.

---

## Appendix: Key Source Files Used

- `README.md`
- `docs/ARCHITECTURE.md`
- `docs/domain/BOUNDED_CONTEXTS.md`
- `docs/domain/CONTEXT_MAP.md`
- `docs/tasks/STATUS.md`
- `docs/tasks/phase-9.md`
- `docs/tasks/phase-10.md`
- `src/UniHub.API/Program.cs`
- `frontend/src/app/page.tsx`
- `src/Modules/AI/UniHub.AI.Presentation/Class1.cs`
