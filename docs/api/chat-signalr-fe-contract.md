# Chat SignalR — contract cho frontend

**Hub path:** `GET/WS` cùng origin API, path `/hubs/chat` (xem `getChatHubUrl()`).  
**Auth:** `accessTokenFactory` gửi JWT (header do client SignalR cấu hình).  
**Giao thức client JS:** tên phương thức từ server thường ở dạng **camelCase** (ví dụ `receiveMessage`).

## 1. Gọi từ client → server (`Hub` public methods)

| Tên (C#) | Tên gợi ý từ JS | Tham số |
|----------|------------------|---------|
| `JoinConversation` | `joinConversation` | `conversationId: string` (GUID) |
| `LeaveConversation` | `leaveConversation` | `conversationId: string` |
| `SendMessage` | `sendMessage` | `conversationId`, `content`, `messageType`, `replyToMessageId?` |
| `SendTypingIndicator` | `sendTypingIndicator` | `conversationId`, `isTyping: boolean` |
| `JoinChannel` | `joinChannel` | `channelId: string` |
| `LeaveChannel` | `leaveChannel` | `channelId: string` |
| `SendChannelMessage` | `sendChannelMessage` | `channelId`, `content`, `messageType` |
| `AddReaction` | `addReaction` | `messageId`, `conversationId`, `emoji` |
| `RemoveReaction` | `removeReaction` | `messageId`, `conversationId`, `emoji` |
| `MarkAsRead` / tương đương | *(xem `ChatHub.cs`)* | … |
| `RelayWebRtcSignal` | `relayWebRtcSignal` | `conversationId`, `targetUserId`, `kind`, `payload` — `kind`: `offer` \| `answer` \| `ice` \| `hangup` (chuỗi). SDP/ICE trong `payload` (JSON string). |

**WebRTC relay:** Khi **đích không có connection SignalR** (offline / không vào hub): server **`throw HubException("webrtc_peer_offline")`** chỉ cho **`offer`** và **`answer`**. Với **`ice`** và **`hangup`** thì **không throw** (best-effort).

> Gọi từ FE: `connection.invoke('joinConversation', id)` — nếu lỗi, kiểm tra đúng casing bằng frame WebSocket trong DevTools.

## 2. Server gọi xuống client (`IChatClient`)

Nguồn: `IChatClient.cs`. Handler đăng ký: `connection.on('<camelCase>', fn)`.

| C# method | Gợi ý sự kiện JS | Payload (JSON, camelCase) | Hành vi FE (v1) |
|-----------|------------------|---------------------------|-----------------|
| `ReceiveMessage` | `receiveMessage` | `messageId`, `conversationId`, `channelId?`, `senderId`, `senderName`, `content`, `messageType`, `sentAt`, `replyToMessageId?` | Invalidate `ChatMessage` + list; cập nhật transcript kênh; unread + notification |
| `MessageEdited` | `messageEdited` | `messageId`, `conversationId`, `newContent`, `editedAt` | Invalidate thread + list |
| `MessageDeleted` | `messageDeleted` | `messageId`, `conversationId`, `deletedAt` | Giống trên |
| `UserJoined` | `userJoined` | `userId`, `userName`, `conversationId?`, `channelId?`, `joinedAt` | Invalidate `ChatConversation` hoặc `ChatChannel` list |
| `UserLeft` | `userLeft` | `userId`, `userName`, `conversationId?`, `channelId?`, `leftAt` | Giống trên |
| `UserTyping` | `userTyping` | `userId`, `userName`, `conversationId`, `isTyping` | Cập nhật dòng “đang nhập” (chỉ hội thoại) |
| `ReactionAdded` | `reactionAdded` | `messageId`, `conversationId`, `userId`, `userName`, `emoji`, `timestamp` | Invalidate thread |
| `ReactionRemoved` | `reactionRemoved` | *(cùng shape với thêm)* | Invalidate thread |
| `MessageRead` | `messageRead` | `messageId`, `conversationId`, `userId`, `readAt` | Invalidate thread |
| `ChannelUpdated` | `channelUpdated` | `channelId`, `newName`, `newDescription?`, `updatedAt` | Invalidate list kênh |
| `UserStatusChanged` | `userStatusChanged` | `userId`, `userName`, `status`, `timestamp` | Invalidate list hội thoại (hiện tại) |
| `ReceiveWebRtcSignal` | `receiveWebRtcSignal` | `conversationId`, `fromUserId`, `fromUserName`, `kind`, `payload` | Truyền tới peer để WebRTC (SDP/ICE/hangup); media vẫn P2P |

## 3. Ràng buộc sản phẩm (đã ghi ở plan)

- **Kênh:** lịch sử REST vẫn chưa gắn `conversationId` — UI kênh dùng **realtime + transcript bộ nhớ**; `GET /messages?channelId=` sẽ là bước BE sau.  
- **Guid rỗng:** `00000000-0000-0000-0000-000000000000` thường nghĩa là “không áp dụng” (ví dụ tin kênh trước bản sửa BE).
- **WebRTC relay:** `ConnectionManager` theo dõi connection **trong bộ nhớ từng instance API**. Scale-out nhiều pod: người dùng có thể “online” trên instance khác → relay báo offline dù họ đang mở app; cần sticky session hoặc registry tập trung nếu triển khai đa instance.

## 4. Tham chiếu mã nguồn

- Server: `src/Modules/Chat/UniHub.Chat.Presentation/Hubs/ChatHub.cs`, `IChatClient.cs`
- Client: `frontend/src/features/chat/lib/chatHub.ts`, `mapHubMessage.ts`, `context/ChatContext.tsx`
