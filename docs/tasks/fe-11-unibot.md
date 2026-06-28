# FE-11: UniBot AI Chat

| Property | Value |
|---|---|
| **ID** | FE-11 |
| **Branch** | `feature/FE-11-unibot` |
| **Commit** | `feat(fe/ai): implement UniBot AI chat interface` |
| **Priority** | Medium |
| **Estimate** | 6h |
| **Status** | ⬜ NOT_STARTED |
| **Depends on** | FE-03 |

---

## API Endpoints

| Action | Endpoint |
|---|---|
| Send message to bot | POST `/api/v1/ai/chat` body: `{ message, conversationId? }` |
| Get conversation history | GET `/api/v1/ai/conversations/{id}` |
| Smart search | POST `/api/v1/ai/search` body: `{ query }` |

---

## Layout `/chat/ai`

```
┌──────────────────────────────────────────────┐
│  UniBot                              [New +] │
│  ──────────────────────────────────────────  │
│                                              │
│        Xin chào! Tôi là UniBot 🎓           │
│        Tôi có thể giúp bạn:                 │
│        • Tìm tài liệu học tập               │
│        • Giải đáp thắc mắc về trường         │
│        • Hỗ trợ tìm việc làm               │
│                                              │
│  ──────────────────────────────────────────  │
│  [User message bubble - right aligned]       │
│  [Bot response bubble - left aligned]        │
│  [Typing indicator...]                       │
│  ──────────────────────────────────────────  │
│  [📎] [Nhập tin nhắn...]            [Send→] │
└──────────────────────────────────────────────┘
```

---

## Components

```
components/features/ai/
├── UniBotChat.tsx          ← full page chat interface
├── BotMessageBubble.tsx    ← bot message với typing animation
├── UserMessageBubble.tsx   ← user message
├── SuggestedPrompts.tsx    ← quick-tap suggestions khi chat trống
└── BotTypingIndicator.tsx  ← pulsing dots animation
```

### Suggested Prompts (initial state)

```tsx
const SUGGESTED_PROMPTS = [
  'Tìm tài liệu môn Phương pháp giảng dạy',
  'Có việc làm thực tập nào phù hợp không?',
  'Lịch đóng học phí học kỳ này là khi nào?',
  'Học bổng nào đang mở đăng ký?',
];
```

### Streaming Response

Nếu BE hỗ trợ SSE streaming: hiện từng token dần. Nếu không: hiện full response với fade-in animation.

---

## Acceptance Criteria

- [ ] Chat interface render đúng
- [ ] Send message → nhận response từ bot
- [ ] Typing indicator trong khi chờ response
- [ ] Suggested prompts hiện khi chat trống
- [ ] New conversation button clear chat
- [ ] Messages scroll to bottom tự động
- [ ] Link trong response bot có thể click (forum post, document, job)
