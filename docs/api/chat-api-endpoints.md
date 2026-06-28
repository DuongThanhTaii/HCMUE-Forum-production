# Chat Module API Documentation

> Comprehensive API reference for UniHub Chat Module

---

## 📚 Overview

The Chat Module provides real-time messaging capabilities through REST APIs and SignalR. This document covers all REST API endpoints for conversations, messages, channels, and related features.

**Base URL:** `/api/v1/chat`

**Authentication:** All endpoints require JWT Bearer token authentication.

**Response Format:** `ApiResponse<T>` envelope JSON

> FE note: từ 2026-03-26, Chat endpoints đã migrate sang envelope. Các payload ví dụ cũ bên dưới nên được hiểu là phần `data` bên trong `ApiResponse<T>`.
>
> Frontend-friendly quick reference: `docs/api/chat-fe-api.md`.

Envelope mẫu (success):

```json
{
  "success": true,
  "data": {},
  "message": "optional",
  "error": null
}
```

Envelope mẫu (failure):

```json
{
  "success": false,
  "data": null,
  "message": null,
  "error": "Human readable message"
}
```

---

## 📋 Table of Contents

- [Conversations API](#conversations-api)
- [Messages API](#messages-api)
- [Channels API](#channels-api)
- [Error Responses](#error-responses)

---

## 🗨️ Conversations API

### Get All Conversations

Get all conversations for the authenticated user, ordered by last message time.

**Endpoint:** `GET /conversations`

**Authentication:** Required

**Query Parameters:** None

**Response:** `200 OK`

```json
[
  {
    "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "type": "Direct|Group",
    "title": "Group Chat Name",
    "participantIds": ["uuid1", "uuid2"],
    "lastMessageTime": "2026-02-07T10:30:00Z",
    "isArchived": false
  }
]
```

**Error Responses:**

- `401 Unauthorized` - Missing or invalid authentication token

---

### Create Direct Conversation

Create a 1:1 conversation with another user. Idempotent - returns existing conversation if already exists.

**Endpoint:** `POST /conversations/direct`

**Authentication:** Required

**Request Body:**

```json
{
  "otherUserId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:** `201 Created`

```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Error Responses:**

- `400 Bad Request` - Invalid user ID or validation error
- `401 Unauthorized` - Missing or invalid authentication token

---

### Create Group Conversation

Create a group conversation with multiple participants.

**Endpoint:** `POST /conversations/group`

**Authentication:** Required

**Request Body:**

```json
{
  "title": "Project Team Chat",
  "participantIds": [
    "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "4fa85f64-5717-4562-b3fc-2c963f66afa7"
  ]
}
```

**Validation:**

- Title: optional, max 200 characters
- Participants: minimum 2 participants required
- Creator is automatically added as participant

**Response:** `201 Created`

```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Error Responses:**

- `400 Bad Request` - Validation error (insufficient participants, invalid title)
- `401 Unauthorized` - Missing or invalid authentication token

---

### Add Participant to Group

Add a new participant to an existing group conversation.

**Endpoint:** `POST /conversations/{id}/participants`

**Authentication:** Required

**Path Parameters:**

- `id` (UUID) - Conversation ID

**Request Body:**

```json
{
  "participantId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:** `200 OK`

```json
{
  "message": "Participant added successfully"
}
```

**Error Responses:**

- `400 Bad Request` - Not a group conversation, participant already exists
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Conversation not found

---

### Remove Participant from Group

Remove a participant from a group conversation. Minimum 2 participants must remain.

**Endpoint:** `DELETE /conversations/{id}/participants/{participantId}`

**Authentication:** Required

**Path Parameters:**

- `id` (UUID) - Conversation ID
- `participantId` (UUID) - Participant ID to remove

**Response:** `200 OK`

```json
{
  "message": "Participant removed successfully"
}
```

**Error Responses:**

- `400 Bad Request` - Not a group conversation, would leave < 2 participants
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Conversation not found

---

## 💬 Messages API

### Get Messages

Get paginated messages for a conversation, ordered by sent time (newest first).

**Endpoint:** `GET /messages`

**Authentication:** Required

**Query Parameters:**

- `conversationId` (UUID, required) - Conversation ID
- `page` (integer, optional, default: 1) - Page number
- `pageSize` (integer, optional, default: 50, max: 100) - Items per page

**Response:** `200 OK`

```json
{
  "items": [
    {
      "messageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "conversationId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
      "senderId": "5fa85f64-5717-4562-b3fc-2c963f66afa8",
      "content": "Hello, world!",
      "type": "Text|File|Image|Video|System",
      "sentAt": "2026-02-07T10:30:00Z",
      "editedAt": null,
      "isDeleted": false,
      "replyToMessageId": null,
      "attachments": [],
      "reactions": []
    }
  ],
  "totalCount": 150,
  "page": 1,
  "pageSize": 50,
  "totalPages": 3
}
```

**Error Responses:**

- `400 Bad Request` - Invalid pagination parameters
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Conversation not found

---

### Send Text Message

Send a text message to a conversation.

**Endpoint:** `POST /messages`

**Authentication:** Required

**Request Body:**

```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "content": "Hello, world!",
  "replyToMessageId": "4fa85f64-5717-4562-b3fc-2c963f66afa7"
}
```

**Validation:**

- Content: required, max 2000 characters
- ReplyToMessageId: optional, must exist in same conversation

**Response:** `201 Created`

```json
{
  "messageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sentAt": "2026-02-07T10:30:00Z"
}
```

**Error Responses:**

- `400 Bad Request` - Validation error (empty content, invalid reply)
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - User is not a participant in the conversation
- `404 Not Found` - Conversation not found

---

### Upload File

Upload a file for use in chat messages. Returns a file URL for later use in sending messages with attachments.

**Endpoint:** `POST /messages/upload`

**Authentication:** Required

**Request:** `multipart/form-data`

- `file` (file, required) - File to upload

**Validation:**

- Max file size: 50 MB
- Allowed types: images, documents, videos, audio, archives (25+ MIME types)
- File name: max 255 characters

**Response:** `200 OK`

```json
{
  "fileName": "document.pdf",
  "fileUrl": "http://localhost:5000/uploads/chat/3fa85f64-5717-4562-b3fc-2c963f66afa6.pdf",
  "fileSize": 1048576,
  "contentType": "application/pdf"
}
```

**Error Responses:**

- `400 Bad Request` - No file provided, file too large, unsupported type
- `401 Unauthorized` - Missing or invalid authentication token

---

### Send Message with Attachments

Send a message with file attachments (images, documents, videos).

**Endpoint:** `POST /messages/with-attachments`

**Authentication:** Required

**Request Body:**

```json
{
  "conversationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "content": "Check out these files",
  "attachments": [
    {
      "fileName": "document.pdf",
      "fileUrl": "http://localhost:5000/uploads/chat/file1.pdf",
      "fileSize": 1048576,
      "mimeType": "application/pdf",
      "thumbnailUrl": null
    }
  ],
  "replyToMessageId": null
}
```

**Validation:**

- Content: optional (can send attachments without text)
- Attachments: required, max 10 attachments per message
- Each attachment: fileName, fileUrl, fileSize > 0, mimeType required
- Message type auto-detected from MIME type (Image/Video/File)

**Response:** `201 Created`

```json
{
  "messageId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "sentAt": "2026-02-07T10:30:00Z"
}
```

**Error Responses:**

- `400 Bad Request` - Validation error (max attachments exceeded, invalid file data)
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - User is not a participant in the conversation
- `404 Not Found` - Conversation not found

---

### Add Reaction to Message

Add an emoji reaction to a message.

**Endpoint:** `POST /messages/{messageId}/reactions`

**Authentication:** Required

**Path Parameters:**

- `messageId` (UUID) - Message ID

**Request Body:**

```json
{
  "emoji": "👍"
}
```

**Supported Emojis (20 total):**

- 👍 👎 ❤️ 😂 😮 😢 😡 🎉 🔥 👏
- ✅ ❌ ⭐ 💯 🙏 💪 👀 🤔 😍 🥳

**Response:** `200 OK`

```json
{
  "success": true
}
```

**Error Responses:**

- `400 Bad Request` - Invalid emoji (not in supported list)
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Message not found

---

### Remove Reaction from Message

Remove an emoji reaction from a message.

**Endpoint:** `DELETE /messages/{messageId}/reactions/{emoji}`

**Authentication:** Required

**Path Parameters:**

- `messageId` (UUID) - Message ID
- `emoji` (string) - URL-encoded emoji to remove (e.g., `%F0%9F%91%8D` for 👍)

**Response:** `200 OK`

```json
{
  "success": true
}
```

**Error Responses:**

- `400 Bad Request` - Reaction not found for this user
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Message not found

---

### Mark Message as Read

Mark a message as read by the authenticated user. Idempotent operation.

**Endpoint:** `POST /messages/{messageId}/read`

**Authentication:** Required

**Path Parameters:**

- `messageId` (UUID) - Message ID

**Request Body:** None (empty body)

**Response:** `200 OK`

```json
{
  "success": true
}
```

**Error Responses:**

- `400 Bad Request` - Message is deleted
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Message not found

---

### Get Read Receipts

Get list of users who have read a message, with timestamps.

**Endpoint:** `GET /messages/{messageId}/read-receipts`

**Authentication:** Required

**Path Parameters:**

- `messageId` (UUID) - Message ID

**Response:** `200 OK`

```json
[
  {
    "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "readAt": "2026-02-07T10:35:00Z"
  },
  {
    "userId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "readAt": "2026-02-07T10:36:00Z"
  }
]
```

**Note:** Results are ordered by `readAt` timestamp (oldest first).

**Error Responses:**

- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Message not found

---

## 📢 Channels API

### Get Public Channels

Discover all public channels available for joining.

**Endpoint:** `GET /channels/public`

**Authentication:** Required

**Query Parameters:** None

**Response:** `200 OK`

```json
[
  {
    "channelId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "General Announcements",
    "description": "University-wide announcements",
    "type": "Public",
    "ownerId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "memberCount": 150,
    "createdAt": "2026-01-01T00:00:00Z",
    "isArchived": false
  }
]
```

**Error Responses:**

- `401 Unauthorized` - Missing or invalid authentication token

---

### Get My Channels

Get all channels where the authenticated user is a member.

**Endpoint:** `GET /channels/my-channels`

**Authentication:** Required

**Query Parameters:** None

**Response:** `200 OK`

```json
[
  {
    "channelId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "CS Department",
    "description": "Computer Science department channel",
    "type": "Private",
    "ownerId": "4fa85f64-5717-4562-b3fc-2c963f66afa7",
    "memberCount": 45,
    "isModerator": false,
    "isOwner": false,
    "createdAt": "2026-01-15T00:00:00Z",
    "isArchived": false
  }
]
```

**Error Responses:**

- `401 Unauthorized` - Missing or invalid authentication token

---

### Create Channel

Create a new public or private channel. Creator is automatically added as owner, moderator, and member.

**Endpoint:** `POST /channels`

**Authentication:** Required

**Request Body:**

```json
{
  "name": "Study Group",
  "description": "Group for studying algorithms",
  "isPublic": true
}
```

**Validation:**

- Name: required, 3-100 characters
- Description: optional, max 500 characters
- IsPublic: required (true = public, false = private)

**Response:** `201 Created`

```json
{
  "channelId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Error Responses:**

- `400 Bad Request` - Validation error (invalid name length, etc.)
- `401 Unauthorized` - Missing or invalid authentication token

---

### Join Channel

Join a public channel or private channel (requires invitation/permission).

**Endpoint:** `POST /channels/{id}/join`

**Authentication:** Required

**Path Parameters:**

- `id` (UUID) - Channel ID

**Request Body:** None (empty body)

**Response:** `200 OK`

```json
{
  "message": "Successfully joined channel"
}
```

**Error Responses:**

- `400 Bad Request` - Already a member, private channel without permission
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Channel not found

---

### Leave Channel

Leave a channel. Owner cannot leave their own channel.

**Endpoint:** `POST /channels/{id}/leave`

**Authentication:** Required

**Path Parameters:**

- `id` (UUID) - Channel ID

**Request Body:** None (empty body)

**Response:** `200 OK`

```json
{
  "message": "Successfully left channel"
}
```

**Error Responses:**

- `400 Bad Request` - Not a member, or owner attempting to leave
- `401 Unauthorized` - Missing or invalid authentication token
- `404 Not Found` - Channel not found

---

### Add Moderator

Promote a channel member to moderator. Only channel owner can perform this action.

**Endpoint:** `POST /channels/{id}/moderators`

**Authentication:** Required

**Path Parameters:**

- `id` (UUID) - Channel ID

**Request Body:**

```json
{
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6"
}
```

**Response:** `200 OK`

```json
{
  "message": "Moderator added successfully"
}
```

**Error Responses:**

- `400 Bad Request` - User not a member, already a moderator
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Requester is not the channel owner
- `404 Not Found` - Channel not found

---

### Remove Moderator

Demote a moderator to regular member. Only channel owner can perform this action.

**Endpoint:** `DELETE /channels/{id}/moderators/{moderatorId}`

**Authentication:** Required

**Path Parameters:**

- `id` (UUID) - Channel ID
- `moderatorId` (UUID) - User ID to demote

**Response:** `200 OK`

```json
{
  "message": "Moderator removed successfully"
}
```

**Error Responses:**

- `400 Bad Request` - User is not a moderator
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Requester is not the channel owner
- `404 Not Found` - Channel not found

---

### Update Channel

Update channel name and/or description. Only moderators and owner can perform this action.

**Endpoint:** `PUT /channels/{id}`

**Authentication:** Required

**Path Parameters:**

- `id` (UUID) - Channel ID

**Request Body:**

```json
{
  "name": "Updated Channel Name",
  "description": "Updated description"
}
```

**Validation:**

- Name: optional, 3-100 characters if provided
- Description: optional, max 500 characters if provided

**Response:** `200 OK`

```json
{
  "message": "Channel updated successfully"
}
```

**Error Responses:**

- `400 Bad Request` - Validation error (invalid name length, etc.)
- `401 Unauthorized` - Missing or invalid authentication token
- `403 Forbidden` - Requester is not a moderator or owner
- `404 Not Found` - Channel not found

---

## 📦 Schemas

All schemas below describe the `data` object inside the `ApiResponse<T>` envelope.

### `ConversationResponse`
- `id` (guid)
- `type` (string)
- `participantIds` (guid[])
- `lastMessageAt` (datetime | null)
- `createdAt` (datetime)
- `isArchived` (boolean)

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
- `replyToMessageId` (guid | null)

### `SendMessageWithAttachmentsRequest`
- `conversationId` (guid)
- `content` (string | null)
- `attachments` (AttachmentRequest[])
- `replyToMessageId` (guid | null)

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

---

## ❌ Error Responses

All endpoints follow consistent error response format:

### General Error Format

```json
{
  "error": "Human-readable error message"
}
```

### Common HTTP Status Codes

| Status Code                 | Description                             |
| --------------------------- | --------------------------------------- |
| `200 OK`                    | Request successful                      |
| `201 Created`               | Resource created successfully           |
| `400 Bad Request`           | Validation error or invalid input       |
| `401 Unauthorized`          | Missing or invalid authentication token |
| `403 Forbidden`             | User lacks permission for this action   |
| `404 Not Found`             | Resource not found                      |
| `500 Internal Server Error` | Server error (contact support)          |

---

## 🔐 Authentication

All endpoints require JWT Bearer token authentication.

**Header Format:**

```http
Authorization: Bearer <your-jwt-token>
```

**Token Claims:**

- `sub` or `nameid` - User ID (extracted for operations)
- Token must be valid and not expired

**Obtaining Tokens:**
Tokens are obtained through the Identity Module's authentication endpoints (see Identity API documentation).

---

## 📊 Rate Limiting

**Current Status:** No rate limiting implemented

**Future Plans:**

- 100 requests per minute per user for message endpoints
- 20 requests per minute for file upload endpoints
- 200 requests per minute for read-only endpoints

---

## 🚀 Real-Time Features (SignalR)

While this document covers REST APIs, the Chat Module also provides real-time messaging via SignalR Hub.

**SignalR Hub Endpoint:** `/hubs/chat`

**Features:**

- Real-time message delivery
- Typing indicators
- User presence (online/offline)
- Message reactions in real-time
- Read receipts notifications

For detailed SignalR client implementation, see [SignalR Hub Documentation](../architecture/signalr-redis-backplane.md).

---

## 📝 Postman Collection

A Postman collection with all endpoints and example requests is available:

**Location:** `docs/api/postman/UniHub-Chat-API.postman_collection.json`

**Import Steps:**

1. Open Postman
2. Click Import → Upload Files
3. Select the JSON file
4. Configure environment variables (API URL, JWT token)

---

## 🆕 Changelog

### Version 1.0.0 (2026-02-07)

- ✅ Initial release with all 21 endpoints
- ✅ Conversations API (5 endpoints)
- ✅ Messages API (8 endpoints)
- ✅ Channels API (8 endpoints)
- ✅ Authentication and error handling
- ✅ File upload support (max 50MB, 25+ types)
- ✅ Reactions (20 supported emojis)
- ✅ Read receipts tracking

---

## 📞 Support

For API issues or questions:

- **Documentation:** [GitHub Wiki](https://github.com/your-org/unihub/wiki)
- **Issues:** [GitHub Issues](https://github.com/your-org/unihub/issues)
- **Contact:** dev@unihub.edu

---

_Last Updated: 2026-02-07_
_API Version: 1.0.0_
