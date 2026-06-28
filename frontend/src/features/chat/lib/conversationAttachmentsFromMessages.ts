import type { ConversationAttachmentDto, MessageDto } from '../types/chat.types'

export function conversationAttachmentsFromMessages(
  messages: MessageDto[],
): ConversationAttachmentDto[] {
  const out: ConversationAttachmentDto[] = []
  for (const m of messages) {
    if (m.isDeleted) continue
    for (const a of m.attachments ?? []) {
      out.push({
        messageId: m.id,
        sentAt: m.sentAt,
        fileName: a.fileName,
        fileUrl: a.fileUrl,
        mimeType: a.mimeType,
        thumbnailUrl: a.thumbnailUrl ?? null,
        fileSize: a.fileSize,
      })
    }
  }
  return out
}

export function mergeConversationAttachments(
  ...lists: ConversationAttachmentDto[][]
): ConversationAttachmentDto[] {
  const byKey = new Map<string, ConversationAttachmentDto>()
  for (const list of lists) {
    for (const item of list) {
      byKey.set(`${item.messageId}:${item.fileUrl}`, item)
    }
  }
  return [...byKey.values()].sort(
    (a, b) => new Date(b.sentAt).getTime() - new Date(a.sentAt).getTime(),
  )
}
