import type { ConversationDto } from '../types/chat.types'

export function primaryConversationTitle(c: ConversationDto, selfId: string | null): string {
  const type = c.type?.toLowerCase() ?? ''
  if (type.includes('group')) {
    const t = c.title?.trim()
    return t && t.length > 0 ? t : `Group (${c.participantIds.length})`
  }
  const named = c.directPeerFullName?.trim()
  if (named) return named
  const other = c.participantIds.find((id) => id !== selfId)
  return other ? `…${other.slice(0, 8)}` : 'Direct'
}

export function conversationSubtitle(c: ConversationDto): string | null {
  const type = c.type?.toLowerCase() ?? ''
  if (type.includes('group')) return null
  const email = c.directPeerEmail?.trim()
  return email && email.length > 0 ? email : null
}
