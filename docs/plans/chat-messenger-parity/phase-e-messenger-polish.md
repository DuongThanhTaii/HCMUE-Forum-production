# Phase E — Messenger polish — Implementation Plan

> **For agentic workers:** Implement cuối; dùng drawer Phase C + scroll Phase A.

**Goal:** Reactions, read receipts UI, reply-to, menu tin (copy/delete/edit), trạng thái gửi, mark read khi xem, info panel hoàn chỉnh.

**Architecture:** Chủ yếu FE trên API/hub có sẵn. Batch mark read khi scroll chạm đáy. ReactionPicker gọi REST + hub events.

**Depends on:** A, C (drawer), khuyến nghị B, D xong

---

## Đã có sẵn (tận dụng)

- REST: reactions add/remove, mark read, read-receipts GET, PATCH message, DELETE message
- Hub: `ReactionAdded`, `ReactionRemoved`, `MessageRead`, `MessageEdited`, `MessageDeleted`
- FE: `MessageBubble` cơ bản, `reactions` trên DTO

---

## Frontend tasks

| Action | Path |
|--------|------|
| Create | `ReactionPicker.tsx` (emoji subset, không emoji icons UI) |
| Create | `MessageActionsMenu.tsx` |
| Create | `ReplyPreviewBar.tsx` |
| Create | `ReadReceiptIndicator.tsx` |
| Modify | `MessageBubble.tsx`, `ChatComposer.tsx`, `ChatContext.tsx` |
| Modify | `chatHub.ts` (đảm bảo invalidate/update cache reactions/read) |

### E1 — Reactions

- [ ] Hover/long-press bubble → picker 6–8 emoji phổ biến
- [ ] Toggle reaction (POST/DELETE) + optimistic UI
- [ ] Hiển thị reactions dưới bubble (count + emoji row)
- [ ] Hub sync multi-tab

### E2 — Read receipts

- [ ] Tin cuối của mình (DM): "Đã xem" / avatar nhỏ khi `readReceipts` có peer
- [ ] Khi scroll đáy: `MarkMessageAsRead` cho tin visible chưa read (debounce, batch id)
- [ ] Hub `MessageRead` cập nhật UI

### E3 — Reply

- [ ] Menu "Trả lời" → `replyToMessageId` trong composer + preview bar
- [ ] Bubble hiển thị quote 1–2 dòng tin gốc (fetch lazy nếu cần)
- [ ] Gửi qua `SendMessage` payload có `replyToMessageId`

### E4 — Message actions

- [ ] Copy text
- [ ] Edit (chỉ tin mình, trong 15 phút — match BE rule nếu có)
- [ ] Delete (soft) — confirm
- [ ] Report → link Phase D

### E5 — Delivery status (own messages)

- [ ] Icon: sending (outbox), sent, failed (retry)
- [ ] Tích hợp `processOutbox` state per optimistic id

### E6 — Info panel hoàn chỉnh

- [ ] Mở từ C drawer: tên peer, participants (group), shortcuts media
- [ ] Link mute/block từ D

### E7 — i18n & tests

- [ ] Keys: `chat.reactions.*`, `chat.read.*`, `chat.reply.*`, `chat.message.*`
- [ ] Unit: ReplyPreviewBar, read receipt formatter
- [ ] Manual checklist DM 2 user

---

## Phase E — Done checklist

- [ ] Thả reaction → peer thấy realtime
- [ ] Đã xem hiện trên tin cuối DM
- [ ] Reply quote đúng tin
- [ ] Copy/edit/delete hoạt động
- [ ] Outbox failed → nút thử lại
