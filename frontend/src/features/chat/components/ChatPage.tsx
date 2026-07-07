import { startTransition, useEffect, useMemo, useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import { ArrowLeft, BellOff } from 'lucide-react'
import { skipToken } from '@reduxjs/toolkit/query/react'
import {
  useCreateDirectConversationMutation,
  useGetConversationsQuery,
  useGetMyChannelsQuery,
  useGetPublicChannelsQuery,
  useSearchChatUsersQuery,
  useJoinChannelMutation,
} from '../api/chat.api'
import { useChatContext } from '../context/ChatContext'
import type { ChatThreadRef } from '../types/chat.types'
import { threadKey } from '../types/chat.types'
import { ChatCallBar } from './ChatCallBar'
import { ChatConversationPanel } from './ChatConversationPanel'
import { ChatPeerAvatar } from './ChatPeerAvatar'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import { selectUserId } from '@features/auth/model/auth.slice'
import { requestChatNotificationPermission } from '../lib/chatNotifications'
import {
  conversationSubtitle,
  primaryConversationTitle,
} from '../lib/conversationLabels'

function useNarrowChatLayout() {
  const [narrow, setNarrow] = useState(false)
  useEffect(() => {
    const mq = window.matchMedia('(max-width: 767px)')
    const fn = () => setNarrow(mq.matches)
    fn()
    mq.addEventListener('change', fn)
    return () => mq.removeEventListener('change', fn)
  }, [])
  return narrow
}

export function ChatPage() {
  const { t } = useTranslation()
  const [searchParams, setSearchParams] = useSearchParams()
  const narrow = useNarrowChatLayout()
  const [panel, setPanel] = useState<'list' | 'thread'>('list')
  const [listTab, setListTab] = useState<'conv' | 'channels'>('conv')
  const [selected, setSelected] = useState<ChatThreadRef | null>(null)
  const [dmPickerOpen, setDmPickerOpen] = useState(false)
  const [userQuery, setUserQuery] = useState('')
  const currentUserId = useAppSelector(selectUserId)
  const {
    setActiveThreadKey,
    joinThread,
    clearUnread,
    unreadByThread,
    hubStatus,
    setMutedConversationIds,
    isUserOnline,
  } = useChatContext()

  const { data: convos, isLoading: convLoading } = useGetConversationsQuery()

  useEffect(() => {
    if (!convos) return
    setMutedConversationIds(convos.filter((c) => c.isMuted).map((c) => c.id))
  }, [convos, setMutedConversationIds])

  const selectedConversation = useMemo(() => {
    if (selected?.kind !== 'conversation') return null
    return convos?.find((c) => c.id === selected.conversationId) ?? null
  }, [convos, selected])
  const selectedPeerActiveNow = useMemo(() => {
    if (!selectedConversation) return false
    const isDirect = (selectedConversation.type ?? '').toLowerCase().includes('direct')
    return isDirect && isUserOnline(selectedConversation.directPeerUserId)
  }, [selectedConversation, isUserOnline])

  const conversationFromUrl = searchParams.get('conversation')

  useEffect(() => {
    const cid = conversationFromUrl?.trim()
    if (!cid || !convos?.some((c) => c.id === cid)) return
    if (selected?.kind !== 'conversation' || selected.conversationId !== cid) {
      startTransition(() => {
        setSelected({ kind: 'conversation', conversationId: cid })
        if (narrow) setPanel('thread')
      })
    }
  }, [conversationFromUrl, convos, narrow, selected])
  const { data: publicChannels } = useGetPublicChannelsQuery()
  const { data: myChannels } = useGetMyChannelsQuery()
  const qTrim = userQuery.trim()
  const searchUsersArg =
    dmPickerOpen ? { q: qTrim, take: 24 } : skipToken
  const { data: searchHits, isFetching: searchLoading } = useSearchChatUsersQuery(searchUsersArg)
  const [joinChannel] = useJoinChannelMutation()
  const [createDm] = useCreateDirectConversationMutation()

  useEffect(() => {
    if (!selected) {
      void joinThread(null)
      setActiveThreadKey(null)
      return
    }
    const key = threadKey(selected)
    setActiveThreadKey(key)
    clearUnread(key)
    void joinThread(selected)
  }, [selected, setActiveThreadKey, joinThread, clearUnread])

  const selectThread = (ref: ChatThreadRef) => {
    setSelected(ref)
    if (narrow) setPanel('thread')
    if (ref.kind === 'conversation') {
      setSearchParams({ conversation: ref.conversationId }, { replace: true })
    }
  }

  const backToList = () => {
    setPanel('list')
    setSearchParams({}, { replace: true })
  }

  const showList = !narrow || panel === 'list'
  const showThread = !narrow || panel === 'thread'

  const titleForSelected = () => {
    if (!selected) return ''
    if (selected.kind === 'channel') {
      const all = [...(publicChannels ?? []), ...(myChannels ?? [])]
      const ch = all.find((c) => c.id === selected.channelId)
      return ch?.name ?? selected.channelId
    }
    const c = convos?.find((x) => x.id === selected.conversationId)
    return c ? primaryConversationTitle(c, currentUserId) : selected.conversationId
  }

  const subtitleForSelected = () => {
    if (!selected || selected.kind !== 'conversation') return null
    const c = convos?.find((x) => x.id === selected.conversationId)
    if (!c) return null
    if (selectedPeerActiveNow) {
      return t('chat.presence.activeNow')
    }
    return conversationSubtitle(c)
  }

  const hubLabel =
    hubStatus === 'connected'
      ? t('chat.connectionStatus.connected')
      : hubStatus === 'connecting' || hubStatus === 'reconnecting'
        ? t('chat.connectionStatus.reconnecting')
        : t('chat.connectionStatus.disconnected')

  const closeDmPicker = () => {
    setDmPickerOpen(false)
    setUserQuery('')
  }

  const startDmWithUser = async (otherUserId: string) => {
    try {
      const res = await createDm({ otherUserId }).unwrap()
      closeDmPicker()
      selectThread({
        kind: 'conversation',
        conversationId: res.conversationId,
      })
    } catch {
      window.alert(t('chat.dm.createError'))
    }
  }

  return (
    <div className="chat-theme-root flex h-[calc(100dvh-7rem)] max-h-[calc(100dvh-7rem)] flex-col overflow-hidden md:flex-row md:gap-4">
      <section
        className={`flex min-h-0 flex-col border-border md:w-80 md:border-r md:pr-4 ${showList ? 'flex' : 'hidden'} md:flex`}
      >
        <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
          <h1 className="text-lg font-semibold text-foreground">{t('chat.title')}</h1>
          <span className="rounded-full bg-background px-2 py-0.5 text-[10px] text-muted">
            {hubLabel}
          </span>
        </div>

        <button
          type="button"
          className="mb-3 rounded-lg border border-border-strong px-3 py-2 text-left text-xs text-slate-700 hover:bg-background"
          onClick={() => void requestChatNotificationPermission()}
        >
          {t('chat.notifications.enable')}
        </button>

        <div className="mb-3 flex rounded-lg bg-background p-1 text-xs font-medium">
          <button
            type="button"
            className={`flex-1 rounded-md px-2 py-1 ${listTab === 'conv' ? 'bg-surface shadow-sm' : ''}`}
            onClick={() => setListTab('conv')}
          >
            {t('chat.conversations')}
          </button>
          <button
            type="button"
            className={`flex-1 rounded-md px-2 py-1 ${listTab === 'channels' ? 'bg-surface shadow-sm' : ''}`}
            onClick={() => setListTab('channels')}
          >
            {t('chat.channels')}
          </button>
        </div>

        {listTab === 'conv' && (
          <div className="flex min-h-0 flex-1 flex-col space-y-3">
            <button
              type="button"
              className="w-full rounded-xl bg-primary px-3 py-2.5 text-sm font-medium text-primary-foreground shadow-sm hover:bg-indigo-700"
              onClick={() => setDmPickerOpen(true)}
            >
              {t('chat.dm.newChat')}
            </button>

            <div className="min-h-0 flex-1 overflow-y-auto">
              {convLoading ? (
                <p className="text-sm text-muted">{t('common.loading')}</p>
              ) : !convos?.length ? (
                <p className="text-sm text-muted">{t('chat.noConversations')}</p>
              ) : (
                <ul className="space-y-1">
                  {convos.map((c) => {
                    const ref: ChatThreadRef = {
                      kind: 'conversation',
                      conversationId: c.id,
                    }
                    const k = threadKey(ref)
                    const unread = unreadByThread[k] ?? 0
                    const active =
                      selected?.kind === 'conversation' && selected.conversationId === c.id
                    const title = primaryConversationTitle(c, currentUserId)
                    const isDirect = (c.type ?? '').toLowerCase().includes('direct')
                    const isActiveNow = isDirect && isUserOnline(c.directPeerUserId)
                    const sub =
                      isActiveNow
                        ? t('chat.presence.activeNow')
                        : conversationSubtitle(c)
                    return (
                      <li key={c.id}>
                        <button
                          type="button"
                          onClick={() => selectThread(ref)}
                          className={`flex w-full items-center gap-3 rounded-xl px-2 py-2.5 text-left ${
                            active ? 'bg-primary/10 ring-1 ring-indigo-100' : 'hover:bg-background'
                          }`}
                        >
                          <ChatPeerAvatar name={title} />
                          <span className="min-w-0 flex-1">
                            <span className="block truncate font-medium text-foreground">{title}</span>
                            {sub ? (
                              <span
                                className={`block truncate text-xs ${isActiveNow ? 'inline-flex items-center gap-1 text-emerald-600' : 'text-muted'}`}
                              >
                                {isActiveNow && (
                                  <span
                                    className="inline-block h-2 w-2 rounded-full bg-emerald-500"
                                    aria-hidden
                                  />
                                )}
                                {sub}
                              </span>
                            ) : (
                              <span className="block truncate text-xs text-slate-400">
                                {t('chat.dm.directChat')}
                              </span>
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
                              <span className="rounded-full bg-primary px-2 py-0.5 text-[10px] text-primary-foreground">
                                {unread > 99 ? '99+' : unread}
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
          </div>
        )}

        {listTab === 'channels' && (
          <div className="min-h-0 flex-1 space-y-4 overflow-y-auto">
            <div>
              <h2 className="mb-2 text-xs font-semibold uppercase text-muted">
                {t('chat.channelsPage.publicChannels')}
              </h2>
              <ul className="space-y-1">
                {(publicChannels ?? []).map((ch) => {
                  const ref: ChatThreadRef = { kind: 'channel', channelId: ch.id }
                  const k = threadKey(ref)
                  const unread = unreadByThread[k] ?? 0
                  const active =
                    selected?.kind === 'channel' && selected.channelId === ch.id
                  return (
                    <li key={ch.id} className="flex gap-1">
                      <button
                        type="button"
                        className={`flex flex-1 items-center justify-between rounded-lg px-2 py-2 text-left text-sm ${
                          active ? 'bg-primary/10 text-primary' : 'hover:bg-background'
                        }`}
                        onClick={() => selectThread(ref)}
                      >
                        <span className="truncate">{ch.name}</span>
                        {unread > 0 && (
                          <span className="ml-2 shrink-0 rounded-full bg-primary px-2 py-0.5 text-[10px] text-primary-foreground">
                            {unread > 99 ? '99+' : unread}
                          </span>
                        )}
                      </button>
                      <button
                        type="button"
                        className="rounded-lg border border-border-strong px-2 text-xs"
                        onClick={() =>
                          void joinChannel(ch.id)
                            .unwrap()
                            .catch(() => undefined)
                        }
                      >
                        {t('chat.channelsPage.joinChannel')}
                      </button>
                    </li>
                  )
                })}
              </ul>
            </div>
            <div>
              <h2 className="mb-2 text-xs font-semibold uppercase text-muted">
                {t('chat.channelsPage.myChannels')}
              </h2>
              <ul className="space-y-1">
                {(myChannels ?? []).map((ch) => {
                  const ref: ChatThreadRef = { kind: 'channel', channelId: ch.id }
                  const k = threadKey(ref)
                  const unread = unreadByThread[k] ?? 0
                  const active =
                    selected?.kind === 'channel' && selected.channelId === ch.id
                  return (
                    <li key={ch.id}>
                      <button
                        type="button"
                        className={`flex w-full items-center justify-between rounded-lg px-2 py-2 text-left text-sm ${
                          active ? 'bg-primary/10 text-primary' : 'hover:bg-background'
                        }`}
                        onClick={() => selectThread(ref)}
                      >
                        <span className="truncate">{ch.name}</span>
                        {unread > 0 && (
                          <span className="ml-2 shrink-0 rounded-full bg-primary px-2 py-0.5 text-[10px] text-primary-foreground">
                            {unread > 99 ? '99+' : unread}
                          </span>
                        )}
                      </button>
                    </li>
                  )
                })}
              </ul>
            </div>
          </div>
        )}
      </section>

      {dmPickerOpen && (
        <div
          className="fixed inset-0 z-[60] flex items-end justify-center bg-black/40 p-4 sm:items-center"
          role="dialog"
          aria-modal="true"
          aria-labelledby="dm-picker-title"
        >
          <button
            type="button"
            className="absolute inset-0 cursor-default"
            aria-label={t('chat.dm.closePicker')}
            onClick={closeDmPicker}
          />
          <div className="relative z-10 w-full max-w-md rounded-2xl bg-surface p-4 shadow-xl">
            <div className="mb-3 flex items-center justify-between gap-2">
              <h2 id="dm-picker-title" className="text-base font-semibold text-foreground">
                {t('chat.dm.newChat')}
              </h2>
              <button
                type="button"
                className="rounded-lg px-2 py-1 text-sm text-muted hover:bg-background"
                onClick={closeDmPicker}
              >
                {t('common.close')}
              </button>
            </div>
            <p className="mb-2 text-xs text-muted">{t('chat.dm.searchHint')}</p>
            <input
              autoFocus
              value={userQuery}
              onChange={(e) => setUserQuery(e.target.value)}
              placeholder={t('chat.dm.searchPlaceholder')}
              className="mb-3 w-full rounded-xl border border-border-strong px-3 py-2 text-sm outline-none ring-indigo-500 focus:ring-2"
            />
            <ul className="max-h-56 overflow-y-auto rounded-xl border border-border">
              {searchLoading ? (
                <li className="px-3 py-4 text-center text-sm text-muted">{t('common.loading')}</li>
              ) : !(searchHits ?? []).filter((u) => u.id !== currentUserId).length ? (
                <li className="px-3 py-4 text-center text-sm text-muted">{t('chat.dm.noResults')}</li>
              ) : (
                (searchHits ?? [])
                  .filter((u) => u.id !== currentUserId)
                  .map((u) => (
                    <li key={u.id} className="border-b border-slate-50 last:border-0">
                      <button
                        type="button"
                        className="flex w-full items-center gap-3 px-3 py-2.5 text-left hover:bg-background"
                        onClick={() => void startDmWithUser(u.id)}
                      >
                        <ChatPeerAvatar name={u.fullName} className="h-10 w-10 text-[11px]" />
                        <span className="min-w-0 flex-1">
                          <span className="block truncate font-medium text-foreground">{u.fullName}</span>
                          <span className="block truncate text-xs text-muted">{u.email}</span>
                        </span>
                      </button>
                    </li>
                  ))
              )}
            </ul>
          </div>
        </div>
      )}

      <section
        className={`flex min-h-0 flex-1 flex-col overflow-hidden ${showThread ? 'flex' : 'hidden'} md:flex`}
      >
        {narrow && selected && (
          <button
            type="button"
            className="mb-2 inline-flex items-center text-sm text-primary hover:underline"
            onClick={backToList}
          >
            ← {t('chat.mobile.back')}
          </button>
        )}
        {!selected ? (
          <div className="flex flex-1 flex-col items-center justify-center rounded-xl border border-dashed border-border p-8 text-center text-muted">
            <p className="max-w-sm text-sm">{t('chat.thread.pickConversation')}</p>
          </div>
        ) : (
          <>
            <div className="flex min-h-0 flex-1 flex-col overflow-hidden">
            <ChatConversationPanel
              threadRef={selected}
              currentUserId={currentUserId}
              conversationTitle={
                selected.kind === 'conversation' ? titleForSelected() : null
              }
              conversationSubtitle={
                selected.kind === 'conversation' ? (
                  <span
                    className={`truncate mt-0.5 text-xs ${selectedPeerActiveNow ? 'inline-flex items-center gap-1 text-emerald-600' : 'text-muted'}`}
                  >
                    {selectedPeerActiveNow && (
                      <span className="inline-block h-2 w-2 rounded-full bg-emerald-500" aria-hidden />
                    )}
                    {subtitleForSelected()}
                  </span>
                ) : null
              }
              conversationType={selectedConversation?.type ?? null}
              peerUserId={
                selectedConversation?.directPeerUserId ?? null
              }
              isMuted={selectedConversation?.isMuted ?? false}
              isBlockedWithPeer={selectedConversation?.isBlockedWithPeer ?? false}
              onPeerBlocked={() => {
                setSelected(null)
                if (narrow) setPanel('list')
              }}
              headerLeft={
                <>
                  <button
                    type="button"
                    className="p-1 md:hidden -ml-2 text-muted hover:text-foreground transition-colors shrink-0"
                    onClick={backToList}
                    aria-label={t('common.back')}
                  >
                    <ArrowLeft className="h-5 w-5" />
                  </button>
                  {selected.kind === 'conversation' &&
                    (() => {
                      const c = convos?.find((x) => x.id === selected.conversationId)
                      if (!c) return null
                      return (
                        <ChatPeerAvatar
                          name={primaryConversationTitle(c, currentUserId)}
                          className="hidden sm:inline-flex shrink-0"
                        />
                      )
                    })()}
                </>
              }
              headerActions={
                <ChatCallBar threadRef={selected} conversation={selectedConversation ?? null} />
              }
            />
            </div>
          </>
        )}
      </section>
    </div>
  )
}
