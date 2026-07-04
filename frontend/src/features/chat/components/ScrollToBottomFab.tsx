import { ChevronDown } from 'lucide-react'
import { useTranslation } from 'react-i18next'

export function ScrollToBottomFab({
  pendingCount,
  onClick,
}: {
  pendingCount: number
  onClick: () => void
}) {
  const { t } = useTranslation()
  const label =
    pendingCount > 0
      ? t('chat.scroll.newMessages', { count: pendingCount })
      : t('chat.scroll.toBottom')

  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={label}
      className="absolute bottom-3 right-3 z-10 flex cursor-pointer items-center gap-1 rounded-full border border-border bg-surface px-2.5 py-1.5 text-xs font-medium text-slate-700 shadow-md transition-colors hover:border-indigo-200 hover:bg-primary/10 hover:text-indigo-700"
    >
      <ChevronDown className="h-4 w-4 shrink-0" aria-hidden />
      {pendingCount > 0 ? (
        <span className="tabular-nums">{pendingCount > 99 ? '99+' : pendingCount}</span>
      ) : null}
    </button>
  )
}
