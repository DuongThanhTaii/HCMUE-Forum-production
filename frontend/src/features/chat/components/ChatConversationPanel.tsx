import { useCallback, useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Info, Search } from 'lucide-react'
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
  conversationType,
  peerUserId,
  isMuted = false,
  isBlockedWithPeer = false,
  onPeerBlocked,
}: {
  threadRef: ChatThreadRef
  currentUserId: string | null
  conversationTitle?: string | null
  conversationType?: string | null
  peerUserId?: string | null
  isMuted?: boolean
  isBlockedWithPeer?: boolean
  onPeerBlocked?: () => void
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
    <div className="relative flex h-full min-h-0 min-w-0 flex-1 flex-col gap-2 overflow-hidden">
      {conversationId ? (
        <div className="flex shrink-0 items-center justify-end gap-1">
          <ConversationHeaderMenu
            conversationId={conversationId}
            peerUserId={peerUserId ?? null}
            peerName={conversationTitle}
            isMuted={isMuted}
            isBlockedWithPeer={isBlockedWithPeer}
            onBlocked={onPeerBlocked}
          />
          <button
            type="button"
            onClick={() => setInfoOpen(true)}
            className="inline-flex cursor-pointer items-center gap-1 rounded-lg border border-slate-200 px-2 py-1 text-xs font-medium text-slate-600 hover:border-indigo-200 hover:bg-indigo-50 hover:text-indigo-700"
            aria-label={t('chat.info.open')}
          >
            <Info className="h-3.5 w-3.5" aria-hidden />
            <span className="hidden sm:inline">{t('chat.info.open')}</span>
          </button>
          <button
            type="button"
            onClick={() => setSearchOpen(true)}
            className="inline-flex cursor-pointer items-center gap-1 rounded-lg border border-slate-200 px-2 py-1 text-xs font-medium text-slate-600 hover:border-indigo-200 hover:bg-indigo-50 hover:text-indigo-700"
            aria-label={t('chat.search.open')}
          >
            <Search className="h-3.5 w-3.5" aria-hidden />
            <span className="hidden sm:inline">{t('chat.search.open')}</span>
          </button>
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
            title={conversationTitle}
            peerUserId={peerUserId}
            isMuted={isMuted}
            isBlockedWithPeer={isBlockedWithPeer}
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
