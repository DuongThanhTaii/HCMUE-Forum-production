import { FileText, Mic } from 'lucide-react'
import { resolveChatAssetUrl } from '../lib/mediaUrl'
import type { ConversationAttachmentDto } from '../types/chat.types'

function formatBytes(size: number): string {
  if (size < 1024) return `${size} B`
  if (size < 1024 * 1024) return `${(size / 1024).toFixed(1)} KB`
  return `${(size / (1024 * 1024)).toFixed(1)} MB`
}

function isVoiceMime(mime: string): boolean {
  return mime.trim().toLowerCase().startsWith('audio/')
}

export function SharedFilesList({ items }: { items: ConversationAttachmentDto[] }) {
  return (
    <ul className="space-y-1">
      {items.map((item) => {
        const href = resolveChatAssetUrl(item.fileUrl)
        const voice = isVoiceMime(item.mimeType)
        return (
          <li key={`${item.messageId}-${item.fileUrl}`}>
            <a
              href={href}
              target="_blank"
              rel="noopener noreferrer"
              className="flex cursor-pointer items-center gap-3 rounded-lg px-2 py-2 hover:bg-background"
            >
              <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-background text-muted">
                {voice ? <Mic className="h-4 w-4" /> : <FileText className="h-4 w-4" />}
              </span>
              <span className="min-w-0 flex-1">
                <span className="block truncate text-sm font-medium text-foreground">{item.fileName}</span>
                <span className="block text-[11px] text-muted">
                  {formatBytes(item.fileSize)} · {new Date(item.sentAt).toLocaleString()}
                </span>
              </span>
            </a>
          </li>
        )
      })}
    </ul>
  )
}
