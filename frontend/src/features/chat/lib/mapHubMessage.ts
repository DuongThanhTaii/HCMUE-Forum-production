import type { HubMessageNotification } from '../types/chat.types'

const LIST_ID = 'LIST' as const

export function readStr(obj: Record<string, unknown>, camel: string, pascal: string): string {
  const v = obj[camel] ?? obj[pascal]
  return v != null ? String(v) : ''
}

/** Normalize SignalR payload (camelCase or PascalCase JSON). */
export function parseHubMessageNotification(payload: unknown): HubMessageNotification | null {
  if (!payload || typeof payload !== 'object') return null
  const p = payload as Record<string, unknown>
  const messageId = readStr(p, 'messageId', 'MessageId') || readStr(p, 'id', 'Id')
  const conversationId = readStr(p, 'conversationId', 'ConversationId')
  const rawChannel = p.channelId ?? p.ChannelId
  const channelId =
    rawChannel != null && String(rawChannel) !== '00000000-0000-0000-0000-000000000000'
      ? String(rawChannel)
      : undefined
  const senderId = readStr(p, 'senderId', 'SenderId')
  const senderName = readStr(p, 'senderName', 'SenderName')
  const content = readStr(p, 'content', 'Content')
  const messageType = readStr(p, 'messageType', 'MessageType')
  const sentAtRaw = p.sentAt ?? p.SentAt
  const sentAt = sentAtRaw != null ? String(sentAtRaw) : new Date().toISOString()
  const rawReply = p.replyToMessageId ?? p.ReplyToMessageId
  const replyToMessageId =
    rawReply != null && String(rawReply) !== '00000000-0000-0000-0000-000000000000'
      ? String(rawReply)
      : undefined
  if (!messageId) return null
  return {
    messageId,
    conversationId,
    channelId: channelId ?? null,
    senderId,
    senderName,
    content,
    messageType,
    sentAt,
    replyToMessageId: replyToMessageId ?? null,
  }
}

export type ChatInvalidateTag = {
  type: 'ChatConversation' | 'ChatMessage' | 'ChatChannel'
  id: string | number
}

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000'

export function invalidateTagsForConversationThread(conversationId: string): ChatInvalidateTag[] {
  const tags: ChatInvalidateTag[] = [{ type: 'ChatConversation', id: LIST_ID }]
  if (conversationId && conversationId !== EMPTY_GUID) {
    tags.push({ type: 'ChatMessage', id: conversationId })
  }
  return tags
}

export function invalidateTagsChannelDiscovery(): ChatInvalidateTag[] {
  return [{ type: 'ChatChannel', id: LIST_ID }]
}

export function getInvalidateTagsForHubMessage(msg: HubMessageNotification): ChatInvalidateTag[] {
  const tags: ChatInvalidateTag[] = [
    { type: 'ChatConversation', id: LIST_ID },
  ]
  const empty = '00000000-0000-0000-0000-000000000000'
  if (msg.conversationId && msg.conversationId !== empty) {
    tags.push({ type: 'ChatMessage', id: msg.conversationId })
  }
  if (msg.channelId && msg.channelId !== empty) {
    tags.push({ type: 'ChatChannel', id: LIST_ID })
    tags.push({ type: 'ChatMessage', id: `channel:${msg.channelId}` })
  }
  return tags
}

/** Server → client: `UserTyping` / TypingNotification */
export function parseHubUserTyping(
  payload: unknown
): {
  userId: string
  userName: string
  conversationId: string
  isTyping: boolean
} | null {
  if (!payload || typeof payload !== 'object') return null
  const p = payload as Record<string, unknown>
  const userId = readStr(p, 'userId', 'UserId')
  const userName = readStr(p, 'userName', 'UserName')
  const conversationId = readStr(p, 'conversationId', 'ConversationId')
  const isTyping = Boolean(p.isTyping ?? p.IsTyping)
  if (!userId || !conversationId || conversationId === EMPTY_GUID) return null
  return { userId, userName, conversationId, isTyping }
}

/** Server → client: `UserStatusChanged` / UserStatusNotification */
export function parseHubUserStatus(
  payload: unknown
): {
  userId: string
  userName: string
  status: string
  timestamp: string
} | null {
  if (!payload || typeof payload !== 'object') return null
  const p = payload as Record<string, unknown>
  const userId = readStr(p, 'userId', 'UserId')
  if (!userId || userId === EMPTY_GUID) return null
  const userName = readStr(p, 'userName', 'UserName')
  const status = readStr(p, 'status', 'Status')
  const timestampRaw = p.timestamp ?? p.Timestamp
  const timestamp = timestampRaw != null ? String(timestampRaw) : new Date().toISOString()
  return { userId, userName, status, timestamp }
}

export function readConversationIdFromPayload(payload: unknown): string | null {
  if (!payload || typeof payload !== 'object') return null
  const p = payload as Record<string, unknown>
  const id = readStr(p, 'conversationId', 'ConversationId')
  return id && id !== EMPTY_GUID ? id : null
}

export function readChannelIdFromPayload(payload: unknown): string | null {
  if (!payload || typeof payload !== 'object') return null
  const p = payload as Record<string, unknown>
  const raw = p.channelId ?? p.ChannelId
  if (raw == null) return null
  const id = String(raw)
  return id !== EMPTY_GUID ? id : null
}
