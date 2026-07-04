import {
  startTransition,
  useCallback,
  useEffect,
  useMemo,
  useState,
} from 'react'
import { Link, useLocation } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { BellOff, ChevronLeft, MessageCircle, Minus, Maximize2 } from 'lucide-react'
import { useGetConversationsQuery } from '../api/chat.api'
import { useChatContext } from '../context/ChatContext'
import {
  conversationSubtitle,
  primaryConversationTitle,
} from '../lib/conversationLabels'
import { getDockVisibility, setDockVisibility } from '../lib/dockStorage'
import type { ChatThreadRef, ConversationDto } from '../types/chat.types'
import { threadKey } from '../types/chat.types'
import { ChatCallBar } from './ChatCallBar'
import { ChatConversationPanel } from './ChatConversationPanel'
import { ChatPeerAvatar } from './ChatPeerAvatar'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserId } from '@features/auth/model/auth.slice'

const SK_PANEL = 'chatDock.panel'
const SK_CONV = 'chatDock.conversationId'

function readInitialDockState(): { panel: 'list' | 'thread'; convId: string | null } {
  try {
    if (typeof sessionStorage === 'undefined') return { panel: 'list', convId: null }
    const panel = sessionStorage.getItem(SK_PANEL) === 'thread' ? 'thread' : 'list'
    const convId = sessionStorage.getItem(SK_CONV)
    if (panel === 'thread' && convId) return { panel: 'thread', convId }
  } catch {
    /* ignore */
  }
  return { panel: 'list', convId: null }
}

function persistDockState(panel: 'list' | 'thread', convId: string | null) {
  try {
    if (typeof sessionStorage === 'undefined') return
    sessionStorage.setItem(SK_PANEL, panel)
    if (panel === 'thread' && convId) sessionStorage.setItem(SK_CONV, convId)
    else sessionStorage.removeItem(SK_CONV)
  } catch {
    /* ignore */
  }
}

export function ChatDock() {
  const { t } = useTranslation()
  const location = useLocation()
  const isFullChatRoute = location.pathname.startsWith('/chat')
  const currentUserId = useAppSelector(selectUserId)
  const {
    totalUnread,
    unreadByThread,
    setActiveThreadKey,
    clearUnread,
    joinThread,
    setMutedConversationIds,
  } = useChatContext()

  const { data: convos, isLoading: convLoading } = useGetConversationsQuery()

  useEffect(() => {
    if (!convos) return
    setMutedConversationIds(convos.filter((c) => c.isMuted).map((c) => c.id))
  }, [convos, setMutedConversationIds])

  const [minimized, setMinimized] = useState(
    () => getDockVisibility() === 'hidden',
  )
  const initial = useMemo(() => readInitialDockState(), [])
  const [panel, setPanel] = useState<'list' | 'thread'>(initial.panel)
  const [activeConvId, setActiveConvId] = useState<string | null>(initial.convId)

  const activeThreadRef: ChatThreadRef | null = useMemo(
    () =>
      activeConvId && panel === 'thread'
        ? { kind: 'conversation', conversationId: activeConvId }
        : null,
    [activeConvId, panel],
  )

  const sortedConvos = useMemo(() => {
    const list = [...(convos ?? [])]
    return list.sort((a, b) => {
      const ta = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0
      const tb = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0
      return tb - ta
    })
  }, [convos])

  const openConversation = (c: ConversationDto) => {
    setActiveConvId(c.id)
    setPanel('thread')
    persistDockState('thread', c.id)
  }

  const backToList = useCallback(() => {
    setPanel('list')
    setActiveConvId(null)
    persistDockState('list', null)
    if (!isFullChatRoute) {
      void joinThread(null)
      setActiveThreadKey(null)
    }
  }, [isFullChatRoute, joinThread, setActiveThreadKey])

  useEffect(() => {
    if (isFullChatRoute) return
    if (!activeThreadRef || activeThreadRef.kind !== 'conversation') return
    const k = threadKey(activeThreadRef)
    setActiveThreadKey(k)
    clearUnread(k)
    void joinThread(activeThreadRef)
  }, [activeThreadRef, isFullChatRoute, setActiveThreadKey, clearUnread, joinThread])

  useEffect(() => {
    if (isFullChatRoute) return
    if (panel !== 'thread' || !activeConvId || convLoading) return
    if (convos === undefined) return
    if (!convos.some((c) => c.id === activeConvId)) {
      startTransition(() => {
        backToList()
      })
    }
  }, [panel, activeConvId, convos, convLoading, backToList, isFullChatRoute])

  // Hide floating dock when user is already in full chat page.
  if (isFullChatRoute) {
    return null
  }

  if (minimized) {
    return (
      <button
        type="button"
        onClick={() => {
          setMinimized(false)
          setDockVisibility('visible')
        }}
        className="fixed bottom-4 right-4 z-50 flex items-center gap-2 rounded-full border border-border bg-surface px-4 py-2.5 text-sm font-medium text-foreground shadow-lg hover:bg-background"
        aria-label={t('chat.dock.open')}
      >
        <MessageCircle className="h-4 w-4" />
        <span>{t('chat.title')}</span>
        {totalUnread > 0 && (
          <span className="flex h-5 min-w-5 items-center justify-center rounded-full bg-primary px-1.5 text-[11px] text-primary-foreground">
            {totalUnread > 99 ? '99+' : totalUnread}
          </span>
        )}
      </button>
    )
  }

  const activeConv = activeConvId ? convos?.find((c) => c.id === activeConvId) : null

  const headerTitle =
    panel === 'list'
      ? t('chat.title')
      : activeConv
        ? primaryConversationTitle(activeConv, currentUserId)
        : t('chat.title')

  const headerSubtitle =
    panel === 'thread' && activeConv ? conversationSubtitle(activeConv) : null

  return (
    <div className="chat-theme-root fixed bottom-0 right-4 z-50 flex w-full max-w-sm flex-col rounded-t-xl border border-b-0 border-border bg-surface shadow-2xl">
      {panel === 'list' && (
        <div className="flex shrink-0 items-center gap-2 border-b border-border px-2 py-2">
          <div className="min-w-0 flex-1">
            <Link
              to="/chat"
              className="flex items-center gap-2 text-sm font-semibold text-foreground hover:text-primary"
            >
              <MessageCircle className="h-4 w-4 shrink-0" />
              <span className="truncate">{headerTitle}</span>
              {totalUnread > 0 && (
                <span className="shrink-0 rounded-full bg-primary px-2 py-0.5 text-[10px] text-primary-foreground">
                  {totalUnread > 99 ? '99+' : totalUnread}
                </span>
              )}
            </Link>
          </div>
          <div className="flex shrink-0 items-center gap-0.5">
            <Link
              to={activeConvId ? `/chat?conversation=${activeConvId}` : '/chat'}
              className="rounded p-1.5 text-muted hover:bg-background"
              aria-label={t('chat.dock.expandFull')}
            >
              <Maximize2 className="h-4 w-4" />
            </Link>
            <button
              type="button"
              className="rounded p-1.5 text-muted hover:bg-background"
              onClick={() => {
                setMinimized(true)
                setDockVisibility('hidden')
              }}
              aria-label={t('chat.dock.minimize')}
            >
              <Minus className="h-4 w-4" />
            </button>
          </div>
        </div>
      )}

      <div className="flex h-[min(520px,82vh)] min-h-[320px] max-h-[85vh] flex-col overflow-hidden">
        {panel === 'list' && (
          <>
            <div className="min-h-0 flex-1 overflow-y-auto px-1 py-1">
              {convLoading ? (
                <p className="px-2 py-4 text-center text-sm text-muted">
                  {t('common.loading')}
                </p>
              ) : sortedConvos.length === 0 ? (
                <div className="px-2 py-4 text-center">
                  <p className="text-sm text-muted">{t('chat.dock.emptyConversations')}</p>
                  <Link
                    to="/chat"
                    className="mt-2 inline-block text-sm font-medium text-primary hover:underline"
                  >
                    {t('chat.dock.openFullCta')}
                  </Link>
                </div>
              ) : (
                <ul className="space-y-0.5">
                  {sortedConvos.slice(0, 15).map((c) => {
                    const title = primaryConversationTitle(c, currentUserId)
                    const sub = conversationSubtitle(c)
                    const unread =
                      unreadByThread[
                        threadKey({ kind: 'conversation', conversationId: c.id })
                      ] ?? 0
                    return (
                      <li key={c.id}>
                        <button
                          type="button"
                          onClick={() => openConversation(c)}
                          className="flex w-full items-center gap-2 rounded-lg px-2 py-2 text-left hover:bg-background"
                        >
                          <ChatPeerAvatar name={title} className="h-10 w-10 text-[11px]" />
                          <span className="min-w-0 flex-1">
                            <span className="block truncate text-sm font-medium text-foreground">
                              {title}
                            </span>
                            {sub && (
                              <span className="block truncate text-[11px] text-muted">{sub}</span>
                            )}
                          </span>
                          <span className="flex shrink-0 items-center gap-1">
                            {c.isMuted && (
                              <BellOff
                                className="h-3.5 w-3.5 text-slate-400"
                                aria-label={t('chat.safety.mutedBadge')}
                              />
                            )}
                            {unread > 0 && (
                              <span className="rounded-full bg-primary px-1.5 text-[10px] text-primary-foreground">
                                {unread > 9 ? '9+' : unread}
                              </span>
                            )}
                          </span>
                        </button>
                      </li>
                    )
                  })}
                </ul>
              )}
            </div>
            <div className="shrink-0 border-t border-border px-3 py-2">
              <Link
                to="/chat"
                className="block w-full rounded-lg bg-primary py-2 text-center text-sm font-semibold text-primary-foreground hover:bg-primary-hover"
              >
                {t('chat.dock.openFullCta')}
              </Link>
              <p className="mt-1.5 text-center text-[11px] leading-snug text-muted">
                {t('chat.dock.hintShort')}
              </p>
            </div>
          </>
        )}

        {panel === 'thread' && activeThreadRef && (
          <div className="flex h-full min-h-0 min-w-0 flex-1 flex-col overflow-hidden px-2 pb-2 pt-1">
            <ChatConversationPanel
              threadRef={activeThreadRef}
              currentUserId={currentUserId}
              conversationTitle={headerTitle}
              conversationSubtitle={headerSubtitle}
              conversationType={activeConv?.type ?? null}
              peerUserId={activeConv?.directPeerUserId ?? null}
              isMuted={activeConv?.isMuted ?? false}
              isBlockedWithPeer={activeConv?.isBlockedWithPeer ?? false}
              onPeerBlocked={backToList}
              headerLeft={
                <>
                  <button
                    type="button"
                    onClick={backToList}
                    className="rounded-full p-1.5 text-muted hover:bg-background -ml-2 shrink-0"
                    aria-label={t('chat.dock.backToList')}
                  >
                    <ChevronLeft className="h-5 w-5" />
                  </button>
                  {activeConv && (
                    <ChatPeerAvatar
                      name={primaryConversationTitle(activeConv, currentUserId)}
                      className="h-9 w-9 text-[11px] shrink-0"
                    />
                  )}
                </>
              }
              headerActions={
                <>
                  <ChatCallBar threadRef={activeThreadRef} conversation={activeConv ?? null} />
                  <button
                    type="button"
                    className="rounded-full p-1.5 text-muted hover:bg-background"
                    onClick={() => {
                      setMinimized(true)
                      setDockVisibility('hidden')
                    }}
                    aria-label={t('chat.dock.minimize')}
                  >
                    <Minus className="h-4 w-4" />
                  </button>
                </>
              }
            />
          </div>
        )}
      </div>
    </div>
  )
}
