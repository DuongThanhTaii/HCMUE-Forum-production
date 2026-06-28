# Phase A — Messenger scroll UX — Implementation Plan

> **For agentic workers:** REQUIRED: implement task-by-task; tick checkboxes khi xong.

**Goal:** Auto-scroll xuống đáy khi vào thread và khi có tin mới (nếu user đang ở đáy); nút ↓ + đếm tin chưa đọc trong viewport; load tin cũ khi scroll lên.

**Architecture:** Hook `useChatScrollAnchor` quản lý ref container, `atBottom`, `pendingNewCount`, prepend pagination giữ anchor. `ChatMessageList` tách khỏi `ChatThread`, dùng chung cho `ChatPage` + `ChatDock`.

**Tech Stack:** React 19, RTK Query (`useGetMessagesQuery`), SignalR cache invalidation hiện có.

**Spec:** [2026-05-16-chat-messenger-parity-design.md](../../specs/2026-05-16-chat-messenger-parity-design.md)

---

## Files

| Action | Path |
|--------|------|
| Create | `frontend/src/features/chat/hooks/useChatScrollAnchor.ts` |
| Create | `frontend/src/features/chat/components/ChatMessageList.tsx` |
| Create | `frontend/src/features/chat/components/ScrollToBottomFab.tsx` |
| Create | `frontend/src/features/chat/lib/scrollConstants.ts` |
| Create | `frontend/src/features/chat/hooks/useChatScrollAnchor.test.ts` |
| Modify | `frontend/src/features/chat/components/ChatThread.tsx` |
| Modify | `frontend/src/features/chat/api/chat.api.ts` (optional `merge` pagination) |
| Modify | `frontend/src/shared/i18n/locales/vi/chat.json`, `en/chat.json` |

---

## Tasks

### Task 1: Hằng số & types

- [ ] Tạo `scrollConstants.ts`: `BOTTOM_THRESHOLD_PX = 48`, `LOAD_MORE_TOP_PX = 80`
- [ ] Export type `ScrollAnchorState = { atBottom: boolean; pendingNewCount: number }`

### Task 2: Hook `useChatScrollAnchor`

- [ ] Ref `containerRef`, state `atBottom`, `pendingNewCount`, `isLoadingOlder`
- [ ] `checkAtBottom()` on scroll (passive listener)
- [ ] `scrollToBottom({ smooth })` — `container.scrollTop = container.scrollHeight`
- [ ] `onMessagesChanged(length, isPrepend)`:
  - prepend → restore `scrollTop += newHeight - oldHeight`
  - append + was atBottom → scrollToBottom
  - append + !atBottom → `pendingNewCount++`
- [ ] `onUserSentMessage()` → always scrollToBottom + clear pending
- [ ] FAB click → scrollToBottom + clear pending

### Task 3: Load older messages

- [ ] RTK: hỗ trợ tăng `page` hoặc `fetchOlder(page+1)` merge vào đầu mảng (sort `sentAt` asc)
- [ ] Khi `scrollTop < LOAD_MORE_TOP_PX` và `page < totalPages` → gọi fetch, `isLoadingOlder` guard debounce 300ms
- [ ] Không load khi đang `isLoading` lần đầu

### Task 4: `ChatMessageList` + `ScrollToBottomFab`

- [ ] List render `MessageBubble` như `ChatThread` hiện tại
- [ ] FAB fixed trong panel thread: icon ↓ + badge số nếu `pendingNewCount > 0`
- [ ] `cursor-pointer`, `aria-label` từ i18n `chat.scroll.toBottom`, `chat.scroll.newMessages`

### Task 5: Tích hợp `ChatThread` + Dock

- [ ] `ChatThread` bọc `ChatMessageList` + hook; truyền `messages`, `conversationId`
- [ ] `useEffect` khi `conversationId` đổi → scrollToBottom sau paint (`requestAnimationFrame` double)
- [ ] Hub/RTK: subscribe message list length trong thread active → gọi `onMessagesChanged`

### Task 6: i18n & a11y

- [ ] `chat.scroll.newMessages`: "{{count}} tin mới" / "{{count}} new messages"
- [ ] `chat.scroll.loadingOlder`: "Đang tải tin cũ…"
- [ ] Respect `prefers-reduced-motion`: tắt smooth scroll

### Task 7: Tests

- [ ] Unit: `checkAtBottom` math với mock dimensions
- [ ] Unit: pending count tăng khi append và !atBottom
- [ ] Manual: mở `/chat`, gửi 3 tin, hub từ tab khác → badge ↓ xuất hiện

---

## Phase A — Done checklist

- [ ] Vào thread có tin → viewport ở tin mới nhất
- [ ] Kéo lên đọc cũ → tin mới không kéo mất vị trí; có FAB
- [ ] Click FAB → xuống đáy
- [ ] Kéo lên đầu → load thêm trang (nếu có)
- [ ] Dock và full page hành vi giống nhau
