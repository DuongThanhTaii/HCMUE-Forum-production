# Channel vs conversation — REST history (Task 0 decision log)

**Date:** 2026-05-02  
**Spec:** `docs/specs/2026-05-02-chat-realtime-v1-design.md`  
**Related plan:** `docs/plans/2026-05-02-chat-realtime-v1-plan.md` (Task 0)

## Finding

- **`GET /api/v1/chat/messages`** accepts **`conversationId` only** and loads rows from the **Message** store keyed by **`ConversationId`** (`GetMessagesQueryHandler` → `IConversationRepository.ExistsAsync` + `IMessageRepository.GetByConversationIdAsync`).
- **Channels** are a **separate aggregate** from **Conversations**. `ConversationType` has **Direct** and **Group** only — no channel type.
- **`ChannelResponse`** exposes **`Id`, name, metadata** — **no `conversationId`** (`GetChannelsQuery.cs`).
- **Joining a channel** updates the **Channel** aggregate only; it **does not** create or attach a conversation (`JoinChannelCommandHandler`).
- **Hub `SendChannelMessage`** broadcasts to the SignalR channel group and uses **`Guid.Empty` for conversationId** in the payload; it **does not** call the application layer or persist a message (see comment in `ChatHub.cs`).

So there is **no** stable **`conversationId`** for a channel thread today, and **no** REST history for channel traffic.

## Frontend thread model (v1 target)

Use a discriminated shape so DMs/groups and channels do not share IDs blindly:

```typescript
type ChatThreadRef =
  | { kind: 'direct' | 'group'; conversationId: string }
  | { kind: 'channel'; channelId: string; conversationId?: undefined }
```

- For **direct/group**, **`conversationId`** is the only ID needed for **`GET /messages`**, **`POST /messages`**, and hub groups keyed by conversation.
- For **channels**, **`channelId`** is the hub identity (`JoinChannel`, `SendChannelMessage`, `ReceiveMessage` with `channelId` in the notification). **Do not** pass **`channelId`** as **`conversationId`** to REST — it will 404 or corrupt semantics.

Until backend persistence exists, the UI may show **realtime-only** channel messages (SignalR) and **empty** scrollback from REST, or a explicit **“history unavailable”** state — **not** ideal for v1 spec; see below.

## Backend follow-up (recommended same sprint as chat v1)

Pick one (product + schema):

1. **One backing conversation per channel** — e.g. create a **`Conversation`** (new type **Channel** or reuse **Group** with fixed membership rules) when a **`Channel`** is created; expose **`conversationId`** on **`ChannelResponse`** (or resolve via `GET /channels/{id}`). Then **`GET /messages?conversationId=`** works unchanged for channel threads.
2. **Dedicated channel message store** — **`GET /api/v1/chat/messages?channelId=`** (or **`/channels/{id}/messages`**) with auth/membership checks; **`SendChannelMessage`** persists and returns the real **`messageId`**.

Either path removes hub-only ephemerality and aligns with the **reliable send / reconnect** story.

## Code references

| Area | Location |
|------|----------|
| Messages query | `UniHub.Chat.Application/Queries/GetMessages/GetMessagesQueryHandler.cs` |
| Channel hub send | `UniHub.Chat.Presentation/Hubs/ChatHub.cs` → `SendChannelMessage` |
| Conversation types | `UniHub.Chat.Domain/Conversations/ConversationType.cs` |
| Channel list DTO | `UniHub.Chat.Application/Queries/GetChannels/GetChannelsQuery.cs` → `ChannelResponse` |
