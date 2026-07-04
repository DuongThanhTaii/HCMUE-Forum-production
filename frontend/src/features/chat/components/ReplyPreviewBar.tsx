import { X } from 'lucide-react'
import { useTranslation } from 'react-i18next'
import type { ReplyTarget } from '../types/chat.types'

export function ReplyPreviewBar({
  target,
  onClear,
}: {
  target: ReplyTarget
  onClear: () => void
}) {
  const { t } = useTranslation()
  return (
    <div className="flex items-start gap-2 rounded-lg border border-indigo-100 bg-primary/10/80 px-3 py-2 text-sm">
      <div className="min-w-0 flex-1 border-l-2 border-indigo-400 pl-2">
        <p className="text-[10px] font-medium uppercase tracking-wide text-indigo-700">
          {t('chat.reply.replyingTo', { name: target.senderLabel })}
        </p>
        <p className="line-clamp-2 text-slate-700">{target.preview}</p>
      </div>
      <button
        type="button"
        onClick={onClear}
        className="shrink-0 rounded p-1 text-muted hover:bg-surface/80"
        aria-label={t('chat.reply.cancel')}
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  )
}
