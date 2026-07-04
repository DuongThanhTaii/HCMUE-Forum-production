import { resolveChatAssetUrl } from '../lib/mediaUrl'
import type { ConversationAttachmentDto } from '../types/chat.types'

export function SharedMediaGrid({
  items,
  onSelect,
}: {
  items: ConversationAttachmentDto[]
  onSelect?: (item: ConversationAttachmentDto) => void
}) {
  return (
    <div className="grid grid-cols-3 gap-1.5 sm:grid-cols-4">
      {items.map((item) => {
        const src = resolveChatAssetUrl(item.thumbnailUrl ?? item.fileUrl)
        return (
          <button
            key={`${item.messageId}-${item.fileUrl}`}
            type="button"
            className="aspect-square cursor-pointer overflow-hidden rounded-lg border border-border bg-background"
            onClick={() => onSelect?.(item)}
          >
            <img src={src} alt={item.fileName} className="h-full w-full object-cover" loading="lazy" />
          </button>
        )
      })}
    </div>
  )
}
