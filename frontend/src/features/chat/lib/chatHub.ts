import { io, Socket } from 'socket.io-client'
import type { AppDispatch } from '../../../app/store'
import { chatApi } from '../api/chat.api'
import {
  getInvalidateTagsForHubMessage,
  invalidateTagsChannelDiscovery,
  invalidateTagsForConversationThread,
  parseHubMessageNotification,
  parseHubUserStatus,
  parseHubUserTyping,
  readChannelIdFromPayload,
  readConversationIdFromPayload,
} from './mapHubMessage'
import type { HubMessageNotification, WebRtcSignalPayload } from '../types/chat.types'

export function createChatConnection(getAccessToken: () => string | null): Socket {
  const baseUrl = (import.meta.env.VITE_API_URL as string | undefined)?.replace(/\/$/, '') ?? 'http://localhost:3000'
  return io(`${baseUrl}/chat`, {
    auth: { token: `Bearer ${getAccessToken() ?? ''}` },
    transports: ['websocket', 'polling'],
    reconnectionDelay: 2000,
    reconnectionDelayMax: 10000,
  })
}

export function parseWebRtcSignalPayload(raw: unknown): WebRtcSignalPayload | null {
  if (!raw || typeof raw !== 'object') return null
  const o = raw as Record<string, unknown>
  const conversationId = o.conversationId != null ? String(o.conversationId) : ''
  const fromUserId = o.fromUserId != null ? String(o.fromUserId) : ''
  const kind = o.kind != null ? String(o.kind) : ''
  const payload = o.payload != null ? String(o.payload) : ''
  if (!conversationId || !fromUserId || !kind) return null
  return {
    conversationId,
    fromUserId,
    fromUserName: o.fromUserName != null ? String(o.fromUserName) : '',
    kind: kind.toLowerCase(),
    payload,
  }
}

export type ChatHubHandlers = {
  onReceiveMessage?: (msg: HubMessageNotification) => void
  onUserTyping?: (payload: {
    userId: string
    userName: string
    conversationId: string
    isTyping: boolean
  }) => void
  onUserStatusChanged?: (payload: {
    userId: string
    userName: string
    status: string
    timestamp: string
  }) => void
  onWebRtcSignal?: (payload: WebRtcSignalPayload) => void
}

export function attachChatHubHandlers(
  socket: Socket,
  dispatch: AppDispatch,
  handlers: ChatHubHandlers
): void {
  const onReceive = (payload: unknown) => {
    const msg = parseHubMessageNotification(payload)
    if (!msg) return
    dispatch(chatApi.util.invalidateTags(getInvalidateTagsForHubMessage(msg)))
    handlers.onReceiveMessage?.(msg)
  }

  const onMessageEdited = (payload: unknown) => {
    const cid = readConversationIdFromPayload(payload)
    if (cid) {
      dispatch(chatApi.util.invalidateTags(invalidateTagsForConversationThread(cid)))
    }
  }

  const onUserJoined = (payload: unknown) => {
    const conv = readConversationIdFromPayload(payload)
    const ch = readChannelIdFromPayload(payload)
    if (conv) {
      dispatch(chatApi.util.invalidateTags([{ type: 'ChatConversation', id: 'LIST' }]))
    }
    if (ch) {
      dispatch(chatApi.util.invalidateTags(invalidateTagsChannelDiscovery()))
    }
  }

  const onUserTyping = (payload: unknown) => {
    const t = parseHubUserTyping(payload)
    if (t) handlers.onUserTyping?.(t)
  }

  const onReaction = (payload: unknown) => {
    const cid = readConversationIdFromPayload(payload)
    if (cid) {
      dispatch(chatApi.util.invalidateTags(invalidateTagsForConversationThread(cid)))
    }
  }

  const onMessageRead = (payload: unknown) => {
    const cid = readConversationIdFromPayload(payload)
    if (cid) {
      dispatch(chatApi.util.invalidateTags(invalidateTagsForConversationThread(cid)))
    }
  }

  const onChannelUpdated = () => {
    dispatch(chatApi.util.invalidateTags(invalidateTagsChannelDiscovery()))
  }

  const onUserStatusChanged = (payload: unknown) => {
    const s = parseHubUserStatus(payload)
    if (s) handlers.onUserStatusChanged?.(s)
    dispatch(chatApi.util.invalidateTags([{ type: 'ChatConversation', id: 'LIST' }]))
  }

  const onWebRtcSignal = (payload: unknown) => {
    const p = parseWebRtcSignalPayload(payload)
    if (p) handlers.onWebRtcSignal?.(p)
  }

  socket.on('new_message', onReceive)
  socket.on('ReceiveMessage', onReceive) // Fallback for old mapping if added to backend
  socket.on('MessageEdited', onMessageEdited)
  socket.on('MessageDeleted', onMessageEdited)
  socket.on('UserJoined', onUserJoined)
  socket.on('UserLeft', onUserJoined)
  socket.on('UserTyping', onUserTyping)
  socket.on('message_reaction_added', onReaction)
  socket.on('ReactionAdded', onReaction)
  socket.on('ReactionRemoved', onReaction)
  socket.on('MessageRead', onMessageRead)
  socket.on('ChannelUpdated', onChannelUpdated)
  socket.on('UserStatusChanged', onUserStatusChanged)
  socket.on('ReceiveWebRtcSignal', onWebRtcSignal)
}

export function detachChatHubHandlers(socket: Socket): void {
  socket.off('new_message')
  socket.off('ReceiveMessage')
  socket.off('MessageEdited')
  socket.off('MessageDeleted')
  socket.off('UserJoined')
  socket.off('UserLeft')
  socket.off('UserTyping')
  socket.off('message_reaction_added')
  socket.off('ReactionAdded')
  socket.off('ReactionRemoved')
  socket.off('MessageRead')
  socket.off('ChannelUpdated')
  socket.off('UserStatusChanged')
  socket.off('ReceiveWebRtcSignal')
}
