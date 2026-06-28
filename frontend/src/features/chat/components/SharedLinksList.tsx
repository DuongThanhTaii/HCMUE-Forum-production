import { ExternalLink } from 'lucide-react'
import type { ConversationLinkDto } from '../types/chat.types'

export function SharedLinksList({
  items,
  onSelect,
}: {
  items: ConversationLinkDto[]
  onSelect?: (item: ConversationLinkDto) => void
}) {
  return (
    <ul className="space-y-1">
      {items.map((item) => (
        <li key={`${item.messageId}-${item.url}`}>
          <a
            href={item.url}
            target="_blank"
            rel="noopener noreferrer"
            className="flex cursor-pointer items-start gap-2 rounded-lg px-2 py-2 hover:bg-slate-50"
            onClick={(e) => {
              if (onSelect) {
                e.preventDefault()
                onSelect(item)
              }
            }}
          >
            <ExternalLink className="mt-0.5 h-4 w-4 shrink-0 text-indigo-600" aria-hidden />
            <span className="min-w-0 flex-1">
              <span className="block truncate text-sm font-medium text-indigo-700">{item.host}</span>
              <span className="block truncate text-xs text-slate-500">{item.url}</span>
              <time className="mt-0.5 block text-[10px] text-slate-400">
                {new Date(item.sentAt).toLocaleString()}
              </time>
            </span>
          </a>
        </li>
      ))}
    </ul>
  )
}
