# Phase B — In-conversation search — Implementation Plan

> **For agentic workers:** Implement sau Phase A.

**Goal:** Tìm tin nhắn trong một cuộc trò chuyện (text), nhảy tới vị trí tin; keyboard ↑↓ giữa kết quả.

**Architecture:** Endpoint search phía server (paginated). FE: panel search trong header thread, highlight snippet, `scrollIntoView` message id (tích hợp hook scroll A).

**Tech Stack:** .NET MediatR query, EF Core, React RTK Query.

**Depends on:** Phase A (`scrollToMessageId` helper)

---

## Backend

| Action | Path |
|--------|------|
| Create | `UniHub.Chat.Application/Queries/SearchConversationMessages/` |
| Modify | `UniHub.Chat.Presentation/Controllers/MessagesController.cs` hoặc `ConversationsController` |
| Create | Migration index (optional) `chat.messages (conversation_id, sent_at)` + GIN content |

### API (đề xuất)

`GET /api/v1/chat/conversations/{conversationId}/messages/search`

| Query | Mô tả |
|-------|--------|
| `q` | Chuỗi tìm (min 2 ký tự) |
| `filter` | `all` \| `text` \| `media` \| `links` (media/links có thể phase C tái dùng) |
| `page`, `pageSize` | Mặc định 20 |

**Response item:** `messageId`, `sentAt`, `snippet` (highlight), `senderId`, `senderDisplayName`

**Rules:**

- Chỉ participant mới search được
- Không trả tin `isDeleted` (hoặc masked)
- `ILIKE '%' || q || '%'` trên `content` cho MVP; index sau nếu chậm

### Tasks BE

- [ ] `SearchConversationMessagesQuery` + Handler + Validator
- [ ] Controller action + `ApiResponse`
- [ ] Unit test handler (participant, empty q, pagination)
- [ ] Cập nhật `docs/api/chat-fe-api.md`

---

## Frontend

| Action | Path |
|--------|------|
| Create | `frontend/src/features/chat/components/ConversationSearchPanel.tsx` |
| Create | `frontend/src/features/chat/hooks/useConversationSearch.ts` |
| Modify | `frontend/src/features/chat/api/chat.api.ts` |
| Modify | `frontend/src/features/chat/components/ChatPage.tsx` (header nút search) |
| Modify | `ChatDock` thread header nếu có |

### Tasks FE

- [ ] RTK endpoint `searchConversationMessages`
- [ ] UI: icon 🔍 mở panel (slide-over hoặc popover full-width mobile)
- [ ] Input debounce 300ms, min 2 chars
- [ ] List kết quả; click → đóng panel, `scrollToMessageId(id)` (fetch thêm pages nếu tin chưa loaded)
- [ ] Highlight tạm message 2s (`ring-2 ring-primary`)
- [ ] Keyboard: Enter mở kết quả đầu; ↑↓ chọn
- [ ] i18n: `chat.search.placeholder`, `chat.search.noResults`, `chat.search.minChars`

---

## Phase B — Done checklist

- [ ] Tìm "EF Core" trong thread 100+ tin → thấy kết quả đúng trang
- [ ] Click kết quả → scroll tới bubble đúng
- [ ] User không trong conversation → 403
- [ ] Mobile: panel search usable 375px
