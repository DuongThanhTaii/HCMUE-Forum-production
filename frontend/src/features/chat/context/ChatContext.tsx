import {
  createContext,
  startTransition,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import { Socket } from 'socket.io-client'
import { useAppDispatch } from '@shared/hooks/useAppDispatch'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { chatApi } from '../api/chat.api'
import { attachChatHubHandlers, createChatConnection, detachChatHubHandlers } from '../lib/chatHub'
import { notifyInboundChatMessage } from '../lib/chatNotifications'
import { drainChatOutbox } from '../lib/processOutbox'
import type { ChatThreadRef, HubMessageNotification, WebRtcSignalPayload } from '../types/chat.types'
import { threadKey as threadKeyOf } from '../types/chat.types'

export type HubConnectionStatus =
  | 'idle'
  | 'connecting'
  | 'connected'
  | 'reconnecting'
  | 'disconnected'

function inboundThreadKey(msg: HubMessageNotification): string | null {
  const empty = '00000000-0000-0000-0000-000000000000'
  if (msg.channelId && msg.channelId !== empty) {
    return threadKeyOf({ kind: 'channel', channelId: msg.channelId })
  }
  if (msg.conversationId && msg.conversationId !== empty) {
    return threadKeyOf({ kind: 'conversation', conversationId: msg.conversationId })
  }
  return null
}

type ChatContextValue = {
  hubStatus: HubConnectionStatus
  activeThreadKey: string | null
  setActiveThreadKey: (key: string | null) => void
  unreadByThread: Record<string, number>
  clearUnread: (key: string) => void
  joinThread: (ref: ChatThreadRef | null) => Promise<void>
  sendTyping: (conversationId: string, isTyping: boolean) => Promise<void>
  sendChannelMessage: (channelId: string, content: string) => Promise<void>
  /** Ephemeral channel lines from SignalR only (no REST history yet). */
  channelTranscripts: Record<string, HubMessageNotification[]>
  /** Display names currently typing (conversation threads only), from hub `userTyping`. */
  typingPeerNamesByConversation: Record<string, string[]>
  /** User IDs currently online according to SignalR presence events. */
  onlineUserIds: string[]
  isUserOnline: (userId: string | null | undefined) => boolean
  totalUnread: number
  /** WebRTC signaling (see `receiveWebRtcSignal` on hub). */
  subscribeWebRtcSignal: (handler: (payload: WebRtcSignalPayload) => void) => () => void
  relayWebRtcSignal: (
    conversationId: string,
    targetUserId: string,
    kind: string,
    payload: string
  ) => Promise<void>
  /** Persists a missed-call message when caller hung up before callee answered. */
  reportMissedCall: (conversationId: string) => Promise<void>
  /** Persists a call-ended message when a connected call is hung up. */
  reportCallEnded: (conversationId: string, durationSeconds?: number) => Promise<void>
  setMutedConversationIds: (ids: string[]) => void
}

const ChatContext = createContext<ChatContextValue | undefined>(undefined)

export function ChatProvider({ children }: { children: React.ReactNode }) {
  const dispatch = useAppDispatch()
  const accessToken = useAppSelector((s) => s.auth.accessToken)
  const currentUserId = useAppSelector((s) => s.auth.user?.id ?? null)

  const connectionRef = useRef<Socket | null>(null)
  const lastJoinedRef = useRef<ChatThreadRef | null>(null)
  const activeThreadKeyRef = useRef<string | null>(null)
  const currentUserIdRef = useRef<string | null>(null)
  const mutedConversationIdsRef = useRef<Set<string>>(new Set())
  const [hubStatus, setHubStatus] = useState<HubConnectionStatus>('idle')
  const [activeThreadKey, setActiveThreadKey] = useState<string | null>(null)
  const [unreadByThread, setUnreadByThread] = useState<Record<string, number>>({})
  const [channelTranscripts, setChannelTranscripts] = useState<
    Record<string, HubMessageNotification[]>
  >({})
  const [typingPeerNamesByConversation, setTypingPeerNamesByConversation] = useState<
    Record<string, string[]>
  >({})
  const [onlineByUserId, setOnlineByUserId] = useState<Record<string, boolean>>({})

  const webRtcSubscribersRef = useRef(new Set<(payload: WebRtcSignalPayload) => void>())

  useEffect(() => {
    activeThreadKeyRef.current = activeThreadKey
  }, [activeThreadKey])

  useEffect(() => {
    currentUserIdRef.current = currentUserId
  }, [currentUserId])

  const subscribeWebRtcSignal = useCallback((handler: (payload: WebRtcSignalPayload) => void) => {
    webRtcSubscribersRef.current.add(handler)
    return () => {
      webRtcSubscribersRef.current.delete(handler)
    }
  }, [])

  const relayWebRtcSignal = useCallback(
    async (conversationId: string, targetUserId: string, kind: string, payload: string) => {
      const conn = connectionRef.current
      if (!conn || !conn.connected) {
        throw new Error('chat_hub_not_connected')
      }
      conn.emit('RelayWebRtcSignal', conversationId, targetUserId, kind, payload)
    },
    []
  )

  const reportMissedCall = useCallback(async (conversationId: string) => {
    const conn = connectionRef.current
    if (!conn || !conn.connected) return
    try {
      conn.emit('ReportMissedCall', conversationId)
    } catch {
      /* best-effort */
    }
  }, [])

  const reportCallEnded = useCallback(async (conversationId: string, durationSeconds?: number) => {
    const conn = connectionRef.current
    if (!conn || !conn.connected) return
    try {
      conn.emit('ReportCallEnded', conversationId, durationSeconds)
    } catch {
      /* best-effort */
    }
  }, [])

  const clearUnread = useCallback((key: string) => {
    setUnreadByThread((prev) => {
      if (!(key in prev)) return prev
      const next = { ...prev }
      delete next[key]
      return next
    })
  }, [])

  const onReceiveMessage = useCallback((msg: HubMessageNotification) => {
    const key = inboundThreadKey(msg)
    if (!key) return
    if (msg.channelId) {
      const cid = msg.channelId
      setChannelTranscripts((prev) => {
        const next = [...(prev[cid] ?? []), msg].slice(-200)
        return { ...prev, [cid]: next }
      })
    }
    if (msg.senderId === currentUserIdRef.current) return

    if (key === activeThreadKeyRef.current) return

    if (
      msg.conversationId &&
      mutedConversationIdsRef.current.has(msg.conversationId)
    ) {
      return
    }

    setUnreadByThread((prev) => ({
      ...prev,
      [key]: (prev[key] ?? 0) + 1,
    }))
    notifyInboundChatMessage({
      threadKey: key,
      messageId: msg.messageId,
      title: msg.senderName || 'Message',
      body: msg.content?.slice(0, 140) || '',
    })
  }, [])

  const handleRemoteTyping = useCallback(
    (p: { userId: string; userName: string; conversationId: string; isTyping: boolean }) => {
      const raw = p.userName?.trim() ?? ''
      const label = raw.length > 0 && !/^unknow/i.test(raw) ? raw : `User ${p.userId.slice(0, 8)}`
      setTypingPeerNamesByConversation((prev) => {
        const set = new Set(prev[p.conversationId] ?? [])
        if (p.isTyping) set.add(label)
        else set.delete(label)
        return { ...prev, [p.conversationId]: [...set] }
      })
    },
    []
  )

  const handleUserStatusChanged = useCallback(
    (p: { userId: string; status: string }) => {
      const normalized = p.status.trim().toLowerCase()
      const online = normalized === 'online' || normalized === 'active'
      setOnlineByUserId((prev) => {
        if (prev[p.userId] === online) return prev
        return { ...prev, [p.userId]: online }
      })
    },
    []
  )

  useEffect(() => {
    if (!accessToken) {
      connectionRef.current?.disconnect()
      connectionRef.current = null
      startTransition(() => {
        setHubStatus('idle')
      })
      setOnlineByUserId({})
      lastJoinedRef.current = null
      return
    }

    const conn = createChatConnection(() => accessToken)
    connectionRef.current = conn

    attachChatHubHandlers(conn, dispatch, {
      onReceiveMessage: (msg) => {
        onReceiveMessage(msg)
      },
      onUserTyping: handleRemoteTyping,
      onUserStatusChanged: handleUserStatusChanged,
      onWebRtcSignal: (p) => {
        webRtcSubscribersRef.current.forEach((fn) => {
          try {
            fn(p)
          } catch {
            /* ignore */
          }
        })
      },
    })

    const onReconnecting = () =>
      startTransition(() => {
        setHubStatus('reconnecting')
      })
    const onReconnected = () => {
      startTransition(() => {
        setHubStatus('connected')
      })
      dispatch(chatApi.util.invalidateTags([{ type: 'ChatConversation', id: 'LIST' }]))
      void drainChatOutbox().catch(() => undefined)
    }
    const onClose = () =>
      startTransition(() => {
        setHubStatus('disconnected')
      })

    conn.on('connect_error', onReconnecting)
    conn.io.on('reconnect', onReconnected)
    conn.on('disconnect', onClose)

    startTransition(() => {
      setHubStatus('connecting')
    })
    
    conn.on('connect', () => {
      startTransition(() => {
        setHubStatus('connected')
      })
      void drainChatOutbox().catch(() => undefined)
    })

    conn.connect()

    const onFocus = () => {
      void drainChatOutbox().catch(() => undefined)
    }
    const onOnline = () => {
      void drainChatOutbox().catch(() => undefined)
    }
    window.addEventListener('focus', onFocus)
    window.addEventListener('online', onOnline)

    return () => {
      window.removeEventListener('focus', onFocus)
      window.removeEventListener('online', onOnline)
      detachChatHubHandlers(conn)
      conn.disconnect()
      connectionRef.current = null
      lastJoinedRef.current = null
      startTransition(() => {
        setHubStatus('idle')
      })
    }
  }, [accessToken, dispatch, onReceiveMessage, handleRemoteTyping, handleUserStatusChanged])

  const joinThread = useCallback(async (ref: ChatThreadRef | null) => {
    const conn = connectionRef.current
    if (!conn || !conn.connected) return

    const prev = lastJoinedRef.current

    if (!ref) {
      try {
        if (prev?.kind === 'conversation') {
          conn.emit('leave_conversation', { conversationId: prev.conversationId })
        } else if (prev?.kind === 'channel') {
          conn.emit('leave_channel', { channelId: prev.channelId })
        }
      } catch {
        // ignore
      }
      lastJoinedRef.current = null
      return
    }

    const same =
      prev &&
      prev.kind === ref.kind &&
      (ref.kind === 'conversation'
        ? prev.kind === 'conversation' && prev.conversationId === ref.conversationId
        : prev.kind === 'channel' && prev.channelId === ref.channelId)
    if (same) return

    try {
      if (prev?.kind === 'conversation') {
        conn.emit('leave_conversation', { conversationId: prev.conversationId })
      } else if (prev?.kind === 'channel') {
        conn.emit('leave_channel', { channelId: prev.channelId })
      }

      if (ref.kind === 'conversation') {
        conn.emit('join_conversation', { conversationId: ref.conversationId })
      } else {
        conn.emit('join_channel', { channelId: ref.channelId })
      }
      lastJoinedRef.current = ref
    } catch {
      // Hub membership is best-effort; REST remains authoritative
    }
  }, [])

  const sendTyping = useCallback(async (conversationId: string, isTyping: boolean) => {
    const conn = connectionRef.current
    if (!conn || !conn.connected) return
    try {
      conn.emit('SendTypingIndicator', { conversationId, isTyping })
    } catch {
      // ignore
    }
  }, [])

  const sendChannelMessage = useCallback(async (channelId: string, content: string) => {
    const conn = connectionRef.current
    if (!conn || !conn.connected) return
    conn.emit('SendChannelMessage', channelId, content, 'Text')
  }, [])

  const totalUnread = useMemo(
    () => Object.values(unreadByThread).reduce((a, b) => a + b, 0),
    [unreadByThread]
  )
  const onlineUserIds = useMemo(
    () => Object.keys(onlineByUserId).filter((id) => onlineByUserId[id]),
    [onlineByUserId]
  )
  const isUserOnline = useCallback(
    (userId: string | null | undefined) => {
      if (!userId) return false
      return Boolean(onlineByUserId[userId])
    },
    [onlineByUserId]
  )

  const setMutedConversationIds = useCallback((ids: string[]) => {
    mutedConversationIdsRef.current = new Set(ids)
  }, [])

  const value = useMemo(
    () =>
      ({
        hubStatus,
        activeThreadKey,
        setActiveThreadKey,
        unreadByThread,
        clearUnread,
        joinThread,
        sendTyping,
        sendChannelMessage,
        channelTranscripts,
        typingPeerNamesByConversation,
        onlineUserIds,
        isUserOnline,
        totalUnread,
        subscribeWebRtcSignal,
        relayWebRtcSignal,
        reportMissedCall,
        reportCallEnded,
        setMutedConversationIds,
      }) satisfies ChatContextValue,
    [
      hubStatus,
      activeThreadKey,
      unreadByThread,
      channelTranscripts,
      typingPeerNamesByConversation,
      onlineUserIds,
      isUserOnline,
      clearUnread,
      joinThread,
      sendTyping,
      sendChannelMessage,
      totalUnread,
      subscribeWebRtcSignal,
      relayWebRtcSignal,
      reportMissedCall,
      reportCallEnded,
      setMutedConversationIds,
    ]
  )
  return <ChatContext.Provider value={value}>{children}</ChatContext.Provider>
}

/** Colocated with ChatProvider — Fast Refresh requires hook in separate file otherwise. */
// eslint-disable-next-line react-refresh/only-export-components -- intentional hook + provider pair
export function useChatContext(): ChatContextValue {
  const ctx = useContext(ChatContext)
  if (!ctx) {
    throw new Error('useChatContext must be used within ChatProvider')
  }
  return ctx
}
