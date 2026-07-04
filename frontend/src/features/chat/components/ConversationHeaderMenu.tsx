import { useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { BellOff, BellRing, ShieldBan, Info, Search, Maximize2, ChevronDown } from 'lucide-react'
import { useBlockUserMutation, useSetConversationMuteMutation } from '../api/chat.api'
import { Link } from 'react-router-dom'

export function ConversationHeaderMenu({
  conversationId,
  peerUserId,
  peerName,
  subtitle,
  isMuted,
  isBlockedWithPeer,
  onBlocked,
  onOpenInfo,
  onOpenSearch,
  expandUrl,
}: {
  conversationId: string
  peerUserId: string | null
  peerName?: React.ReactNode | null
  subtitle?: React.ReactNode | null
  isMuted: boolean
  isBlockedWithPeer: boolean
  onBlocked?: () => void
  onOpenInfo?: () => void
  onOpenSearch?: () => void
  expandUrl?: string
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
    const label = (typeof peerName === 'string' ? peerName.trim() : null) || peerUserId.slice(0, 8)
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
        className="flex items-center gap-1 rounded px-2 py-1 hover:bg-slate-50 transition-colors text-left"
        aria-label={t('chat.safety.menu')}
      >
        <div className="min-w-0">
          <p className="truncate text-sm font-semibold text-slate-900">{peerName}</p>
          {subtitle && (
            <p className="truncate text-[11px] text-slate-500">{subtitle}</p>
          )}
        </div>
        <ChevronDown className="h-4 w-4 text-slate-500 shrink-0" />
      </button>

      {open && (
        <div className="absolute left-0 z-20 mt-1 min-w-[14rem] rounded-lg border border-slate-200 bg-white py-1 text-sm shadow-lg">
          <button
            type="button"
            className="flex w-full cursor-pointer items-center gap-2 px-3 py-2 text-left hover:bg-slate-50"
            onClick={() => {
              setOpen(false)
              onOpenInfo?.()
            }}
          >
            <Info className="h-4 w-4" />
            {t('chat.info.media', 'Media, file & link')}
          </button>
          <button
            type="button"
            className="flex w-full cursor-pointer items-center gap-2 px-3 py-2 text-left hover:bg-slate-50"
            onClick={() => {
              setOpen(false)
              onOpenSearch?.()
            }}
          >
            <Search className="h-4 w-4" />
            {t('chat.search.open', 'Tìm kiếm tin nhắn')}
          </button>
          
          {expandUrl && (
            <Link
              to={expandUrl}
              className="flex w-full cursor-pointer items-center gap-2 px-3 py-2 text-left hover:bg-slate-50"
              onClick={() => setOpen(false)}
            >
              <Maximize2 className="h-4 w-4" />
              {t('chat.dock.expandFull', 'Mở rộng trò chuyện')}
            </Link>
          )}
          
          <div className="my-1 h-px bg-slate-100" />
          
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
