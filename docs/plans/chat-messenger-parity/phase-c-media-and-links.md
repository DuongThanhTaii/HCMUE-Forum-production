# Phase C — Shared media & links — Implementation Plan

> **For agentic workers:** Implement sau Phase B (tái dùng filter search).

**Goal:** Trong cuộc trò chuyện, xem grid ảnh, danh sách file, danh sách link đã gửi — giống Messenger "Media, files, links".

**Architecture:** API list attachments theo conversation + filter mime. Links: regex URL trong `content` (client) hoặc `filter=links` server (ưu tiên server nếu B đã có). UI: drawer/tab từ header thread.

**Depends on:** Phase A (layout), Phase B (search infra tùy chọn)

---

## Backend

`GET /api/v1/chat/conversations/{conversationId}/attachments`

| Query | Mô tả |
|-------|--------|
| `kind` | `image` \| `file` \| `voice` \| `all` |
| `page`, `pageSize` | |

**Response:** `messageId`, `sentAt`, `fileName`, `fileUrl`, `mimeType`, `thumbnailUrl`, `fileSize`

- Query join `messages` + unnest `message_attachments`
- Chỉ participant; sort `sentAt` desc

`GET .../messages/search?filter=links` (nếu chưa có từ B): match URL pattern trong content.

### Tasks BE

- [ ] Query + Handler + Controller
- [ ] Tests + `chat-fe-api.md`

---

## Frontend

| Action | Path |
|--------|------|
| Create | `ConversationInfoDrawer.tsx` |
| Create | `SharedMediaGrid.tsx`, `SharedFilesList.tsx`, `SharedLinksList.tsx` |
| Create | `frontend/src/features/chat/lib/extractLinks.ts` |
| Modify | `chat.api.ts` |

### Tasks FE

- [ ] Header thread: nút ℹ️ / avatar → mở drawer
- [ ] Tabs: **Ảnh** | **File** | **Link**
- [ ] Ảnh: grid thumbnail, click → lightbox (hoặc tab mới)
- [ ] File: icon + tên + size + ngày, click tải
- [ ] Link: domain + snippet, click mở `rel="noopener"`
- [ ] Empty states per tab
- [ ] i18n `chat.info.media`, `chat.info.files`, `chat.info.links`

### `extractLinks.ts`

- [ ] Regex URL http(s); dedupe; unit test vài chuỗi tiếng Việt + URL dính

---

## Phase C — Done checklist

- [ ] Thread có 5 ảnh → tab Media hiện đủ
- [ ] Voice/file hiện tab Files
- [ ] Tin có URL → tab Links
- [ ] Drawer hoạt động dock + full page
