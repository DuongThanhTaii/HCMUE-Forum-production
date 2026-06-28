# Phase D — Block, mute, report — Implementation Plan

> **For agentic workers:** Có thể song song Phase B sau khi Phase A xong.

**Goal:** Chặn người dùng (không DM được), tắt tiếng hội thoại, báo cáo tin nhắn vi phạm.

**Architecture:** Block thuộc **Identity** (quan hệ user-user). Mute + report thuộc **Chat** hoặc shared moderation. Enforce block ở `SendMessageCommandHandler` và tạo DM.

**Tech Stack:** EF migration, MediatR, FE thread overflow menu.

---

## Backend — Identity (Block)

| Action | Path |
|--------|------|
| Create | `UserBlock` entity, `BlockedUserId` / `BlockerUserId` |
| Create | `BlockUserCommand`, `UnblockUserCommand`, `GetBlockedUsersQuery` |
| Create | `Identity` controller `UsersController` hoặc `BlocksController` |
| Migration | `identity.user_blocks` |

**Rules:**

- Không block chính mình
- Block A→B: A không gửi DM tới B; B không gửi tới A (đối xứng cho DM)
- List blocked cho settings

### Tasks

- [ ] Domain + configuration + migration
- [ ] `IUserBlockRepository`
- [ ] API: `POST /api/v1/identity/users/{userId}/block`, `DELETE` unblock, `GET /api/v1/identity/users/me/blocked`
- [ ] Tests

---

## Backend — Chat (enforce + mute + report)

| Action | Path |
|--------|------|
| Modify | `SendMessageCommandHandler`, `CreateDirectConversationCommandHandler` |
| Create | `MuteConversationCommand`, `ReportMessageCommand` |
| Create | `conversation_participant.is_muted` hoặc bảng `conversation_settings` |

**Report:** `messageId`, `reason` enum, `description?` → tạo record moderation (có thể email mod sau).

### Tasks

- [ ] Check block trước send → `403` `Chat.UserBlocked`
- [ ] `POST /api/v1/chat/conversations/{id}/mute` body `{ muted: true }`
- [ ] `POST /api/v1/chat/messages/{id}/report`
- [ ] Hub: không push tới user đã mute thread (optional v1: chỉ ẩn badge FE)
- [ ] Tests send blocked, report

---

## Frontend

| Action | Path |
|--------|------|
| Create | `ConversationHeaderMenu.tsx` |
| Modify | `chat.api.ts`, identity API nếu cần |
| Modify | `ChatPage`, `ChatComposer` (disable khi blocked) |

### Tasks FE

- [ ] Menu ⋮: Tắt tiếng, Chặn (DM), Báo cáo, (E sau: Xem profile)
- [ ] Confirm dialog chặn — i18n rõ hậu quả
- [ ] Sau block: quay list, toast
- [ ] Mute: icon trên conversation list
- [ ] Report modal: lý do + mô tả
- [ ] Hiển thị banner "Bạn đã chặn người này" — composer disabled

---

## Phase D — Done checklist

- [ ] A block B → B không gửi tin cho A (API 403)
- [ ] Unblock → gửi lại được
- [ ] Mute → không popup notification thread đó
- [ ] Report → 201 + mod có record (hoặc log)
