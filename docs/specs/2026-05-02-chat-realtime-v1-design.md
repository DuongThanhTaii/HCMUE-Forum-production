# Chat realtime (v1) — design spec

**Status:** Draft for review  
**Date:** 2026-05-02  
**Last revised:** 2026-05-02 (review feedback)  
**Scope label:** C + channels (B) + reliable send (B) + DM discovery (C) + dock (C) + browser notifications (A)  
**Out of scope (v1):** Video/voice *calls*, screen share, WebRTC signaling.

## 1. Product decisions (locked from brainstorming)

| Topic | Choice |
|--------|--------|
| Feature bundle | **C** — text realtime + typing + dock + full page + **images/docs** + **voice notes**; no video call |
| Public channels | **B** — **Channels** in v1 (`/channels` API: public + my) alongside **Conversations** (1-1 + group) |
| Offline / delivery | **B** — **Retry queue** for failed sends; recipient **refetches** history on reconnect + **SignalR** when live |
| New 1-1 chat | **C** — **Search** (name/email) + **suggestions** (e.g. recent partners when data exists) |
| Dock visibility | **C** — dock **everywhere** when authenticated + **hide/minimize** persisted (e.g. `localStorage`) |
| Background alerts | **A** — **Browser Notification API**, permission prompt, show on new messages (prioritize DMs; channels may aggregate later) |

## 2. Architectural approaches (frontend)

1. **Recommended — Feature module + RTK Query + single SignalR hub client**  
   - `features/chat`: REST via RTK Query (`baseApi`), connection lifecycle in `chatHub.ts`, thin React context for dock + route `/chat`.  
   - **Send queue:** separate module (retry with backoff, surfaces “đang gửi / lỗi / đã gửi” — i18n keys in `vi`/`en`).  
   - Aligns with existing Redux/`baseApi` patterns.

2. **Alternative — Zustand for ephemeral UI** (typing bubbles, dock open state) on top of (1). Use only if Redux updates feel noisy.

3. **Deferred — TanStack Query-only** — higher hand-merge cost with hub; not recommended for v1.

## 3. Backend surface (existing)

- REST: see `docs/api/chat-fe-api.md` — conversations, messages (paged), upload, send with attachments, channels, reactions, read receipts.  
- SignalR hub is mapped at **`/hubs/chat`** (see `Program.cs`).  
- **Server-invokable methods** on `ChatHub` (for FE `connection.invoke`):  
  `JoinConversation`, `LeaveConversation`, `SendMessage`, `SendTypingIndicator`, `JoinChannel`, `LeaveChannel`, `SendChannelMessage`, `AddReaction`, `RemoveReaction`, `MarkMessageAsRead`.  
- **Client handlers** (`IChatClient` — register with `connection.on`):  
  `ReceiveMessage`, `MessageEdited`, `MessageDeleted`, `UserJoined`, `UserLeft`, `UserTyping`, `ReactionAdded`, `ReactionRemoved`, `MessageRead`, `ChannelUpdated`, `UserStatusChanged`.  
- **Implementation note:** `MessageNotification` includes both `ConversationId` and optional `ChannelId` — use this to unify thread rendering once payloads are verified in integration tests.

## 4. UX / IA

### 4.1 Routes

- **`/chat`** — full-screen inbox: sidebar (segments **Messages** vs **Channels**) + thread view + composer.  
- **Dock** — global overlay (z-index above content, below modal alerts); layout reference: **multi-strip minimized bar + expandable chat panels** (same interaction model as common social “chat heads”; avoid relying on the word “Facebook-like” in tickets — **wireframe link TBD** before marking spec *Approved*).

### 4.2 Dock behavior (decision C)

- Persist: **hidden**, **minimized** (bar only), **expanded** panels — keys e.g. `chat:dock:visibility`, `chat:dock:openThreads`.  
- **Multi-tab caveat:** `localStorage` does **not** sync across tabs. Default v1: **document as known limitation**. Optional improvement: **`BroadcastChannel`** (`chat-dock-sync`) to broadcast visibility + open thread IDs so all tabs stay aligned; if not built in v1, keep the limitation in release notes.  
- Respect **keyboard**: Esc minimizes panel; focus trap inside expanded panel (ui-ux-pro-max: a11y).

### 4.3 New conversation

- Entry points: **New message** from `/chat` and optionally shortcut from dock.  
- **Search** users (requires Identity/search endpoint — add if missing).  
- **Suggestions:** derive from `GET /chat/conversations` sorted by `lastMessageAt` (top N).

### 4.4 Channels (decision B)

- Tab **Khám phá** / **Public** — `GET .../channels/public`.  
- **Của tôi** — `GET .../channels/my-channels`.  
- Join/leave per existing API; thread UI mirrors DM where IDs align.

### 4.5 Composer — attachments & voice (scope C)

- **Images/docs:** `multipart` upload → `UploadFileResponse` → `SendMessageWithAttachmentsRequest` with `AttachmentRequest` rows.  
- **Voice:** record in-browser (`MediaRecorder`), upload as file (treat as attachment).  
  - **While recording:** show a **live elapsed timer** (e.g. `0:00` → mm:ss).  
  - **After stop, before send:** minimum **play** control + **duration** label; allow **discard** and **re-record**.  
  - **Upload failure:** keep the blob in memory (or short-lived session reference) so user can **retry upload** without re-recording until they navigate away or explicitly discard.  
  - Waveform UI — **deferred** unless time permits.  
- Enforce limits **from API**; FE validates roughly before upload (avoid useless traffic).

### 4.6 Typing indicators

- **Receive:** subscribe to hub `UserTyping` (`IChatClient.UserTyping`); show “**X đang nhập…**” in **dock panel** and **full page**.  
- **Emit (composer):** debounce keystrokes so the server is not spammed:  
  - On input change in the active thread, **start/restart a timer**; after **300–500 ms** without a new keystroke, invoke `SendTypingIndicator(conversationId, isTyping: true)` once per “burst”.  
  - After **~2 s** of **no** keystrokes, invoke `SendTypingIndicator(..., false)` (stop typing).  
  - On blur / leave thread / send message, emit **stop** immediately.  
  - Document these constants in `chatHub` or a small `typingThrottle.ts` so two devs cannot diverge silently.

### 4.7 Notifications (decision A)

- **Granted:** post `Notification` when a new message arrives for a thread that is **not** in foreground focus; dedupe by `(conversationId or channel context, latestMessageId)` or debounce ~2–5 s per thread. Click → navigate `/chat` or open dock thread.  
- **Denied or default (never asked):** do **not** block core chat. **In-app fallbacks always on:**  
  - **Unread badge** on dock launcher / icon.  
  - **Per-thread unread count** in conversation list (from REST + hub-driven invalidation).  
  - Optional **toast** inside the app (non-native) for power users — lower priority than badge.  
- Settings v2: per-thread mute / channel noise — out of scope unless time permits.

### 4.8 Reliable send (decision B)

- Outbound: enqueue on Send; on HTTP failure → retry with backoff; **max attempts** → failed row + **“Thử lại”**.  
- **Queue bounds:** cap **pending** items (recommend **50** messages — tune after UX test). If exceeded, **block new sends** with clear copy (“Gửi quá nhiều tin đang chờ — xử lý hoặc xóa bớt”) rather than silent data loss.  
- **Persistence:** queue must **survive full page refresh** — use **IndexedDB** (or `localStorage` only for metadata + IDB for payloads if large). **In-memory-only** is not sufficient for decision B.  
- **Inbound:** on SignalR reconnect, invalidate messages for open threads + refresh conversation list.

### 4.9 Mobile — `/chat` full page (narrow viewport)

- **v1 default:** **master–detail**: first screen = **conversation/channel list**; selecting a thread opens **full-width thread** with a **back** control to the list. Sidebar does not stay fixed beside content on 375px.  
- **Alternative** (not v1 unless specified): sidebar as **drawer** over thread — **TBD** only if usability testing rejects master–detail.  
- Dock (§5): still collapses to a **single** floating panel / strip per earlier mobile note.

## 5. Non-functional

- **Auth:** JWT on REST + hub; reuse `baseQueryWithReauth`.  
- **i18n:** `chat.json` **vi** and **en**; **Vietnamese is the primary authoring locale** for UniHub-HCMUE (English parity required). Status strings like “đang gửi / lỗi / đã gửi” belong in `vi/chat.json` with English equivalents in `en/chat.json`.  
- **Mobile width:** dock collapses to single panel; `/chat` uses master–detail pattern (§4.9); test **375px**.

## 6. Open points before coding

1. **Channel ↔ conversation ID** mapping for REST message pagination vs hub payload (`ChannelId` on `MessageNotification`) — confirm single thread model for FE.  
2. **User search** endpoint for DM (Identity module).  
3. **Hub base URL** for FE env (`VITE_*`): production vs dev must resolve to same origin or configured API host + `/hubs/chat`.  
4. Exact **upload size** limits from server validation messages (attach to FE pre-check).  
5. **SignalR contract checklist:** freeze a **markdown table** mapping each `connection.on(...)` handler to notification type + sample payload (pull from `IChatClient` + `ChatHub` XML comments). Add integration test or manual script: connect → `JoinConversation` → receive `UserTyping`.

## 7. Self-review

- Scope unchanged; calls/screenshare still out.  
- Channel routing still flagged; typing **emit** and receive **debounce** now explicit.  
- Retry queue **storage model** and **cap** specified to unblock module interfaces.  
- Notification **fallback** paths documented.  
- Dock **cross-tab** limitation + optional `BroadcastChannel` noted.

---

**Next step after approval:** Use **writing-plans** skill to produce an implementation plan (tasks ordered: hub wire-up → REST slices → `/chat` page → dock → queue → attachments/voice → notifications → polish).

**Blocking before implementation plan (per review):** resolve **typing emit constants** (already specified above — encode in shared constants file) and **retry queue persistence** (IDB schema + cap — specified above; finalize schema in plan phase).
