import { useEffect, useRef } from 'react'
import { useMarkMessageAsReadMutation } from '../api/chat.api'
import type { MessageDto } from '../types/chat.types'

const DEBOUNCE_MS = 500

export function useMarkMessagesRead(
  conversationId: string | null,
  messages: MessageDto[],
  atBottom: boolean,
  currentUserId: string | null,
) {
  const [markRead] = useMarkMessageAsReadMutation()
  const markedRef = useRef<Set<string>>(new Set())
  const timerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  useEffect(() => {
    markedRef.current = new Set()
  }, [conversationId])

  useEffect(() => {
    if (!conversationId || !currentUserId || !atBottom) return

    const latestPeer = [...messages]
      .reverse()
      .find((m) => !m.isDeleted && m.senderId !== currentUserId)

    if (!latestPeer || markedRef.current.has(latestPeer.id)) return

    if (timerRef.current) clearTimeout(timerRef.current)
    timerRef.current = setTimeout(() => {
      markedRef.current.add(latestPeer.id)
      void markRead({ messageId: latestPeer.id, conversationId }).catch(() => {
        markedRef.current.delete(latestPeer.id)
      })
    }, DEBOUNCE_MS)

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current)
    }
  }, [atBottom, conversationId, currentUserId, markRead, messages])
}
