# Pha 4 — Soạn nháp & composer (optional / backlog)

Mục tiêu dài hạn: soạn bài nhanh trên UEBot, lưu **nháp** trên Forum, hoàn tất trên web nếu cần rich editor.

## 4.1 Luồng đề xuất

1. User soạn title + content text/markdown tối giản trên UEBot.
2. `POST /api/v1/posts` với **status draft** (nếu API/domain hỗ trợ) — kiểm tra `CreatePostRequest` và `PostStatus`.
3. User mở web để chỉnh format, đính kèm, rồi **publish** hoặc mod duyệt theo Pha 1.

## 4.2 Phụ thuộc sản phẩm

- Nếu luôn tạo **published** từ API — cần điều chỉnh policy (spam risk).
- Nếu chỉ cho phép draft — đảm bảo permission author và quota.

## 4.3 Tiêu chí (khi scope được duyệt)

- [ ] Tạo nháp từ UEBot xuất hiện trong `GET /api/v1/mod/posts` (draft inbox).
- [ ] Không bypass moderation rule của trường.
