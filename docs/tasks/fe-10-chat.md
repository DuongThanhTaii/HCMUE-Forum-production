# FE-10: Chat — DM, Groups, Channels (SignalR Real-time)

| Property | Value |
|---|---|
| **ID** | FE-10 |
| **Branch** | `feature/FE-10-chat` |
| **Commit** | `feat(fe/chat): implement real-time chat with DM, groups, and channels` |
| **Priority** | High |
| **Estimate** | 14h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03 |

---

## API + SignalR

**REST:**
| Action | Endpoint |
|---|---|
| Get conversations | GET `/api/v1/chat/conversations` |
| Create DM | POST `/api/v1/chat/conversations/direct` |
| Create Group | POST `/api/v1/chat/conversations/group` |
| Get messages | GET `/api/v1/chat/conversations/{id}/messages?page=` |
| Send message | POST `/api/v1/chat/conversations/{id}/messages` |
| Send with attachment | POST `/api/v1/chat/conversations/{id}/messages/attachments` |
| Mark as read | POST `/api/v1/chat/messages/{id}/read` |
| Add reaction | POST `/api/v1/chat/messages/{id}/reactions` |
| Get channels | GET `/api/v1/chat/channels` |
| Join channel | POST `/api/v1/chat/channels/{id}/join` |
| Leave channel | POST `/api/v1/chat/channels/{id}/leave` |
| Upload file | POST `/api/v1/chat/files` |

**SignalR Hub:** `wss://api/hubs/chat`

Events:
- `ReceiveMessage` → new message trong conversation
- `MessageRead` → read receipt update
- `ReactionAdded` / `ReactionRemoved`
- `UserTyping` → typing indicator

---

## Layout

```
┌────────────────────────────────────────────────┐
│  LEFT PANEL (280px)   │  CHAT WINDOW (flex-1)  │
│                       │                        │
│  [Search conversations│  [Conversation header] │
│  ────────────────     │  ─────────────────     │
│  DM                   │                        │
│  [User 1]      ● 2   │  [Messages...]         │
│  [User 2]      ● 1   │                        │
│  ────────────────     │  [Input bar]           │
│  Groups               │  [Attach] [Emoji] [→]  │
│  [Group A]            │                        │
│  [Group B]            │                        │
└───────────────────────┴────────────────────────┘
```

---

## Components

```
components/features/chat/
├── ConversationList.tsx     ← left panel: DMs + groups grouped
├── ConversationItem.tsx     ← single row with last msg + unread badge
├── ChatWindow.tsx           ← main chat area
├── MessageBubble.tsx        ← single message (own=right, other=left)
├── MessageInput.tsx         ← textarea + attachment + emoji + send
├── TypingIndicator.tsx      ← "User A đang nhập..."
├── ReactionPicker.tsx       ← emoji reaction picker (EmojiMart)
├── ReadReceiptAvatars.tsx   ← small avatars showing who read
├── FileAttachmentPreview.tsx← image/file preview in message
└── NewConversationModal.tsx ← user search to start DM/group
```

### `hooks/realtime/`

```
useChatHub.ts     ← SignalR connection + event handlers
```

```ts
// useChatHub.ts
export function useChatHub(conversationId: string) {
  useEffect(() => {
    const connection = new HubConnectionBuilder()
      .withUrl(`${SIGNALR_URL}/chat?access_token=${token}`)
      .withAutomaticReconnect()
      .build();

    connection.on('ReceiveMessage', (msg) => {
      queryClient.setQueryData(['messages', conversationId], addMessage(msg));
    });

    connection.start();
    return () => connection.stop();
  }, [conversationId]);
}
```

---

## `/chat/channels` — Channel Browser

Grid of public channels. Each card: name, description, member count, joined status.
- Join: `POST /api/v1/chat/channels/{id}/join`
- Leave: `POST /api/v1/chat/channels/{id}/leave`
- Joined channels hiện trong left panel dưới Groups

---

## Key Notes

- Messages infinite scroll ngược (load older messages khi scroll lên top)
- Message grouping: consecutive messages từ cùng user trong 5 phút → group lại, chỉ hiện avatar 1 lần
- File upload: `POST /api/v1/chat/files` → nhận URL → đính vào message
- Emoji picker: `@emoji-mart/react`

---

## Acceptance Criteria

- [ ] DM conversation list hiện
- [ ] Real-time message nhận qua SignalR
- [ ] Send text message
- [ ] Send file attachment
- [ ] Emoji reactions
- [ ] Read receipts hiện avatar
- [ ] Typing indicator
- [ ] Unread count badge trên conversation item
- [ ] Channel browser + join/leave
- [ ] Joined channels hiện trong left panel
- [ ] New DM modal: search user → start conversation
