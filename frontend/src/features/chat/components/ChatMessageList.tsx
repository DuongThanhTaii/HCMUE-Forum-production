import type { RefObject, ReactNode } from 'react'
import { useTranslation } from 'react-i18next'
import { ScrollToBottomFab } from './ScrollToBottomFab'

export function ChatMessageList({
  containerRef,
  onScroll,
  showScrollFab,
  pendingNewCount,
  onScrollToBottom,
  isLoadingOlder,
  hasMoreOlder,
  isInitialLoading,
  empty,
  children,
}: {
  containerRef: RefObject<HTMLDivElement | null>
  onScroll: React.UIEventHandler<HTMLDivElement>
  showScrollFab: boolean
  pendingNewCount: number
  onScrollToBottom: () => void
  isLoadingOlder?: boolean
  hasMoreOlder?: boolean
  isInitialLoading?: boolean
  empty?: ReactNode
  children: ReactNode
}) {
  const { t } = useTranslation()

  return (
    <div className="relative min-h-0 flex-1">
      <div
        ref={containerRef}
        data-chat-scroll
        onScroll={onScroll}
        className="h-full min-h-0 space-y-2 overflow-y-auto overscroll-contain rounded-lg border border-slate-200 bg-white p-3"
      >
        {hasMoreOlder ? (
          <div className="flex justify-center py-1">
            {isLoadingOlder ? (
              <span className="text-xs text-slate-500">{t('chat.scroll.loadingOlder')}</span>
            ) : (
              <span className="text-[10px] text-slate-400">{t('chat.scroll.pullOlderHint')}</span>
            )}
          </div>
        ) : null}
        {isInitialLoading ? (
          <p className="text-center text-sm text-slate-500">{t('common.loading')}</p>
        ) : empty ? (
          empty
        ) : (
          children
        )}
      </div>
      {showScrollFab ? (
        <ScrollToBottomFab pendingCount={pendingNewCount} onClick={onScrollToBottom} />
      ) : null}
    </div>
  )
}
