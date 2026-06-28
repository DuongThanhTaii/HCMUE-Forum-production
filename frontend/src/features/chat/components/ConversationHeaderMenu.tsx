import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { BellOff, BellRing, MoreVertical, ShieldBan } from 'lucide-react'
import { useBlockUserMutation, useSetConversationMuteMutation } from '../api/chat.api'

export function ConversationHeaderMenu({
  conversationId,
  peerUserId,
  peerName,
  isMuted,
  isBlockedWithPeer,
  onBlocked,
}: {
  conversationId: string
  peerUserId: string | null
  peerName?: string | null
  isMuted: boolean
  isBlockedWithPeer: boolean
  onBlocked?: () => void
}) {
  const { t } = useTranslation()
  const [open, setOpen] = useState(false)
  const menuRef = useRef<HTMLDivElement | null>(null)

  const [setMute, { isLoading: muting }] = useSetConversationMuteMutation()
  const [blockUser, { isLoading: blocking }] = useBlockUserMutation()

  useEffect(() => {
    if (!open) return
    const close = (e: MouseEvent) => {
      if (menuRef.current && !menuRef.current.contains(e.target as Node)) {
        setOpen(false)
      }
    }
    document.addEventListener('mousedown', close)
    return () => document.removeEventListener('mousedown', close)
  }, [open])

  const toggleMute = async () => {
    await setMute({ conversationId, muted: !isMuted }).unwrap()
    setOpen(false)
  }

  const confirmBlock = async () => {
    if (!peerUserId) return
    const label = peerName?.trim() || peerUserId.slice(0, 8)
    if (!window.confirm(t('chat.safety.blockConfirm', { name: label }))) return
    await blockUser(peerUserId).unwrap()
    setOpen(false)
    onBlocked?.()
  }

  return (
    <div className="relative" ref={menuRef}>
      <button
        type="button"
        onClick={() => setOpen((o) => !o)}
        className="cursor-pointer rounded-lg border border-slate-200 p-1.5 text-slate-600 hover:bg-slate-50"
        aria-label={t('chat.safety.menu')}
      >
        <MoreVertical className="h-4 w-4" />
      </button>

      {open && (
        <div className="absolute right-0 z-20 mt-1 min-w-[11rem] rounded-lg border border-slate-200 bg-white py-1 text-sm shadow-lg">
          <button
            type="button"
            className="flex w-full cursor-pointer items-center gap-2 px-3 py-2 text-left hover:bg-slate-50"
            disabled={muting}
            onClick={() => void toggleMute()}
          >
            {isMuted ? <BellRing className="h-4 w-4" /> : <BellOff className="h-4 w-4" />}
            {isMuted ? t('chat.safety.unmute') : t('chat.safety.mute')}
          </button>
          {peerUserId ? (
            <button
              type="button"
              className="flex w-full cursor-pointer items-center gap-2 px-3 py-2 text-left text-red-600 hover:bg-red-50"
              disabled={blocking || isBlockedWithPeer}
              onClick={() => void confirmBlock()}
            >
              <ShieldBan className="h-4 w-4" />
              {t('chat.safety.block')}
            </button>
          ) : null}
        </div>
      )}
    </div>
  )
}
