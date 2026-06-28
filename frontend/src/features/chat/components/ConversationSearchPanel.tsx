import { useCallback, useEffect, useRef, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { Search, X } from 'lucide-react'
import { useConversationSearch } from '../hooks/useConversationSearch'
import { highlightSearchSnippet } from '../lib/highlightSearchSnippet'
import type { MessageSearchHitDto } from '../types/chat.types'

export function ConversationSearchPanel({
  conversationId,
  open,
  onClose,
  onSelectHit,
}: {
  conversationId: string
  open: boolean
  onClose: () => void
  onSelectHit: (hit: MessageSearchHitDto) => void | Promise<void>
}) {
  const { t } = useTranslation()
  const inputRef = useRef<HTMLInputElement | null>(null)
  const [selectedIndex, setSelectedIndex] = useState(0)
  const { q, setQ, debouncedQ, canSearch, results, isFetching, isError } =
    useConversationSearch(conversationId, 'text')

  useEffect(() => {
    if (!open) return
    setSelectedIndex(0)
    const tId = window.setTimeout(() => inputRef.current?.focus(), 0)
    return () => window.clearTimeout(tId)
  }, [open])

  useEffect(() => {
    setSelectedIndex(0)
  }, [results, debouncedQ])

  const pickHit = useCallback(
    async (hit: MessageSearchHitDto) => {
      await onSelectHit(hit)
      onClose()
    },
    [onClose, onSelectHit],
  )

  const onKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === 'Escape') {
      event.preventDefault()
      onClose()
      return
    }

    if (!results.length) {
      if (event.key === 'Enter' && canSearch && results.length === 0) {
        event.preventDefault()
      }
      return
    }

    if (event.key === 'ArrowDown') {
      event.preventDefault()
      setSelectedIndex((i) => Math.min(i + 1, results.length - 1))
      return
    }

    if (event.key === 'ArrowUp') {
      event.preventDefault()
      setSelectedIndex((i) => Math.max(i - 1, 0))
      return
    }

    if (event.key === 'Enter') {
      event.preventDefault()
      const hit = results[selectedIndex]
      if (hit) {
        void pickHit(hit)
      }
    }
  }

  if (!open) {
    return null
  }

  return (
    <div
      className="absolute inset-0 z-20 flex flex-col bg-white/95 backdrop-blur-sm"
      role="dialog"
      aria-label={t('chat.search.title')}
      onKeyDown={onKeyDown}
    >
      <div className="flex items-center gap-2 border-b border-slate-200 px-2 py-2">
        <Search className="h-4 w-4 shrink-0 text-slate-400" aria-hidden />
        <input
          ref={inputRef}
          type="search"
          value={q}
          onChange={(e) => setQ(e.target.value)}
          placeholder={t('chat.search.placeholder')}
          className="min-w-0 flex-1 bg-transparent text-sm text-slate-900 outline-none placeholder:text-slate-400"
          autoComplete="off"
          aria-controls="conversation-search-results"
        />
        <button
          type="button"
          onClick={onClose}
          className="cursor-pointer rounded-lg p-1.5 text-slate-500 hover:bg-slate-100"
          aria-label={t('common.cancel')}
        >
          <X className="h-4 w-4" />
        </button>
      </div>

      <div id="conversation-search-results" className="min-h-0 flex-1 overflow-y-auto px-2 py-2">
        {!canSearch ? (
          <p className="px-1 py-4 text-center text-sm text-slate-500">{t('chat.search.minChars')}</p>
        ) : isFetching ? (
          <p className="px-1 py-4 text-center text-sm text-slate-500">{t('common.loading')}</p>
        ) : isError ? (
          <p className="px-1 py-4 text-center text-sm text-red-600">{t('chat.search.error')}</p>
        ) : results.length === 0 ? (
          <p className="px-1 py-4 text-center text-sm text-slate-500">{t('chat.search.noResults')}</p>
        ) : (
          <ul className="space-y-1">
            {results.map((hit, index) => {
              const label =
                hit.senderDisplayName?.trim() ||
                `${hit.senderId.slice(0, 8)}…`
              const active = index === selectedIndex
              return (
                <li key={hit.messageId}>
                  <button
                    type="button"
                    className={`w-full cursor-pointer rounded-lg px-3 py-2 text-left text-sm transition-colors ${
                      active
                        ? 'bg-indigo-50 ring-1 ring-indigo-200'
                        : 'hover:bg-slate-50'
                    }`}
                    onMouseEnter={() => setSelectedIndex(index)}
                    onClick={() => void pickHit(hit)}
                  >
                    <div className="flex items-baseline justify-between gap-2">
                      <span className="font-medium text-slate-800">{label}</span>
                      <time className="shrink-0 text-[10px] text-slate-400">
                        {new Date(hit.sentAt).toLocaleString()}
                      </time>
                    </div>
                    <p className="mt-0.5 line-clamp-2 text-slate-600">
                      {highlightSearchSnippet(hit.snippet, debouncedQ)}
                    </p>
                  </button>
                </li>
              )
            })}
          </ul>
        )}
      </div>
    </div>
  )
}
