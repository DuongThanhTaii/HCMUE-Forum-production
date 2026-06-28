# TASK-108: Chat & AI Bot Module

> **Real-time messaging with SignalR, AI chatbot integration**

---

## 📋 TASK INFO

| Property         | Value                             |
| ---------------- | --------------------------------- |
| **Task ID**      | TASK-108                          |
| **Module**       | Chat + AI Bot                     |
| **Status**       | ⬜ NOT_STARTED                    |
| **Priority**     | 🔴 Critical                       |
| **Estimate**     | 14 hours                          |
| **Branch**       | `feature/TASK-108-chat-ai-module` |
| **Dependencies** | TASK-104 (Auth), TASK-105 (Layout) |

---

## 🎯 OBJECTIVES

- Implement real-time chat with SignalR
- Build conversations list and chat window
- Add typing indicators and online status
- Support file uploads in chat
- Integrate UniBot AI assistant
- Implement channels (public chat rooms)
- Add emoji reactions and read receipts

---

## 📡 BACKEND INTEGRATION

### SignalR Hub Endpoints

**ChatHub**: `wss://api.unihub.example/hubs/chat`

**Server Methods (Client → Server)**:
- `JoinConversation(conversationId)` — Join a conversation room
- `LeaveConversation(conversationId)` — Leave a conversation room
- `SendMessage(conversationId, content, messageType, replyToMessageId?)` — Send message
- `SendTypingIndicator(conversationId, isTyping)` — Send typing status
- `AddReaction(messageId, emoji)` — Add emoji reaction
- `RemoveReaction(message Id, emoji)` — Remove reaction
- `MarkAsRead(messageId)` — Mark message as read

**Client Events (Server → Client)**:
- `ReceiveMessage(message)` — New message received
- `MessageEdited(messageId, newContent)` — Message edited
- `MessageDeleted(messageId)` — Message deleted
- `UserJoined(conversationId, userId)` — User joined conversation
- `UserLeft(conversationId, userId)` — User left conversation
- `UserTyping(conversationId, userId, isTyping)` — Typing indicator
- `ReactionAdded(messageId, userId, emoji)` — Reaction added
- `ReactionRemoved(messageId, userId, emoji)` — Reaction removed
- `MessageRead(messageId, userId)` — Message read receipt
- `UserStatusChanged(userId, status)` — User online/offline

### REST API Endpoints

**Conversations**:
```http
GET /api/v1/chat/conversations → Get user's conversations
POST /api/v1/chat/conversations/direct → Create direct message
POST /api/v1/chat/conversations/group → Create group chat
POST /api/v1/chat/conversations/{id}/participants → Add participant
DELETE /api/v1/chat/conversations/{id}/participants/{participantId} → Remove participant
```

**Messages**:
```http
GET /api/v1/chat/messages?conversationId={id}&page=1&pageSize=50 → Get messages
POST /api/v1/chat/messages → Send text message
POST /api/v1/chat/messages/upload → Upload file
POST /api/v1/chat/messages/with-attachments → Send with attachments
GET /api/v1/chat/messages/{id}/read-receipts → Get read receipts
```

**Channels**:
```http
GET /api/v1/chat/channels/public → Get public channels
GET /api/v1/chat/channels/my-channels → Get joined channels
POST /api/v1/chat/channels → Create channel
POST /api/v1/chat/channels/{id}/join → Join channel
POST /api/v1/chat/channels/{id}/leave → Leave channel
```

**AI Bot**:
```http
POST /api/v1/ai/chat → Send message to UniBot
GET /api/v1/ai/conversations → Get AI conversations
GET /api/v1/ai/conversations/{id} → Get AI conversation details
```

---

## 📁 FILES TO CREATE

### 1. SignalR Connection Manager

**File**: `src/lib/signalr/connection.ts`

```typescript
import * as signalR from '@microsoft/signalr';
import { useAuthStore } from '@/stores/auth.store';

export class SignalRConnection {
  private connection: signalR.HubConnection | null = null;
  private reconnectRetries = 0;
  private maxReconnectRetries = 5;

  constructor(private hubUrl: string) {}

  async start(): Promise<void> {
    if (this.connection?.state === signalR.HubConnectionState.Connected) {
      return;
    }

    const { accessToken } = useAuthStore.getState();
    if (!accessToken) {
      throw new Error('No access token available');
    }

    this.connection = new signalR.HubConnectionBuilder()
      .withUrl(this.hubUrl, {
        accessTokenFactory: () => accessToken,
        transport: signalR.HttpTransportType.WebSockets,
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: (retryContext) => {
          if (retryContext.previousRetryCount >= this.maxReconnectRetries) {
            return null; // Stop retrying
          }
          return Math.min(1000 * Math.pow(2, retryContext.previousRetryCount), 30000);
        },
      })
      .configureLogging(signalR.LogLevel.Information)
      .build();

    this.connection.onreconnecting((error) => {
      console.warn('SignalR reconnecting:', error);
      this.reconnectRetries++;
    });

    this.connection.onreconnected(() => {
      console.log('SignalR reconnected');
      this.reconnectRetries = 0;
    });

    this.connection.onclose((error) => {
      console.error('SignalR connection closed:', error);
    });

    try {
      await this.connection.start();
      console.log('SignalR connected to', this.hubUrl);
    } catch (error) {
      console.error('SignalR connection failed:', error);
      throw error;
    }
  }

  async stop(): Promise<void> {
    if (this.connection) {
      await this.connection.stop();
      this.connection = null;
    }
  }

  on(eventName: string, callback: (...args: any[]) => void): void {
    if (!this.connection) {
      throw new Error('Connection not started');
    }
    this.connection.on(eventName, callback);
  }

  off(eventName: string, callback: (...args: any[]) => void): void {
    if (!this.connection) return;
    this.connection.off(eventName, callback);
  }

  async invoke(methodName: string, ...args: any[]): Promise<any> {
    if (!this.connection || this.connection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('SignalR not connected');
    }
    return await this.connection.invoke(methodName, ...args);
  }

  get state(): signalR.HubConnectionState | null {
    return this.connection?.state || null;
  }
}
```

**File**: `src/lib/signalr/chatHub.ts`

```typescript
import { SignalRConnection } from './connection';

const CHAT_HUB_URL = `${process.env.NEXT_PUBLIC_SIGNALR_URL}/chat`;

export class ChatHub {
  private connection: SignalRConnection;

  constructor() {
    this.connection = new SignalRConnection(CHAT_HUB_URL);
  }

  async start() {
    await this.connection.start();
  }

  async stop() {
    await this.connection.stop();
  }

  // Server methods
  async joinConversation(conversationId: string) {
    return await this.connection.invoke('JoinConversation', conversationId);
  }

  async leaveConversation(conversationId: string) {
    return await this.connection.invoke('LeaveConversation', conversationId);
  }

  async sendMessage(
    conversationId: string,
    content: string,
    messageType: 'Text' | 'File',
    replyToMessageId?: string
  ) {
    return await this.connection.invoke(
      'SendMessage',
      conversationId,
      content,
      messageType,
      replyToMessageId
    );
  }

  async sendTypingIndicator(conversationId: string, isTyping: boolean) {
    return await this.connection.invoke('SendTypingIndicator', conversationId, isTyping);
  }

  async addReaction(messageId: string, emoji: string) {
    return await this.connection.invoke('AddReaction', messageId, emoji);
  }

  async removeReaction(messageId: string, emoji: string) {
    return await this.connection.invoke('RemoveReaction', messageId, emoji);
  }

  async markAsRead(messageId: string) {
    return await this.connection.invoke('MarkAsRead', messageId);
  }

  // Client events
  onReceiveMessage(callback: (message: any) => void) {
    this.connection.on('ReceiveMessage', callback);
  }

  onMessageEdited(callback: (messageId: string, newContent: string) => void) {
    this.connection.on('MessageEdited', callback);
  }

  onMessageDeleted(callback: (messageId: string) => void) {
    this.connection.on('MessageDeleted', callback);
  }

  onUserTyping(callback: (conversationId: string, userId: string, isTyping: boolean) => void) {
    this.connection.on('UserTyping', callback);
  }

  onReactionAdded(callback: (messageId: string, userId: string, emoji: string) => void) {
    this.connection.on('ReactionAdded', callback);
  }

  onUserStatusChanged(callback: (userId: string, status: string) => void) {
    this.connection.on('UserStatusChanged', callback);
  }

  // Remove event listeners
  offReceiveMessage(callback: (message: any) => void) {
    this.connection.off('ReceiveMessage', callback);
  }

  get state() {
    return this.connection.state;
  }
}

// Singleton instance
let chatHubInstance: ChatHub | null = null;

export function getChatHub(): ChatHub {
  if (!chatHubInstance) {
    chatHubInstance = new ChatHub();
  }
  return chatHubInstance;
}
```

### 2. Chat Store (Zustand)

**File**: `src/stores/chat.store.ts`

```typescript
import { create } from 'zustand';

export interface Message {
  id: string;
  conversationId: string;
  senderId: string;
  senderName: string;
  senderAvatar?: string;
  content: string;
  messageType: 'Text' | 'File';
  fileUrl?: string;
  fileName?: string;
  replyTo?: {
    id: string;
    content: string;
    senderName: string;
  };
  reactions: Array<{ emoji: string; userIds: string[] }>;
  readBy: string[];
  createdAt: string;
  editedAt?: string;
}

export interface Conversation {
  id: string;
  name: string;
  type: 'Direct' | 'Group';
  participants: Array<{
    userId: string;
    userName: string;
    avatar?: string;
    isOnline: boolean;
  }>;
  lastMessage?: Message;
  unreadCount: number;
  createdAt: string;
}

interface ChatState {
  conversations: Conversation[];
  messages: Record<string, Message[]>; // conversationId → messages[]
  activeConversationId: string | null;
  typingUsers: Record<string, string[]>; // conversationId → userIds[]
  onlineUsers: string[];

  // Actions
  setConversations: (conversations: Conversation[]) => void;
  addConversation: (conversation: Conversation) => void;
  setActiveConversation: (conversationId: string | null) => void;
  setMessages: (conversationId: string, messages: Message[]) => void;
  addMessage: (message: Message) => void;
  updateMessage: (messageId: string, updates: Partial<Message>) => void;
  deleteMessage: (messageId: string) => void;
  setTyping: (conversationId: string, userId: string, isTyping: boolean) => void;
  setUserOnline: (userId: string, isOnline: boolean) => void;
  incrementUnread: (conversationId: string) => void;
  clearUnread: (conversationId: string) => void;
}

export const useChatStore = create<ChatState>((set, get) => ({
  conversations: [],
  messages: {},
  activeConversationId: null,
  typingUsers: {},
  onlineUsers: [],

  setConversations: (conversations) => set({ conversations }),

  addConversation: (conversation) =>
    set((state) => ({
      conversations: [conversation, ...state.conversations],
    })),

  setActiveConversation: (conversationId) => set({ activeConversationId: conversationId }),

  setMessages: (conversationId, messages) =>
    set((state) => ({
      messages: { ...state.messages, [conversationId]: messages },
    })),

  addMessage: (message) =>
    set((state) => {
      const conversationMessages = state.messages[message.conversationId] || [];
      return {
        messages: {
          ...state.messages,
          [message.conversationId]: [...conversationMessages, message],
        },
        conversations: state.conversations.map((conv) =>
          conv.id === message.conversationId
            ? { ...conv, lastMessage: message }
            : conv
        ),
      };
    }),

  updateMessage: (messageId, updates) =>
    set((state) => {
      const newMessages = { ...state.messages };
      for (const convId in newMessages) {
        newMessages[convId] = newMessages[convId].map((msg) =>
          msg.id === messageId ? { ...msg, ...updates } : msg
        );
      }
      return { messages: newMessages };
    }),

  deleteMessage: (messageId) =>
    set((state) => {
      const newMessages = { ...state.messages };
      for (const convId in newMessages) {
        newMessages[convId] = newMessages[convId].filter((msg) => msg.id !== messageId);
      }
      return { messages: newMessages };
    }),

  setTyping: (conversationId, userId, isTyping) =>
    set((state) => {
      const currentTyping = state.typingUsers[conversationId] || [];
      const newTyping = isTyping
        ? [...new Set([...currentTyping, userId])]
        : currentTyping.filter((id) => id !== userId);

      return {
        typingUsers: {
          ...state.typingUsers,
          [conversationId]: newTyping,
        },
      };
    }),

  setUserOnline: (userId, isOnline) =>
    set((state) => ({
      onlineUsers: isOnline
        ? [...new Set([...state.onlineUsers, userId])]
        : state.onlineUsers.filter((id) => id !== userId),
      conversations: state.conversations.map((conv) => ({
        ...conv,
        participants: conv.participants.map((p) =>
          p.userId === userId ? { ...p, isOnline } : p
        ),
      })),
    })),

  incrementUnread: (conversationId) =>
    set((state) => ({
      conversations: state.conversations.map((conv) =>
        conv.id === conversationId ? { ...conv, unreadCount: conv.unreadCount + 1 } : conv
      ),
    })),

  clearUnread: (conversationId) =>
    set((state) => ({
      conversations: state.conversations.map((conv) =>
        conv.id === conversationId ? { ...conv, unreadCount: 0 } : conv
      ),
    })),
}));
```

### 3. Chat Hooks

**File**: `src/hooks/realtime/useChatHub.ts`

```typescript
'use client';

import { useEffect, useRef } from 'react';
import { getChatHub } from '@/lib/signalr/chatHub';
import { useChatStore } from '@/stores/chat.store';
import { useAuth } from '@/hooks/auth/useAuth';
import { toast } from 'sonner';

export function useChatHub() {
  const { isAuthenticated, user } = useAuth();
  const chatHubRef = useRef(getChatHub());
  const {
    addMessage,
    updateMessage,
    deleteMessage,
    setTyping,
    setUserOnline,
    incrementUnread,
    activeConversationId,
  } = useChatStore();

  useEffect(() => {
    if (!isAuthenticated) return;

    const chatHub = chatHubRef.current;

    // Start connection
    chatHub.start().catch((error) => {
      console.error('Failed to start ChatHub:', error);
      toast.error('Failed to connect to chat server');
    });

    // Setup event listeners
    chatHub.onReceiveMessage((message) => {
      addMessage(message);
      if (message.conversationId !== activeConversationId) {
        incrementUnread(message.conversationId);
      }
    });

    chatHub.onMessageEdited((messageId, newContent) => {
      updateMessage(messageId, { content: newContent, editedAt: new Date().toISOString() });
    });

    chatHub.onMessageDeleted((messageId) => {
      deleteMessage(messageId);
    });

    chatHub.onUserTyping((conversationId, userId, isTyping) => {
      if (userId !== user?.id) {
        setTyping(conversationId, userId, isTyping);
      }
    });

    chatHub.onUserStatusChanged((userId, status) => {
      setUserOnline(userId, status === 'Online');
    });

    // Cleanup on unmount
    return () => {
      chatHub.stop();
    };
  }, [isAuthenticated]);

  return chatHubRef.current;
}
```

**File**: `src/hooks/api/chat/useConversations.ts`

```typescript
import { useQuery } from '@tanstack/react-query';
import { apiClient } from '@/lib/api/client';
import { useChatStore, type Conversation } from '@/stores/chat.store';

export function useConversations() {
  const setConversations = useChatStore((state) => state.setConversations);

  return useQuery({
    queryKey: ['conversations'],
    queryFn: async () => {
      const response = await apiClient.get<Conversation[]>('/api/v1/chat/conversations');
      setConversations(response.data);
      return response.data;
    },
  });
}
```

### 4. Chat Components

**File**: `src/components/features/chat/ChatWindow.tsx`

```tsx
'use client';

import { useEffect, useRef, useState } from 'react';
import { useChatStore } from '@/stores/chat.store';
import { MessageBubble } from './MessageBubble';
import { MessageInput } from './MessageInput';
import { TypingIndicator } from './TypingIndicator';
import { useAuth } from '@/hooks/auth/useAuth';
import { useChatHub } from '@/hooks/realtime/useChatHub';
import { ScrollArea } from '@/components/ui/scroll-area';
import { Skeleton } from '@/components/ui/skeleton';

interface ChatWindowProps {
  conversationId: string;
}

export function ChatWindow({ conversationId }: ChatWindowProps) {
  const { user } = useAuth();
  const chatHub = useChatHub();
  const messagesEndRef = useRef<HTMLDivElement>(null);
  const [isJoined, setIsJoined] = useState(false);

  const messages = useChatStore((state) => state.messages[conversationId] || []);
  const typingUsers = useChatStore((state) => state.typingUsers[conversationId] || []);
  const clearUnread = useChatStore((state) => state.clearUnread);

  useEffect(() => {
    // Join conversation
    chatHub.joinConversation(conversationId).then(() => {
      setIsJoined(true);
      clearUnread(conversationId);
    });

    return () => {
      chatHub.leaveConversation(conversationId);
    };
  }, [conversationId]);

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  if (!isJoined) {
    return (
      <div className="flex h-full flex-col space-y-4 p-4">
        {Array.from({ length: 5 }).map((_, i) => (
          <Skeleton key={i} className="h-16 w-full" />
        ))}
      </div>
    );
  }

  return (
    <div className="flex h-full flex-col">
      <ScrollArea className="flex-1 p-4">
        <div className="space-y-4">
          {messages.map((message) => (
            <MessageBubble
              key={message.id}
              message={message}
              isOwnMessage={message.senderId === user?.id}
            />
          ))}
          {typingUsers.length > 0 && <TypingIndicator userIds={typingUsers} />}
          <div ref={messagesEndRef} />
        </div>
      </ScrollArea>

      <div className="border-t p-4">
        <MessageInput conversationId={conversationId} />
      </div>
    </div>
  );
}
```

**File**: `src/components/features/chat/MessageBubble.tsx` (Uses GAIA UI)

```tsx
'use client';

import { ChatBubble } from '@/components/ui/chat-bubble'; // GAIA UI component
import { type Message } from '@/stores/chat.store';
import { formatDistanceToNow } from 'date-fns';
import { vi } from 'date-fns/locale';

interface MessageBubbleProps {
  message: Message;
  isOwnMessage: boolean;
}

export function MessageBubble({ message, isOwnMessage }: MessageBubbleProps) {
  return (
    <ChatBubble
      align={isOwnMessage ? 'right' : 'left'}
      variant={isOwnMessage ? 'sender' : 'receiver'}
    >
      <div className="flex flex-col space-y-1">
        {!isOwnMessage && (
          <span className="text-xs font-medium">{message.senderName}</span>
        )}
        
        {message.replyTo && (
          <div className="rounded bg-muted/50 p-2 text-xs">
            <span className="font-medium">{message.replyTo.senderName}</span>
            <p className="text-muted-foreground">{message.replyTo.content}</p>
          </div>
        )}

        <p className="text-sm">{message.content}</p>

        {message.reactions.length > 0 && (
          <div className="flex gap-1">
            {message.reactions.map((reaction, i) => (
              <span key={i} className="text-xs">
                {reaction.emoji} {reaction.userIds.length}
              </span>
            ))}
          </div>
        )}

        <span className="text-xs text-muted-foreground">
          {formatDistanceToNow(new Date(message.createdAt), {
            addSuffix: true,
            locale: vi,
          })}
          {message.editedAt && ' (đã chỉnh sửa)'}
        </span>
      </div>
    </ChatBubble>
  );
}
```

---

## ✅ ACCEPTANCE CRITERIA

### Core Chat
- [ ] Conversations list showing recent chats
- [ ] Real-time message delivery (SignalR)
- [ ] Chat window with scrollable message history
- [ ] Send text messages
- [ ] Upload and send files
- [ ] Reply to messages (quote)
- [ ] Typing indicators working
- [ ] Online/offline status indicators
- [ ] Read receipts displayed
- [ ] Emoji reactions functional

### Channels
- [ ] Public channels list
- [ ] Join/leave channels
- [ ] Channel chat interface
- [ ] Channel member list

### AI Bot (UniBot)
- [ ] Dedicated AI chat page
- [ ] Tool calls visualization (uses GAIA tool-calls-section)
- [ ] AI conversation history
- [ ] Streaming responses (if backend supports)

### Performance
- [ ] Messages pagination (load 50 at a time)
- [ ] Smooth scrolling
- [ ] SignalR auto-reconnect working
- [ ] Offline message queue (send when reconnected)

---

_Last Updated: 2026-02-10_
