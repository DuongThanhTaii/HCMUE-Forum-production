import * as signalR from '@microsoft/signalr'
import type { HubConnection } from '@microsoft/signalr'
import type { AppDispatch } from '../../../app/store'
import { chatApi } from '../api/chat.api'
import { getChatHubUrl } from './hubUrl'
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

export function createChatConnection(getAccessToken: () => string | null): HubConnection {
  return new signalR.HubConnectionBuilder()
    .withUrl(getChatHubUrl(), {
      accessTokenFactory: async () => getAccessToken() ?? '',
      // Prefer WebSocket; fall back to LongPolling if WS unavailable (e.g. some proxies).
      transport:
        signalR.HttpTransportType.WebSockets | signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10_000])
    .build()
}

/** C# hub method names (PascalCase). Wire protocol uses camelCase — see `hubClientMethodNames`. */
export const CHAT_HUB_CLIENT_METHODS = [
  'ReceiveMessage',
  'MessageEdited',
  'MessageDeleted',
  'UserJoined',
  'UserLeft',
  'UserTyping',
  'ReactionAdded',
  'ReactionRemoved',
  'MessageRead',
  'ChannelUpdated',
  'UserStatusChanged',
  'ReceiveWebRtcSignal',
] as const

function hubMethodAliases(pascal: string): string[] {
  const camel = pascal.charAt(0).toLowerCase() + pascal.slice(1)
  return camel === pascal ? [pascal] : [camel, pascal]
}

/** All listener names to register/off (camelCase first — ASP.NET Core JSON hub protocol default). */
export const CHAT_HUB_CLIENT_LISTENER_NAMES = CHAT_HUB_CLIENT_METHODS.flatMap(hubMethodAliases)

function hubOn(
  connection: HubConnection,
  pascalMethod: (typeof CHAT_HUB_CLIENT_METHODS)[number],
  handler: (...args: unknown[]) => void,
): void {
  for (const name of hubMethodAliases(pascalMethod)) {
    connection.on(name, handler)
  }
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
  connection: HubConnection,
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

  const onMessageDeleted = onMessageEdited

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

  const onUserLeft = onUserJoined

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

  hubOn(connection, 'ReceiveMessage', onReceive)
  hubOn(connection, 'MessageEdited', onMessageEdited)
  hubOn(connection, 'MessageDeleted', onMessageDeleted)
  hubOn(connection, 'UserJoined', onUserJoined)
  hubOn(connection, 'UserLeft', onUserLeft)
  hubOn(connection, 'UserTyping', onUserTyping)
  hubOn(connection, 'ReactionAdded', onReaction)
  hubOn(connection, 'ReactionRemoved', onReaction)
  hubOn(connection, 'MessageRead', onMessageRead)
  hubOn(connection, 'ChannelUpdated', onChannelUpdated)
  hubOn(connection, 'UserStatusChanged', onUserStatusChanged)
  hubOn(connection, 'ReceiveWebRtcSignal', onWebRtcSignal)
}

export function detachChatHubHandlers(connection: HubConnection): void {
  for (const method of CHAT_HUB_CLIENT_LISTENER_NAMES) {
    connection.off(method)
  }
}
