import { useCallback, useEffect, useMemo, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { MoreHorizontal, Phone, PhoneMissed, SmilePlus } from 'lucide-react'
import {
  useAddMessageReactionMutation,
  useDeleteMessageMutation,
  useEditMessageMutation,
  useRemoveMessageReactionMutation,
  useReportMessageMutation,
} from '../api/chat.api'
import type { ChatMessageReportReason, MessageDto } from '../types/chat.types'
import { resolveChatAssetUrl } from '../lib/mediaUrl'
import { scrollElementWithinContainer } from '../lib/scrollWithinContainer'
import { MessageActionsMenu } from './MessageActionsMenu'
import { EMOJI_ASSETS, ReactionPicker } from './ReactionPicker'
import { ReadReceiptIndicator } from './ReadReceiptIndicator'
import { VoiceMessagePlayer } from './VoiceMessagePlayer'

const EDIT_WINDOW_MS = 15 * 60 * 1000

function primaryMime(mime: string): string {
  const s = mime.trim()
  const i = s.indexOf(';')
  return i >= 0 ? s.slice(0, i).trim().toLowerCase() : s.toLowerCase()
}

function isAudioMime(mime: string): boolean {
  return primaryMime(mime).startsWith('audio/')
}

function isImageMime(mime: string): boolean {
  return primaryMime(mime).startsWith('image/')
}

function replyPreviewText(m: MessageDto, t: (k: string) => string): string {
  const text = m.content?.trim()
  if (text) return text.length > 120 ? `${text.slice(0, 120)}…` : text
  if ((m.attachments?.length ?? 0) > 0) return t('chat.reply.attachmentPreview')
  return t('chat.reply.emptyPreview')
}

export function MessageBubble({
  message,
  isSelf,
  conversationId,
  currentUserId,
  highlighted = false,
  messageById,
  onReply,
  isLastOwnMessage = false,
  isDirectDm = false,
  peerUserId = null,
}: {
  message: MessageDto
  isSelf: boolean
  conversationId: string | null
  currentUserId: string | null
  highlighted?: boolean
  messageById: Map<string, MessageDto>
  onReply?: (message: MessageDto) => void
  isLastOwnMessage?: boolean
  isDirectDm?: boolean
  peerUserId?: string | null
}) {
  const { t } = useTranslation()
  const [editMessage, editState] = useEditMessageMutation()
  const [deleteMessage, deleteState] = useDeleteMessageMutation()
  const [reportMessage] = useReportMessageMutation()
  const [addReaction] = useAddMessageReactionMutation()
  const [removeReaction] = useRemoveMessageReactionMutation()
  const [menuOpen, setMenuOpen] = useState(false)
  const [pickerOpen, setPickerOpen] = useState(false)
  const [editing, setEditing] = useState(false)
  const [localReactions, setLocalReactions] = useState(message.reactions)
  const trimmed = message.content?.trim() ?? ''
  const [draft, setDraft] = useState(() => trimmed)
  const menuRef = useRef<HTMLDivElement | null>(null)
  const hoverRef = useRef<HTMLDivElement | null>(null)

  useEffect(() => {
    setLocalReactions(message.reactions)
  }, [message.reactions])

  useEffect(() => {
    if (!menuOpen && !pickerOpen) return
    const close = (e: MouseEvent) => {
      const root = menuRef.current
      if (root && !root.contains(e.target as Node)) {
        setMenuOpen(false)
        setPickerOpen(false)
      }
    }
    document.addEventListener('mousedown', close)
    return () => document.removeEventListener('mousedown', close)
  }, [menuOpen, pickerOpen])

  const attachments = message.attachments ?? []
  const audioItems = attachments.filter((a) => isAudioMime(a.mimeType))
  const imageItems = attachments.filter((a) => isImageMime(a.mimeType))
  const fileItems = attachments.filter((a) => !isAudioMime(a.mimeType) && !isImageMime(a.mimeType))
  const hasText = trimmed.length > 0
  const typeUpper = (message.type ?? '').toUpperCase()
  const withinEditWindow =
    Date.now() - new Date(message.sentAt).getTime() < EDIT_WINDOW_MS
  const canModify =
    Boolean(conversationId) &&
    isSelf &&
    !message.isDeleted &&
    typeUpper !== 'SYSTEM' &&
    typeUpper !== 'MISSEDCALL' &&
    typeUpper !== 'CALLENDED'
  const canEdit = canModify && hasText && withinEditWindow
  const canReport = Boolean(conversationId) && !isSelf && !message.isDeleted
  const canReply = Boolean(conversationId) && !message.isDeleted && onReply
  const canCopy = hasText && !message.isDeleted
  const canReact = Boolean(conversationId) && !message.isDeleted

  const replyParent = message.replyToMessageId
    ? messageById.get(message.replyToMessageId)
    : undefined

  const reactionEntries = useMemo(
    () => Object.entries(localReactions).filter(([, users]) => users.length > 0),
    [localReactions],
  )

  const toggleReaction = useCallback(
    async (emoji: string) => {
      if (!conversationId || !currentUserId) return
      const users = localReactions[emoji] ?? []
      const hasMine = users.includes(currentUserId)
      setPickerOpen(false)
      const next = { ...localReactions }
      if (hasMine) {
        next[emoji] = users.filter((id) => id !== currentUserId)
        if (next[emoji].length === 0) delete next[emoji]
        setLocalReactions(next)
        try {
          await removeReaction({ messageId: message.id, conversationId, emoji }).unwrap()
        } catch {
          setLocalReactions(message.reactions)
        }
      } else {
        next[emoji] = [...users, currentUserId]
        setLocalReactions(next)
        try {
          await addReaction({ messageId: message.id, conversationId, emoji }).unwrap()
        } catch {
          setLocalReactions(message.reactions)
        }
      }
    },
    [
      addReaction,
      conversationId,
      currentUserId,
      localReactions,
      message.id,
      message.reactions,
      removeReaction,
    ],
  )

  const submitReport = async () => {
    if (!conversationId) return
    const reason = window.prompt(
      `${t('chat.safety.reportTitle')}\n${t('chat.safety.reasonSpam')} / Harassment / Inappropriate / Other`,
      'Spam',
    ) as ChatMessageReportReason | null
    if (!reason) return
    const description = window.prompt(t('chat.safety.reportPlaceholder')) ?? ''
    try {
      await reportMessage({
        messageId: message.id,
        reason,
        description: description || null,
      }).unwrap()
      setMenuOpen(false)
      window.alert(t('chat.safety.reportSubmitted'))
    } catch {
      window.alert(t('chat.safety.reportError'))
    }
  }

  const [showDeleteModal, setShowDeleteModal] = useState(false)

  const submitEdit = async () => {
    if (!conversationId || !draft.trim()) return
    try {
      await editMessage({
        messageId: message.id,
        conversationId,
        content: draft.trim(),
      }).unwrap()
      setEditing(false)
      setMenuOpen(false)
    } catch {
      /* optional */
    }
  }

  const handleUnsend = async () => {
    if (!conversationId) return
    try {
      await deleteMessage({ messageId: message.id, conversationId }).unwrap()
      setShowDeleteModal(false)
      setMenuOpen(false)
    } catch {
      /* optional */
    }
  }

  const copyText = async () => {
    if (!trimmed) return
    try {
      await navigator.clipboard.writeText(trimmed)
      setMenuOpen(false)
    } catch {
      /* ignore */
    }
  }

  const highlightClass = highlighted ? 'rounded-lg ring-2 ring-indigo-400 ring-offset-2' : ''

  if (typeUpper === 'MISSEDCALL') {
    return (
      <div data-message-id={message.id} className={`flex justify-center py-1.5 ${highlightClass}`}>
        <div className="inline-flex items-center gap-1.5 rounded-full bg-red-50 px-3 py-1 text-xs text-red-600 ring-1 ring-red-100">
          <PhoneMissed className="h-3.5 w-3.5 shrink-0" />
          {isSelf
            ? t('chat.calls.missedCallSent')
            : t('chat.calls.missedCallReceived', {
                name: message.senderDisplayName?.trim() || t('chat.user'),
              })}
        </div>
      </div>
    )
  }

  if (typeUpper === 'CALLENDED') {
    const seconds = Number.parseInt((message.content ?? '').trim(), 10)
    const hasDuration = Number.isFinite(seconds) && seconds > 0
    const durationMinutes = hasDuration ? Math.max(1, Math.round(seconds / 60)) : 0
    return (
      <div data-message-id={message.id} className={`flex justify-center py-1.5 ${highlightClass}`}>
        <div className="inline-flex items-center gap-1.5 rounded-full bg-slate-100 px-3 py-1 text-xs text-slate-500 ring-1 ring-slate-200">
          <Phone className="h-3.5 w-3.5 shrink-0" />
          {hasDuration
            ? t('chat.calls.callEndedWithDuration', { minutes: durationMinutes })
            : t('chat.calls.callEnded')}
        </div>
      </div>
    )
  }

  if (message.isDeleted) {
    return (
      <div
        data-message-id={message.id}
        className={`flex ${isSelf ? 'justify-end' : 'justify-start'} ${highlightClass}`}
      >
        <div
          className={`max-w-[85%] rounded-2xl px-3 py-2 text-sm italic border ${
            isSelf ? 'border-indigo-200 text-indigo-400' : 'border-slate-200 text-slate-400'
          }`}
        >
          {isSelf ? t('chat.message.removedSelf') : t('chat.message.removedOther')}
        </div>
      </div>
    )
  }

  return (
    <div
      data-message-id={message.id}
      className={`group flex items-center gap-2 ${isSelf ? 'justify-end' : 'justify-start'} ${highlightClass}`}
      ref={(el) => {
        hoverRef.current = el
        menuRef.current = el
      }}
    >
      {isSelf && !editing && (canModify || canReport || canReply || canCopy) && (
        <div className={`relative flex items-center gap-1 transition-opacity ${menuOpen || pickerOpen ? 'opacity-100' : 'opacity-100 sm:opacity-0 sm:group-hover:opacity-100'}`}>
          {canReact && (
            <button
              type="button"
              className="rounded-full p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
              aria-label={t('chat.reactions.add')}
              onClick={() => {
                setPickerOpen((o) => !o)
                setMenuOpen(false)
              }}
            >
              <SmilePlus className="h-4 w-4" />
            </button>
          )}
          <button
            type="button"
            className="rounded-full p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
            aria-expanded={menuOpen}
            aria-label={t('chat.message.actions')}
            onClick={() => {
              setMenuOpen((o) => !o)
              setPickerOpen(false)
            }}
          >
            <MoreHorizontal className="h-4 w-4" />
          </button>
          {pickerOpen && (
            <ReactionPicker
              className="left-0"
              onPick={(emoji) => void toggleReaction(emoji)}
            />
          )}
          <MessageActionsMenu
            open={menuOpen}
            className="left-0"
            canCopy={canCopy}
            canReply={Boolean(canReply)}
            canEdit={canEdit}
            canReport={canReport}
            canDelete={canModify}
            deleteLoading={deleteState.isLoading}
            onCopy={() => void copyText()}
            onReply={() => {
              onReply?.(message)
              setMenuOpen(false)
            }}
            onEdit={() => {
              setDraft(trimmed)
              setEditing(true)
              setMenuOpen(false)
            }}
            onReport={() => void submitReport()}
            onDelete={() => setShowDeleteModal(true)}
          />
        </div>
      )}

      <div className={`flex flex-col max-w-[85%] ${isSelf ? 'items-end' : 'items-start'}`}>
        <div
          className={`relative w-full rounded-2xl px-3 py-2 text-sm ${isSelf ? 'bg-[#5b3af5] text-white' : 'bg-slate-100 text-slate-900'}`}
        >
          {replyParent && (
            <button
              type="button"
              className={`mb-2 flex w-full flex-col items-start rounded-xl px-3 py-2 text-left text-xs transition-colors border-l-[3px] shadow-sm ${
                isSelf ? 'bg-black/15 hover:bg-black/20 border-white/50' : 'bg-slate-200 hover:bg-slate-300 border-indigo-500'
              }`}
              onClick={() => {
                const target = document.querySelector(`[data-message-id="${replyParent.id}"]`)
                const scrollRoot = menuRef.current?.closest('[data-chat-scroll]')
                if (target instanceof HTMLElement && scrollRoot instanceof HTMLElement) {
                  scrollElementWithinContainer(scrollRoot, target, 'center')
                }
              }}
            >
              <span className={`font-semibold mb-0.5 ${isSelf ? 'text-white' : 'text-slate-800'}`}>
                {(() => {
                  const parentName = replyParent.senderDisplayName?.trim() || t('chat.user')
                  const parentIsMe = replyParent.senderId === currentUserId
                  return parentIsMe
                    ? t('chat.reply.youRepliedToYourself', 'Bạn đã trả lời chính mình')
                    : t('chat.reply.youRepliedTo', { name: parentName, defaultValue: `Bạn đã trả lời ${parentName}` })
                })()}
              </span>
              <span className={`line-clamp-1 italic ${isSelf ? 'text-white/80' : 'text-slate-500'}`}>
                {replyPreviewText(replyParent, t)}
              </span>
            </button>
          )}
          {editing ? (
            <div className="space-y-2 pt-1">
              <textarea
                value={draft}
                onChange={(e) => setDraft(e.target.value)}
                rows={3}
                className="w-full rounded-lg border border-slate-300 bg-white px-2 py-1.5 text-sm text-slate-900"
              />
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  className="rounded-lg px-2 py-1 text-xs text-slate-600 hover:bg-slate-100"
                  onClick={() => {
                    setEditing(false)
                    setDraft(trimmed)
                  }}
                >
                  {t('common.cancel')}
                </button>
                <button
                  type="button"
                  className="rounded-lg bg-indigo-600 px-2 py-1 text-xs text-white disabled:opacity-50"
                  disabled={editState.isLoading || !draft.trim()}
                  onClick={() => void submitEdit()}
                >
                  {t('chat.message.saveEdit')}
                </button>
              </div>
            </div>
          ) : (
            <div className="space-y-2">
              {audioItems.map((a) => {
                const audioSrc = resolveChatAssetUrl(a.fileUrl)
                return (
                  <VoiceMessagePlayer key={`${message.id}-${audioSrc}`} src={audioSrc} isSelf={isSelf} />
                )
              })}
              {imageItems.map((a) => (
                <a
                  key={`${message.id}-img-${a.fileUrl}`}
                  href={resolveChatAssetUrl(a.fileUrl)}
                  target="_blank"
                  rel="noreferrer"
                  className="block"
                >
                  <img
                    src={resolveChatAssetUrl(a.fileUrl)}
                    alt=""
                    className="max-h-56 max-w-full rounded-lg object-cover"
                  />
                </a>
              ))}
              {fileItems.map((a) => (
                <a
                  key={`${message.id}-file-${a.fileUrl}`}
                  href={resolveChatAssetUrl(a.fileUrl)}
                  target="_blank"
                  rel="noreferrer"
                  className={isSelf ? 'text-indigo-100 underline' : 'text-indigo-600 underline'}
                >
                  {a.fileName || t('chat.attachment')}
                </a>
              ))}
              {hasText && <div className="whitespace-pre-wrap break-words">{trimmed}</div>}
            </div>
          )}

          <div
            className={`mt-1 flex flex-wrap items-center gap-x-2 text-[10px] ${isSelf ? 'text-indigo-200' : 'text-slate-400'}`}
          >
            <span>{new Date(message.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
            {message.editedAt && (
              <span className="italic">{t('chat.message.editedLabel')}</span>
            )}
            {isSelf && isLastOwnMessage && isDirectDm && (
              <ReadReceiptIndicator
                messageId={message.id}
                currentUserId={currentUserId}
                peerUserId={peerUserId}
                enabled
              />
            )}
          </div>
        </div>

        {reactionEntries.length > 0 && (
          <div className="flex flex-wrap gap-1 -mt-2.5 mb-1 ml-2 mr-2 z-10 bg-white dark:bg-slate-800 rounded-full shadow-sm px-1 py-0.5 border border-slate-100">
            {reactionEntries.map(([emoji, users]) => {
              const mine = currentUserId ? users.includes(currentUserId) : false
              return (
                <button
                  key={emoji}
                  type="button"
                  className={`inline-flex items-center gap-1 rounded-full px-1.5 py-0.5 text-xs font-medium ${
                    mine
                      ? 'bg-indigo-50 text-indigo-600'
                      : 'bg-transparent text-slate-600 hover:bg-slate-50'
                  }`}
                  onClick={() => void toggleReaction(emoji)}
                >
                  <img src={EMOJI_ASSETS[emoji] || ''} alt={emoji} className="w-4 h-4 object-contain" />
                  {users.length > 1 && <span>{users.length}</span>}
                </button>
              )
            })}
          </div>
        )}


      </div>
      {!isSelf && !editing && (canModify || canReport || canReply || canCopy) && (
        <div className={`relative flex items-center gap-1 transition-opacity ${menuOpen || pickerOpen ? 'opacity-100' : 'opacity-100 sm:opacity-0 sm:group-hover:opacity-100'}`}>
          <button
            type="button"
            className="rounded-full p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
            aria-expanded={menuOpen}
            aria-label={t('chat.message.actions')}
            onClick={() => {
              setMenuOpen((o) => !o)
              setPickerOpen(false)
            }}
          >
            <MoreHorizontal className="h-4 w-4" />
          </button>
          {canReact && (
            <button
              type="button"
              className="rounded-full p-1.5 text-slate-400 hover:bg-slate-100 hover:text-slate-600"
              aria-label={t('chat.reactions.add')}
              onClick={() => {
                setPickerOpen((o) => !o)
                setMenuOpen(false)
              }}
            >
              <SmilePlus className="h-4 w-4" />
            </button>
          )}
          {pickerOpen && (
            <ReactionPicker
              className="right-0"
              onPick={(emoji) => void toggleReaction(emoji)}
            />
          )}
          <MessageActionsMenu
            open={menuOpen}
            className="right-0"
            canCopy={canCopy}
            canReply={Boolean(canReply)}
            canEdit={canEdit}
            canReport={canReport}
            canDelete={canModify}
            deleteLoading={deleteState.isLoading}
            onCopy={() => void copyText()}
            onReply={() => {
              onReply?.(message)
              setMenuOpen(false)
            }}
            onEdit={() => {
              setDraft(trimmed)
              setEditing(true)
              setMenuOpen(false)
            }}
            onReport={() => void submitReport()}
            onDelete={() => setShowDeleteModal(true)}
          />
        </div>
      )}

      {showDeleteModal && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/50 p-4">
          <div className="w-full max-w-sm rounded-xl bg-white p-5 shadow-xl text-slate-900">
            <h3 className="mb-2 text-lg font-semibold">{t('chat.message.unsendTitle', 'Thu hồi tin nhắn')}</h3>
            <p className="mb-5 text-sm text-slate-600">{t('chat.message.confirmUnsend')}</p>
            <div className="flex justify-end gap-3">
              <button
                type="button"
                className="rounded-lg px-4 py-2 text-sm font-medium text-slate-600 hover:bg-slate-100"
                onClick={() => setShowDeleteModal(false)}
              >
                {t('common.cancel')}
              </button>
              <button
                type="button"
                className="rounded-lg bg-red-600 px-4 py-2 text-sm font-medium text-white hover:bg-red-700"
                onClick={() => void handleUnsend()}
              >
                {t('chat.message.unsend', 'Thu hồi')}
              </button>
            </div>
          </div>
        </div>
      )}

    </div>
  )
}
