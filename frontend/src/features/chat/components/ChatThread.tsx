import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useChatContext } from '../context/ChatContext'
import { useChatScrollAnchor } from '../hooks/useChatScrollAnchor'
import { useConversationMessages } from '../hooks/useConversationMessages'
import { useMarkMessagesRead } from '../hooks/useMarkMessagesRead'
import type { ChatThreadRef, HubMessageNotification, MessageDto, ReplyTarget } from '../types/chat.types'
import { ChatMessageList } from './ChatMessageList'
import { MessageBubble } from './MessageBubble'

function hubToDisplay(m: HubMessageNotification): MessageDto {
  return {
    id: m.messageId,
    conversationId: m.conversationId,
    senderId: m.senderId,
    content: m.content,
    type: m.messageType,
    sentAt: m.sentAt,
    editedAt: null,
    isDeleted: false,
    replyToMessageId: m.replyToMessageId ?? null,
    reactions: {},
    attachments: m.attachments ?? [],
    senderDisplayName: m.senderName,
  }
}

export function ChatThread({
  threadRef,
  currentUserId,
  conversationType,
  peerUserId,
  onRegisterSentScroll,
  onRegisterScrollToMessage,
  onReply,
  onThreadMessagesChange,
}: {
  threadRef: ChatThreadRef
  currentUserId: string | null
  conversationType?: string | null
  peerUserId?: string | null
  onRegisterSentScroll?: (scrollToBottomOnSend: (() => void) | null) => void
  onRegisterScrollToMessage?: (
    scrollToMessage: ((messageId: string) => Promise<boolean>) | null,
  ) => void
  onReply?: (target: ReplyTarget) => void
  onThreadMessagesChange?: (messages: MessageDto[]) => void
}) {
  const { t } = useTranslation()
  const { channelTranscripts, typingPeerNamesByConversation } = useChatContext()

  const convId = threadRef.kind === 'conversation' ? threadRef.conversationId : null
  const isDirectDm =
    (conversationType ?? '').toLowerCase() === 'direct' ||
    (conversationType ?? '').toLowerCase() === 'dm'

  const [highlightedMessageId, setHighlightedMessageId] = useState<string | null>(null)

  const {
    messages: convMessages,
    isLoading,
    isError,
    isLoadingOlder,
    hasMoreOlder,
    loadOlder,
    ensureMessageLoaded,
  } = useConversationMessages(convId)

  const channelLines = useMemo(() => {
    if (threadRef.kind !== 'channel') return []
    return channelTranscripts[threadRef.channelId] ?? []
  }, [channelTranscripts, threadRef])

  const channelMessages = useMemo(
    () => channelLines.map(hubToDisplay),
    [channelLines],
  )

  const messages =
    threadRef.kind === 'conversation' ? convMessages : channelMessages

  const messageById = useMemo(() => {
    const map = new Map<string, MessageDto>()
    for (const m of messages) {
      map.set(m.id, m)
    }
    return map
  }, [messages])

  const lastOwnMessageId = useMemo(() => {
    if (!currentUserId) return null
    for (let i = messages.length - 1; i >= 0; i--) {
      const m = messages[i]
      if (m.senderId === currentUserId && !m.isDeleted) return m.id
    }
    return null
  }, [currentUserId, messages])

  const conversationKey =
    threadRef.kind === 'conversation' ? threadRef.conversationId : null

  const loadOlderRef = useRef(loadOlder)
  loadOlderRef.current = loadOlder

  const captureBeforePrependRef = useRef<() => void>(() => {})

  const scroll = useChatScrollAnchor(messages.length, conversationKey, {
    hasMoreOlder: threadRef.kind === 'conversation' && hasMoreOlder,
    isLoadingOlder: threadRef.kind === 'conversation' && isLoadingOlder,
    onNearTop:
      threadRef.kind === 'conversation'
        ? () => {
            captureBeforePrependRef.current()
            void loadOlderRef.current()
          }
        : undefined,
  })

  useMarkMessagesRead(convId, messages, scroll.atBottom, currentUserId)

  useEffect(() => {
    if (threadRef.kind !== 'conversation' || !onThreadMessagesChange) return
    onThreadMessagesChange(convMessages)
  }, [threadRef.kind, convMessages, onThreadMessagesChange])

  captureBeforePrependRef.current = scroll.captureScrollBeforePrepend

  useEffect(() => {
    if (!onRegisterSentScroll) return
    if (threadRef.kind === 'conversation') {
      onRegisterSentScroll(scroll.onUserSentMessage)
    } else {
      onRegisterSentScroll(null)
    }
    return () => onRegisterSentScroll(null)
  }, [onRegisterSentScroll, scroll.onUserSentMessage, threadRef.kind])

  const scrollToMessage = useCallback(
    async (messageId: string) => {
      if (threadRef.kind !== 'conversation') {
        return false
      }
      const loaded = await ensureMessageLoaded(messageId)
      if (!loaded) {
        return false
      }
      await new Promise<void>((resolve) => {
        requestAnimationFrame(() => requestAnimationFrame(() => resolve()))
      })
      const ok = scroll.scrollToMessageId(messageId)
      if (ok) {
        setHighlightedMessageId(messageId)
        window.setTimeout(() => setHighlightedMessageId(null), 2000)
      }
      return ok
    },
    [ensureMessageLoaded, scroll, threadRef.kind],
  )

  useEffect(() => {
    if (!onRegisterScrollToMessage) return
    if (threadRef.kind === 'conversation') {
      onRegisterScrollToMessage(scrollToMessage)
    } else {
      onRegisterScrollToMessage(null)
    }
    return () => onRegisterScrollToMessage(null)
  }, [onRegisterScrollToMessage, scrollToMessage, threadRef.kind])

  const typingPeers =
    threadRef.kind === 'conversation' ? typingPeerNamesByConversation[threadRef.conversationId] ?? [] : []

  const handleReply = useCallback(
    (message: MessageDto) => {
      if (!onReply) return
      const preview =
        message.content?.trim() ||
        ((message.attachments?.length ?? 0) > 0
          ? t('chat.reply.attachmentPreview')
          : t('chat.reply.emptyPreview'))
      onReply({
        messageId: message.id,
        preview,
        senderLabel:
          message.senderDisplayName?.trim() ||
          t('chat.user'),
      })
    },
    [onReply, t],
  )

  const bubbleList = messages.map((m) => (
    <MessageBubble
      key={`${m.id}-${m.editedAt ?? ''}-${m.isDeleted}`}
      message={m}
      isSelf={m.senderId === currentUserId}
      conversationId={threadRef.kind === 'conversation' ? threadRef.conversationId : null}
      currentUserId={currentUserId}
      highlighted={highlightedMessageId === m.id}
      messageById={messageById}
      onReply={onReply ? handleReply : undefined}
      isLastOwnMessage={m.id === lastOwnMessageId}
      isDirectDm={isDirectDm}
      peerUserId={peerUserId ?? null}
    />
  ))

  if (threadRef.kind === 'channel') {
    return (
      <div className="flex min-h-0 flex-1 flex-col">
        <div className="mb-2 rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-xs text-amber-900">
          {t('chat.channelHistoryNotice')}
        </div>
        <ChatMessageList
          containerRef={scroll.containerRef}
          onScroll={scroll.handleScroll}
          showScrollFab={!scroll.atBottom || scroll.pendingNewCount > 0}
          pendingNewCount={scroll.pendingNewCount}
          onScrollToBottom={() => scroll.scrollToBottom(true)}
          empty={
            messages.length === 0 ? (
              <p className="text-center text-sm text-muted">{t('chat.channelEmptyLive')}</p>
            ) : undefined
          }
        >
          {bubbleList}
        </ChatMessageList>
      </div>
    )
  }

  if (isError || !convId) {
    return (
      <div className="text-sm text-red-600">{t('chat.cannotLoadMessages')}</div>
    )
  }

  return (
    <div className="flex h-full min-h-0 min-w-0 flex-1 flex-col gap-1 overflow-hidden">
      <ChatMessageList
        containerRef={scroll.containerRef}
        onScroll={scroll.handleScroll}
        showScrollFab={!scroll.atBottom || scroll.pendingNewCount > 0}
        pendingNewCount={scroll.pendingNewCount}
        onScrollToBottom={() => scroll.scrollToBottom(true)}
        isLoadingOlder={isLoadingOlder}
        hasMoreOlder={hasMoreOlder}
        isInitialLoading={isLoading && messages.length === 0}
        empty={
          !isLoading && messages.length === 0 ? (
            <p className="text-center text-sm text-muted">{t('chat.thread.noMessagesYet')}</p>
          ) : undefined
        }
      >
        {bubbleList}
      </ChatMessageList>
      {typingPeers.length > 0 && (
        <div className="flex shrink-0 items-center gap-2 px-4 pb-2 text-xs text-muted">
          <div className="flex items-center gap-1 rounded-full bg-background px-3 py-2 shadow-sm">
            <span className="block h-1.5 w-1.5 animate-bounce rounded-full bg-slate-400" style={{ animationDelay: '0ms' }}></span>
            <span className="block h-1.5 w-1.5 animate-bounce rounded-full bg-slate-400" style={{ animationDelay: '150ms' }}></span>
            <span className="block h-1.5 w-1.5 animate-bounce rounded-full bg-slate-400" style={{ animationDelay: '300ms' }}></span>
          </div>
          <span>{typingPeers.join(', ')} {t('chat.typing')}</span>
        </div>
      )}
    </div>
  )
}
