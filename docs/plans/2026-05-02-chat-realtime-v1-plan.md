# Chat realtime (v1) Implementation Plan

> **For agentic workers:** Use **subagent-driven-development** or **executing-plans** to run this plan **task-by-task**. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship in-app **realtime chat (v1)** per `docs/specs/2026-05-02-chat-realtime-v1-design.md`: SignalR hub `/hubs/chat`, REST chat APIs, **`/chat`** full page (master–detail on mobile), global **dock**, **send retry queue** (IndexedDB, cap 50), **typing** emit/receive, **attachments + voice**, **browser notifications** + unread badges, **channels** + **conversations**.

**Architecture:** `frontend/src/features/chat/` — RTK Query slice extending `baseApi` for `/api/v1/chat/*`; **`chatHub.ts`** singleton `@microsoft/signalr` `HubConnection` with JWT `accessTokenFactory`; **`useTypingComposer`** uses **`typingThrottle.ts`** constants (350 ms burst, 2000 ms idle stop); **`outboxDb.ts`** wraps IndexedDB for pending sends; **`ChatProvider`** + **`ChatDock`** mounted under authenticated **`MainLayout`**; notifications via **`chatNotifications.ts`** (`Notification` API + fallback badge).

**Tech stack:** React 19, RTK Query 2, React Router 7, Tailwind 4, `@microsoft/signalr`, IndexedDB (native), Vitest for pure modules.

**Spec reference:** `docs/specs/2026-05-02-chat-realtime-v1-design.md` · REST · `docs/api/chat-fe-api.md`

---

## File map (create / modify)

| Area | Path | Responsibility |
|------|------|------------------|
| Package | `frontend/package.json` | Add `@microsoft/signalr` dependency |
| Env doc | `frontend/.env.example` or README snippet | `VITE_API_URL` — hub is `${origin}/hubs/chat` same host as API |
| Constants | `frontend/src/features/chat/constants/typingThrottle.ts` | `TYPING_DEBOUNCE_MS = 350`, `TYPING_IDLE_STOP_MS = 2000` |
| Constants | `frontend/src/features/chat/constants/outbox.ts` | `MAX_OUTBOX_ITEMS = 50`, `MAX_SEND_ATTEMPTS = 5`, backoff base |
| Types | `frontend/src/features/chat/types/chat.types.ts` | `ThreadId`, envelope types aligned with `MessageResponse` / hub payloads |
| Hub | `frontend/src/features/chat/lib/chatHub.ts` | Build URL, connect, register `IChatClient` handlers, invoke wrappers, reconnect invalidate |
| Hub URL | `frontend/src/features/chat/lib/hubUrl.ts` | `export function getChatHubUrl(): string` → `${API_URL.replace(/\/$/, '')}/hubs/chat` |
| Outbox | `frontend/src/features/chat/lib/outboxDb.ts` | IDB `chat-outbox` v1 store `pending` records `{ id, conversationId, payload, attempts, createdAt }` |
| Outbox logic | `frontend/src/features/chat/lib/processOutbox.ts` | Retry loop, cap enforcement |
| API | `frontend/src/features/chat/api/chat.api.ts` | injectEndpoints: conversations, messages, upload, attachments, channels |
| Context | `frontend/src/features/chat/context/ChatContext.tsx` | hub lifecycle, active thread, unread map |
| Typing hook | `frontend/src/features/chat/hooks/useTypingEmitter.ts` | debounced `SendTypingIndicator` |
| Notifications | `frontend/src/features/chat/lib/chatNotifications.ts` | permission, show, dedupe, visibility API |
| Dock | `frontend/src/features/chat/components/ChatDock.tsx` | minimized strips, panels, localStorage keys §4.2 |
| Page | `frontend/src/features/chat/components/ChatPage.tsx` | master–detail layout |
| Composer | `frontend/src/features/chat/components/ChatComposer.tsx` | text + attach + voice |
| i18n | `frontend/src/shared/i18n/locales/vi/chat.json`, `en/chat.json` | All user strings |
| Router | `frontend/src/app/router.tsx` | Route `chat` under `RequireAuth` |
| Layout | `frontend/src/shared/components/layouts/MainLayout.tsx` | Render `<ChatDock />` when `selectIsAuthenticated` |
| Store | `frontend/src/app/store.ts` | No change if `chat.api` uses existing `baseApi` reducer |
| baseApi tags | `frontend/src/shared/lib/api/baseApi.ts` | Add `ChatConversation`, `ChatMessage`, `ChatChannel` tagTypes |

---

## Task 0: Preflight — channel ↔ conversation for REST history

**Goal:** Unblock `GET /api/v1/chat/messages?conversationId=` for **channel** threads.

**Files:**
- Read: `src/Modules/Chat/UniHub.Chat.Application/Queries/GetMessages/GetMessagesQueryHandler.cs`, domain `Channel`/`Conversation` linkage
- Create: `docs/plans/2026-05-02-chat-channel-rest-notes.md` (short decision log)

- [x] **Step 1:** Trace whether each **joined channel** exposes a **conversationId** usable by `GetMessages`, or if backend needs a new query param (e.g. `channelId`). Document **FE thread model**: `{ kind: 'direct' \| 'group' \| 'channel', conversationId: Guid, channelId?: Guid }`.

- [x] **Step 2:** If only hub `SendChannelMessage` exists without list history, file a **backend follow-up** or define v1 channel UI as **hub-only messages + empty history** (NOT ideal — prefer fixing backend in same sprint if gap confirmed).

- [x] **Step 3:** Commit notes only when decision is written (`docs/plans/2026-05-02-chat-channel-rest-notes.md`).

```bash
git add docs/plans/2026-05-02-chat-channel-rest-notes.md
git commit -m "docs: record channel vs conversationId for chat REST"
```

---

## Task 1: Dependencies + hub URL helper + typing constants

**Files:**
- Modify: `frontend/package.json`
- Create: `frontend/src/features/chat/constants/typingThrottle.ts`
- Create: `frontend/src/features/chat/lib/hubUrl.ts`
- Test: `frontend/src/features/chat/constants/typingThrottle.test.ts` (optional — if exports test helpers)

- [ ] **Step 1:** Add SignalR client package.

```bash
cd frontend && npm install @microsoft/signalr
```

- [ ] **Step 2:** Create `typingThrottle.ts`:

```typescript
/** Keystroke quiet period before emitting typing=true (ms). */
export const TYPING_DEBOUNCE_MS = 350
/** Idle period before emitting typing=false (ms). */
export const TYPING_IDLE_STOP_MS = 2000
```

- [ ] **Step 3:** Create `hubUrl.ts`:

```typescript
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5034'

export function getChatHubUrl(): string {
  const base = API_URL.replace(/\/$/, '')
  return `${base}/hubs/chat`
}
```

- [ ] **Step 4:** Run `npm run build` in `frontend/` — expect PASS.

- [ ] **Step 5:** Commit.

```bash
git add frontend/package.json frontend/package-lock.json frontend/src/features/chat/constants/typingThrottle.ts frontend/src/features/chat/lib/hubUrl.ts
git commit -m "feat(chat): add signalr client, hub URL helper, typing constants"
```

---

## Task 2: IndexedDB outbox (schema + cap)

**Files:**
- Create: `frontend/src/features/chat/constants/outbox.ts`
- Create: `frontend/src/features/chat/lib/outboxDb.ts`
- Create: `frontend/src/features/chat/lib/outboxDb.test.ts` (mock IDB or test pure helpers)
- Test: Vitest for `canEnqueue()` when at cap

**Schema:** Database name `unihub-chat-outbox`, version `1`, store `messages` keyPath `id` (uuid client-generated).

```typescript
export type OutboxRecord = {
  id: string
  conversationId: string
  body: { type: 'text'; content: string; replyToMessageId?: string }
  attempts: number
  createdAt: number
  lastError?: string
}
```

- [ ] **Step 1:** Create `outbox.ts` with `MAX_OUTBOX_ITEMS = 50`, `MAX_SEND_ATTEMPTS = 5`.

- [ ] **Step 2:** Implement `outboxDb.ts` with `openDb()`, `enqueue()`, `listPending()`, `remove()`, `updateAttempts()`, `count()` using `indexedDB` API (open cursor).

- [ ] **Step 3:** Write Vitest test for pure function:

```typescript
// outboxPolicy.ts
export function shouldBlockEnqueue(count: number, max: number): boolean {
  return count >= max
}
```

Run: `cd frontend && npx vitest run src/features/chat/lib/outboxPolicy.test.ts -v` → PASS.

- [ ] **Step 4:** Commit.

```bash
git add frontend/src/features/chat/constants/outbox.ts frontend/src/features/chat/lib/outboxDb.ts frontend/src/features/chat/lib/outboxPolicy.ts frontend/src/features/chat/lib/outboxPolicy.test.ts
git commit -m "feat(chat): add IndexedDB outbox schema and enqueue policy"
```

---

## Task 3: RTK Query `chat.api.ts` (REST core)

**Files:**
- Modify: `frontend/src/shared/lib/api/baseApi.ts` — tagTypes `ChatConversation`, `ChatMessage`, `ChatChannel`
- Create: `frontend/src/features/chat/api/chat.api.ts`
- Modify: `frontend/src/app/store.ts` — ensure reducer already includes injected endpoints (RTK auto if same baseApi)

**Endpoints (minimal v1):**
- `getConversations` → `GET /api/v1/chat/conversations`
- `getMessages` → `GET /api/v1/chat/messages` query `conversationId`, `page`, `pageSize`
- `sendMessage` → `POST /api/v1/chat/messages`
- `uploadChatFile` → `POST /api/v1/chat/messages/upload` **multipart** (field `file` per `chat-fe-api.md`)
- `sendWithAttachments` → `POST /api/v1/chat/messages/with-attachments`
- `getPublicChannels` → `GET /api/v1/chat/channels/public`
- `getMyChannels` → `GET /api/v1/chat/channels/my-channels`
- `createDirectConversation` → `POST /api/v1/chat/conversations/direct`
- `createGroupConversation` → `POST /api/v1/chat/conversations/group`
- `joinChannel` / `leaveChannel` as needed

- [ ] **Step 1:** Implement endpoints with `transformResponse` using existing `data` envelope pattern from `forum.list.api.ts`.

- [ ] **Step 2:** Export hooks: `useGetConversationsQuery`, `useGetMessagesQuery`, `useSendMessageMutation`, etc.

- [ ] **Step 3:** `npm run build` — PASS.

- [ ] **Step 4:** Commit.

```bash
git add frontend/src/shared/lib/api/baseApi.ts frontend/src/features/chat/api/chat.api.ts
git commit -m "feat(chat): add RTK Query slice for chat REST APIs"
```

---

## Task 4: `chatHub.ts` — connect, handlers, invalidate RTK

**Files:**
- Create: `frontend/src/features/chat/lib/chatHub.ts`
- Create: `frontend/src/features/chat/lib/mapHubMessage.ts` — normalize `MessageNotification` → invalidate tags

**Connection:**

```typescript
import * as signalR from '@microsoft/signalr'
import { getChatHubUrl } from './hubUrl'

export function createChatConnection(getAccessToken: () => string | null) {
  return new signalR.HubConnectionBuilder()
    .withUrl(getChatHubUrl(), {
      accessTokenFactory: async () => getAccessToken() ?? '',
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000])
    .build()
}
```

Register handlers matching **`IChatClient`** PascalCase — SignalR JS typically uses **same method names** as server sends (confirm casing: .NET hub sends `ReceiveMessage` → client `.on('receiveMessage', ...)` **verify in browser Network WS frame**; adjust to actual casing).

On `ReceiveMessage` / reconnect: `dispatch(chatApi.util.invalidateTags([{ type: 'ChatMessage', id: conversationId }, ...]))`.

- [ ] **Step 1:** Implement connection factory + `start()`, `stop()`, `onReconnecting`, `onReconnected` (invalidate conversations list).

- [ ] **Step 2:** Manual test: log in, open DevTools → WS → connect hub → invoke `JoinConversation` with known GUID.

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/lib/chatHub.ts frontend/src/features/chat/lib/mapHubMessage.ts
git commit -m "feat(chat): SignalR hub client with reconnect invalidation"
```

---

## Task 5: `ChatProvider` + wire hub to Redux store

**Files:**
- Create: `frontend/src/features/chat/context/ChatContext.tsx`
- Modify: `frontend/src/app/router.tsx` or `MainLayout.tsx` — wrap authenticated tree with provider

Provider holds: `connection`, `joinConversation(id)`, `joinChannel(id)`, `hubSendTyping`.

- [ ] **Step 1:** Implement provider with `useRef` for connection, `useEffect` start on auth token present, stop on logout.

- [ ] **Step 2:** Listen `auth.logout` path — call `connection.stop()`.

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/context/ChatContext.tsx frontend/src/shared/components/layouts/MainLayout.tsx
git commit -m "feat(chat): ChatProvider and hub lifecycle tied to auth"
```

---

## Task 6: Typing — `useTypingEmitter` + composer integration

**Files:**
- Create: `frontend/src/features/chat/hooks/useTypingComposer.ts`
- Uses: `TYPING_DEBOUNCE_MS`, `TYPING_IDLE_STOP_MS`

Logic: on `onChange` text — `clearTimeout` idleTimer; schedule `invoke typing true` after debounce once per burst; reset idleTimer for `typing false` after `TYPING_IDLE_STOP_MS`.

- [ ] **Step 1:** Implement hook returning `{ onComposerChange, flushStop }`.

- [ ] **Step 2:** Unit-test fake timers (Vitest `vi.useFakeTimers()`).

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/hooks/useTypingComposer.ts frontend/src/features/chat/hooks/useTypingComposer.test.ts
git commit -m "feat(chat): typing emitter debounce per spec §4.6"
```

---

## Task 7: `/chat` page — master–detail + lists

**Files:**
- Create: `frontend/src/features/chat/components/ChatPage.tsx`
- Create: `frontend/src/features/chat/components/ConversationList.tsx`
- Create: `frontend/src/features/chat/components/ChannelExplore.tsx`
- Create: `frontend/src/features/chat/components/ChatThread.tsx`
- Modify: `frontend/src/app/router.tsx` — `{ path: 'chat', element: <ChatPage /> }` inside `RequireAuth`

Mobile: list full width; thread replaces list with **Back** button (`navigate(-1)` or local state `selectedThreadId`).

- [ ] **Step 1:** Implement list data from `useGetConversationsQuery`, channels from public/my endpoints.

- [ ] **Step 2:** `npm run build` PASS.

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/components/ChatPage.tsx frontend/src/app/router.tsx
git commit -m "feat(chat): ChatPage route with conversation list shell"
```

---

## Task 8: Composer — text, attachments, voice

**Files:**
- Create: `frontend/src/features/chat/components/ChatComposer.tsx`
- Create: `frontend/src/features/chat/hooks/useVoiceRecorder.ts` — MediaRecorder, timer, blob

Flow: pick file → `uploadChatFile` mutation → build `SendMessageWithAttachmentsRequest`. Voice: record → upload same endpoint → send.

- [ ] **Step 1:** Text send uses `sendMessage` OR outbox enqueue + flush (integrate Task 9).

- [ ] **Step 2:** Respect spec §4.5 — timer while recording, discard, retry upload on failure.

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/components/ChatComposer.tsx frontend/src/features/chat/hooks/useVoiceRecorder.ts
git commit -m "feat(chat): composer with attachment upload and voice note"
```

---

## Task 9: Outbox processor + merge with `sendMessage`

**Files:**
- Create: `frontend/src/features/chat/lib/processOutbox.ts`
- Wire: on app focus / online event / after successful send — drain queue

Backoff: `Math.min(30000, 1000 * Math.pow(2, attempts))` ms between retries.

- [ ] **Step 1:** Implement drain loop calling RTK `sendMessage` mutation imperatively via `store.dispatch(chatApi.endpoints.sendMessage.initiate(...))`.

- [ ] **Step 2:** When cap exceeded, surface toast/string from i18n `chat.outbox.full`.

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/lib/processOutbox.ts
git commit -m "feat(chat): outbox retry processor with backoff and cap"
```

---

## Task 10: `ChatDock` + persistence keys

**Files:**
- Create: `frontend/src/features/chat/components/ChatDock.tsx`
- Create: `frontend/src/features/chat/lib/dockStorage.ts` — `localStorage` keys `chat:dock:visibility`, `chat:dock:openThreads` (JSON array)

Mount in `MainLayout.tsx` below topbar; `fixed bottom-0 right-0 z-50`; hide when `dockStorage.getVisibility() === 'hidden'`.

- [ ] **Step 1:** Implement minimized bar + expand panel for active threads (reuse `ChatThread` compact).

- [ ] **Step 2:** Document multi-tab limitation in `dockStorage.ts` comment (§4.2).

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/components/ChatDock.tsx frontend/src/shared/components/layouts/MainLayout.tsx
git commit -m "feat(chat): global ChatDock with localStorage persistence"
```

---

## Task 11: Browser notifications + unread badge

**Files:**
- Create: `frontend/src/features/chat/lib/chatNotifications.ts`
- Modify: `ChatDock.tsx` — badge count from context

On `ReceiveMessage`: if `document.visibilityState !== 'visible'` and permission `granted`, `new Notification(...)`. Always increment unread in context for badge.

- [ ] **Step 1:** Request permission on first user interaction “Enable notifications” button (avoid automatic prompt on load — better UX; optional auto-request after 2nd message — **pick:** explicit button in `/chat` header).

- [ ] **Step 2:** Dedupe with `Map<threadKey, lastNotifiedId>`.

- [ ] **Step 3:** Commit.

```bash
git add frontend/src/features/chat/lib/chatNotifications.ts
git commit -m "feat(chat): web notifications and unread badge fallbacks"
```

---

## Task 12: New DM — search + suggestions

**Depends:** Identity **user search** endpoint (spec §6 item 2). If missing:

- [ ] **Step 1:** Add minimal `GET /api/v1/users/search?q=` in Identity module **OR** temporary dev-only picker.

- [ ] **Step 2:** UI: combobox in `ChatPage` → `createDirectConversation` → navigate to thread.

- [ ] **Step 3:** Commit.

---

## Task 13: i18n + a11y pass

- [ ] **Step 1:** Fill `vi/chat.json` and `en/chat.json` with keys used above (`typing`, `outbox`, `dock`, `notifications`, `composer`).

- [ ] **Step 2:** Run `npm run verify:i18n` — PASS.

- [ ] **Step 3:** Commit.

---

## Task 14: Docs + SignalR contract table (spec §6 item 5)

- [ ] **Step 1:** Add `docs/api/chat-signalr-fe-contract.md` table: server method → parameters; client event → payload fields from `IChatClient`.

- [ ] **Step 2:** Commit.

---

## Self-review (plan vs spec)

| Spec § | Covered by |
|--------|------------|
| §1 decisions | Tasks 7–11 |
| §3 hub | Tasks 4–5, 14 |
| §4.2 dock | Task 10 |
| §4.5 voice/attach | Task 8 |
| §4.6 typing | Tasks 1, 6 |
| §4.7 notifications | Task 11 |
| §4.8 queue | Tasks 2, 9 |
| §4.9 mobile | Task 7 |
| §6 open (1) channels | Task 0 |
| §6 open (2) search | Task 12 |

**Gap:** Optional **BroadcastChannel** cross-tab dock — not scheduled; add as Task 15 stretch.

---

## Execution options

**Plan saved to:** `docs/plans/2026-05-02-chat-realtime-v1-plan.md`

**Two ways to execute:**

1. **Subagent-driven (recommended)** — Fresh subagent per task; review between tasks.
2. **Inline execution** — Same session, batches with checkpoints (`executing-plans`).

Which approach do you want for implementation?
