import { useCallback, useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import type {
  ChatThreadRef,
  MessageDto,
  MessageSearchHitDto,
  ReplyTarget,
} from '../types/chat.types'
import { ChatComposer } from './ChatComposer'
import { ChatThread } from './ChatThread'
import { ConversationHeaderMenu } from './ConversationHeaderMenu'
import { ConversationInfoDrawer } from './ConversationInfoDrawer'
import { ConversationSearchPanel } from './ConversationSearchPanel'

export function ChatConversationPanel({
  threadRef,
  currentUserId,
  conversationTitle,
  conversationSubtitle,
  conversationType,
  peerUserId,
  isMuted = false,
  isBlockedWithPeer = false,
  onPeerBlocked,
  headerLeft,
  headerActions,
}: {
  threadRef: ChatThreadRef
  currentUserId: string | null
  conversationTitle?: React.ReactNode | null
  conversationSubtitle?: React.ReactNode | null
  conversationType?: string | null
  peerUserId?: string | null
  isMuted?: boolean
  isBlockedWithPeer?: boolean
  onPeerBlocked?: () => void
  headerLeft?: React.ReactNode
  headerActions?: React.ReactNode
}) {
  const { t } = useTranslation()
  const scrollOnSentRef = useRef<(() => void) | null>(null)
  const scrollToMessageRef = useRef<((messageId: string) => Promise<boolean>) | null>(null)
  const [searchOpen, setSearchOpen] = useState(false)
  const [infoOpen, setInfoOpen] = useState(false)
  const [replyTarget, setReplyTarget] = useState<ReplyTarget | null>(null)
  const [threadMessages, setThreadMessages] = useState<MessageDto[]>([])

  const conversationId =
    threadRef.kind === 'conversation' ? threadRef.conversationId : null

  useEffect(() => {
    setThreadMessages([])
  }, [conversationId])

  const onSelectSearchHit = useCallback(async (hit: MessageSearchHitDto) => {
    await scrollToMessageRef.current?.(hit.messageId)
  }, [])

  const onJumpToMessage = useCallback(async (messageId: string) => {
    await scrollToMessageRef.current?.(messageId)
  }, [])

  return (
    <div className="relative flex h-full min-h-0 min-w-0 flex-1 flex-col overflow-hidden">
      {conversationId ? (
        <div className="relative z-10 flex shrink-0 items-center justify-between bg-surface/95 px-4 py-2 backdrop-blur">
          <div className="flex min-w-0 flex-1 items-center gap-2">
            {headerLeft}
            <ConversationHeaderMenu
              conversationId={conversationId}
              peerUserId={peerUserId ?? null}
              peerName={conversationTitle}
              subtitle={conversationSubtitle}
              isMuted={isMuted}
              isBlockedWithPeer={isBlockedWithPeer}
              onBlocked={onPeerBlocked}
              onOpenInfo={() => setInfoOpen(true)}
              onOpenSearch={() => setSearchOpen(true)}
              expandUrl={`/chat?conversation=${conversationId}`}
            />
          </div>
          <div className="flex shrink-0 items-center gap-1">
            {headerActions}
          </div>
        </div>
      ) : null}

      {isBlockedWithPeer ? (
        <div className="rounded-lg border border-amber-200 bg-amber-50 px-3 py-2 text-sm text-amber-900">
          {t('chat.safety.blockedBanner')}
        </div>
      ) : null}

      <ChatThread
        threadRef={threadRef}
        currentUserId={currentUserId}
        conversationType={conversationType}
        peerUserId={peerUserId}
        onReply={setReplyTarget}
        onRegisterSentScroll={(fn) => {
          scrollOnSentRef.current = fn
        }}
        onRegisterScrollToMessage={(fn) => {
          scrollToMessageRef.current = fn
        }}
      />
      <ChatComposer
        threadRef={threadRef}
        disabled={isBlockedWithPeer}
        replyTarget={replyTarget}
        onClearReply={() => setReplyTarget(null)}
        onUserSentMessage={() => {
          scrollOnSentRef.current?.()
          setReplyTarget(null)
        }}
      />

      {conversationId ? (
        <>
          <ConversationSearchPanel
            conversationId={conversationId}
            open={searchOpen}
            onClose={() => setSearchOpen(false)}
            onSelectHit={onSelectSearchHit}
          />
          <ConversationInfoDrawer
            conversationId={conversationId}
            open={infoOpen}
            onClose={() => setInfoOpen(false)}
            onJumpToMessage={onJumpToMessage}
            threadMessages={threadMessages}
          />
        </>
      ) : null}
    </div>
  )
}
