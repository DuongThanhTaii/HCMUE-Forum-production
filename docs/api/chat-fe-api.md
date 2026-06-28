# Chat API for Frontend (`ApiResponse` Envelope)

## Base URL
- `/api/v1/chat`

## Auth
- Required (JWT Bearer) for all endpoints.

## Envelope
```json
{
  "success": true,
  "data": {},
  "message": "optional success message",
  "error": null
}
```

Error sample:
```json
{
  "success": false,
  "data": null,
  "message": null,
  "error": "Conversation not found"
}
```

## Conversations

### `GET /api/v1/chat/conversations`
- **200**: `ApiResponse<IReadOnlyList<ConversationResponse>>`

### `POST /api/v1/chat/conversations/direct`
- **Body**: `CreateDirectConversationRequest`
- **201**: `ApiResponse<CreateDirectConversationResponse>`

### `POST /api/v1/chat/conversations/group`
- **Body**: `CreateGroupConversationRequest`
- **201**: `ApiResponse<CreateGroupConversationResponse>`

### `POST /api/v1/chat/conversations/{id}/participants`
- **Body**: `AddParticipantRequest`
- **200**: `ApiResponse<null>` with message `Participant added successfully`
- **404**: failure envelope (`Conversation.NotFound`)

### `DELETE /api/v1/chat/conversations/{id}/participants/{participantId}`
- **200**: `ApiResponse<null>` with message `Participant removed successfully`
- **404**: failure envelope (`Conversation.NotFound`)

### `GET /api/v1/chat/conversations/{id}/attachments`
- **Query**: `kind` (`all` \| `image` \| `file` \| `voice`, default `all`), `page`, `pageSize`
- **200**: `ApiResponse<PagedResponse<ConversationAttachmentResponse>>`
  - Item: `messageId`, `sentAt`, `fileName`, `fileUrl`, `mimeType`, `thumbnailUrl`, `fileSize`
- **403**: `Conversation.NotParticipant`
- **404**: `Conversation.NotFound`

### `GET /api/v1/chat/conversations/{id}/links`
- **Query**: `page`, `pageSize`
- **200**: `ApiResponse<PagedResponse<ConversationLinkResponse>>`
  - Item: `messageId`, `sentAt`, `url`, `host`
- **403**: `Conversation.NotParticipant`
- **404**: `Conversation.NotFound`

### `GET /api/v1/chat/conversations/{id}/messages/search`
- **Query**: `q` (required, min 2 chars), `filter` (`all` \| `text` \| `media` \| `links`, default `all`), `page`, `pageSize`
- **200**: `ApiResponse<PagedResponse<MessageSearchHitResponse>>`
  - Item: `messageId`, `sentAt`, `snippet`, `senderId`, `senderDisplayName`
- **403**: failure envelope (`Conversation.NotParticipant`)
- **404**: failure envelope (`Conversation.NotFound`)

### `POST /api/v1/chat/conversations/{id}/mute`
- **Body**: `{ "muted": boolean }`
- **200**: `ApiResponse<null>` with message `Conversation mute updated`
- **403**: `Conversation.NotParticipant`
- **404**: `Conversation.NotFound`

## Messages

### `GET /api/v1/chat/messages?conversationId=&page=&pageSize=`
- **Query**: `conversationId` (required), `page`, `pageSize`
- **200**: `ApiResponse<PagedResponse<MessageResponse>>`
- **404**: failure envelope (`Conversation.NotFound`)

### `POST /api/v1/chat/messages`
- **Body**: `SendMessageRequest`
- **201**: `ApiResponse<SendMessageResponse>`
- **403**: failure envelope (`Conversation.NotParticipant` or `Chat.UserBlocked`)
- **404**: failure envelope (resource not found)

### `POST /api/v1/chat/messages/upload`
- **Content-Type**: `multipart/form-data` (field name: `file`)
- **200**: `ApiResponse<UploadFileResponse>`
- **400**: failure envelope (`No file provided`, validation errors)

### `POST /api/v1/chat/messages/with-attachments`
- **Body**: `SendMessageWithAttachmentsRequest`
- **201**: `ApiResponse<SendMessageResponse>`
- **403**: failure envelope (`Conversation.NotParticipant` or `Chat.UserBlocked`)
- **404**: failure envelope (resource not found)

### `POST /api/v1/chat/messages/{messageId}/report`
- **Body**: `{ "reason": "Spam" | "Harassment" | "Inappropriate" | "Other", "description": string | null }`
- **201**: `ApiResponse<null>` with message `Report submitted`
- **403**: `Conversation.NotParticipant`
- **404**: `Message.NotFound`

### `POST /api/v1/chat/messages/{messageId}/reactions`
- **Body**: `AddReactionRequest`
- **200**: `ApiResponse<null>` with message `Reaction added successfully`
- **404**: failure envelope (`Message.NotFound`)

### `DELETE /api/v1/chat/messages/{messageId}/reactions/{emoji}`
- **200**: `ApiResponse<null>` with message `Reaction removed successfully`
- **404**: failure envelope (`Message.NotFound`)

### `POST /api/v1/chat/messages/{messageId}/read`
- **200**: `ApiResponse<null>` with message `Message marked as read`
- **404**: failure envelope (`Message.NotFound`)

### `GET /api/v1/chat/messages/{messageId}/read-receipts`
- **200**: `ApiResponse<List<ReadReceiptResponse>>`
- **404**: failure envelope (`Message.NotFound`)

## Channels

### `GET /api/v1/chat/channels/public`
- **200**: `ApiResponse<IReadOnlyList<ChannelResponse>>`

### `GET /api/v1/chat/channels/my-channels`
- **200**: `ApiResponse<IReadOnlyList<ChannelResponse>>`

### `POST /api/v1/chat/channels`
- **Body**: `CreateChannelRequest`
- **201**: `ApiResponse<CreateChannelResponse>`

### `POST /api/v1/chat/channels/{id}/join`
- **200**: `ApiResponse<null>` with message `Successfully joined channel`
- **404**: failure envelope (`Channel.NotFound`)

### `POST /api/v1/chat/channels/{id}/leave`
- **200**: `ApiResponse<null>` with message `Successfully left channel`
- **404**: failure envelope (`Channel.NotFound`)

### `POST /api/v1/chat/channels/{id}/moderators`
- **Body**: `ModeratorRequest`
- **200**: `ApiResponse<null>` with message `Moderator added successfully`
- **403**: failure envelope (not owner/authorized)
- **404**: failure envelope (`Channel.NotFound`)

### `DELETE /api/v1/chat/channels/{id}/moderators/{moderatorId}`
- **200**: `ApiResponse<null>` with message `Moderator removed successfully`
- **403**: failure envelope (not owner/authorized)
- **404**: failure envelope (`Channel.NotFound`)

### `PUT /api/v1/chat/channels/{id}`
- **Body**: `UpdateChannelRequest`
- **200**: `ApiResponse<null>` with message `Channel updated successfully`
- **403**: failure envelope (not moderator/authorized)
- **404**: failure envelope (`Channel.NotFound`)

## User safety (Identity module)

Base: `/api/v1/users` (same JWT auth).

### `POST /api/v1/users/{userId}/block`
- **200**: `ApiResponse<null>` — `User blocked`

### `DELETE /api/v1/users/{userId}/block`
- **200**: `ApiResponse<null>` — `User unblocked`

### `GET /api/v1/users/me/blocked`
- **200**: `ApiResponse<IReadOnlyList<BlockedUserResponse>>`
  - Item: `userId`, `blockedAt`

Blocking is enforced when creating a DM or sending messages (`Chat.UserBlocked`, HTTP 403).

## Schemas

### `ConversationResponse`
- `id` (guid)
- `type` (string)
- `participantIds` (guid[])
- `lastMessageAt` (datetime | null)
- `createdAt` (datetime)
- `isArchived` (boolean)
- `isMuted` (boolean) — current user muted this conversation
- `isBlockedWithPeer` (boolean) — DM peer block in either direction (direct chats only)

### `CreateDirectConversationRequest`
- `otherUserId` (guid)

### `CreateDirectConversationResponse`
- `conversationId` (guid)

### `CreateGroupConversationRequest`
- `title` (string)
- `participantIds` (guid[])

### `CreateGroupConversationResponse`
- `conversationId` (guid)

### `AddParticipantRequest`
- `participantId` (guid)

### `PagedResponse<MessageResponse>`
- `items` (MessageResponse[])
- `page` (int)
- `pageSize` (int)
- `totalCount` (int)
- `totalPages` (int)

### `MessageResponse`
- `id` (guid)
- `conversationId` (guid)
- `senderId` (guid)
- `content` (string)
- `type` (string)
- `sentAt` (datetime)
- `editedAt` (datetime | null)
- `isDeleted` (boolean)
- `replyToMessageId` (guid | null)
- `reactions` (object: emoji -> userId[])

### `SendMessageRequest`
- `conversationId` (guid)
- `content` (string)
- `replyToMessageId` (guid | null, optional)

### `SendMessageWithAttachmentsRequest`
- `conversationId` (guid)
- `content` (string | null)
- `attachments` (AttachmentRequest[])
- `replyToMessageId` (guid | null, optional)

### `AttachmentRequest`
- `fileName` (string)
- `fileUrl` (string)
- `fileSize` (long)
- `mimeType` (string)
- `thumbnailUrl` (string | null)

### `SendMessageResponse`
- `messageId` (guid)
- `sentAt` (datetime)

### `UploadFileResponse`
- `fileName` (string)
- `fileUrl` (string)
- `fileSize` (long)
- `contentType` (string)

### `AddReactionRequest`
- `emoji` (string)

### `ReadReceiptResponse`
- `userId` (guid)
- `readAt` (datetime)

### `ChannelResponse`
- `id` (guid)
- `name` (string)
- `description` (string | null)
- `type` (string)
- `ownerId` (guid)
- `memberCount` (int)
- `createdAt` (datetime)
- `isArchived` (boolean)

### `CreateChannelRequest`
- `name` (string)
- `description` (string | null)
- `isPublic` (boolean)

### `CreateChannelResponse`
- `channelId` (guid)

### `ModeratorRequest`
- `userId` (guid)

### `UpdateChannelRequest`
- `name` (string | null)
- `description` (string | null)
