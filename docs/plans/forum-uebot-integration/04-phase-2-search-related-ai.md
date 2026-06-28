# Pha 2 — Tìm kiếm, bài liên quan, AI

Mục tiêu: **tra cứu nhanh trên UEBot**, mở đúng bài trên Forum; tùy chọn **gợi ý bài liên quan** và **tóm tắt** qua API Forum.

## 2.1 Tìm kiếm / danh sách bài (đã có)

**Full-text search:** **`GET /api/v1/search?q=`** (anonymous) — query `q` (required), `categoryId`, `postType`, `tags`, pagination — xem [forum-fe-api.md](../../api/forum-fe-api.md) section Search.

**Danh sách / lọc:** **`GET /api/v1/posts`**

| Query (tiêu biểu) | Ghi chú |
|-------------------|---------|
| `pageNumber`, `pageSize` | Bắt buộc giới hạn cho mobile/desktop |
| `categoryId` | Lọc chuyên mục |
| `type` | Loại bài (theo domain Forum) |
| `status` | Lọc trạng thái (public thường dùng published) |

Tham chi tiết: [docs/api/forum-fe-api.md](../../api/forum-fe-api.md).

**UEBot:**

- Ưu tiên **`GET /api/v1/search`** cho ô tìm kiếm (đã có backend).

> Không cần thêm `q` vào `GET /api/v1/posts` trừ khi product muốn một contract khác.

## 2.2 Deep link từ kết quả

Mỗi item list/detail cần **URL ổn định**:

- Dùng `slug` + `id` từ `PostResponse` để dựng URL đúng route SPA (`ForumDetailPage`).

## 2.3 Bài liên quan (mở rộng)

**Hiện trạng:** đã có **`POST /api/v1/assistant/tools/related-posts`** (JWT) — xem [assistant-tools-fe-api.md](../../api/assistant-tools-fe-api.md).

**Hướng MVP (không AI):**

1. `GET /api/v1/posts?categoryId={same}&pageSize=5` sort mới nhất, exclude `id` hiện tại.
2. Hoặc `GET /api/v1/posts/{id}/related` (cần implement): trả về 3–5 `PostSummaryDto`.

**Hướng nâng cao:** embedding / tag overlap — sau MVP.

## 2.4 AI — tóm tắt / chat

Lỗi **"AI service is temporarily unavailable"** đến từ **không có provider LLM** (`IAIProviderFactory`), không phải lỗi exchange UEBot.

**Forum:**

- Merge cấu hình từ [appsettings.AI.example.json](../../../src/UniHub.API/appsettings.AI.example.json) vào môi trường dev.
- Endpoint assistant tools (related posts, summarize, …): xem [docs/api/assistant-tools-fe-api.md](../../api/assistant-tools-fe-api.md).
- Bật ít nhất một provider (Groq / Gemini / OpenRouter) + API key an toàn.

**UEBot:**

- Gọi đúng endpoint AI đã document trong [docs/api/ai-fe-api.md](../../api/ai-fe-api.md) (nếu public cho authenticated user).
- Không gửi secret model key xuống client.

## 2.5 Tiêu chí hoàn thành Pha 2

- [ ] Từ UEBot tìm được bài và mở đúng trang Forum trong browser.
- [ ] (Optional) Endpoint related hoặc workaround tag/category hoạt động.
- [ ] (Optional) Tóm tắt hoạt động sau khi cấu hình `AIProviders`.
