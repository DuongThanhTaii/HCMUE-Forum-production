import { useCallback, useEffect, useState } from 'react'
import {
  listPendingOutbox,
  openOutboxDb,
  purgeFailedOutboxForConversation,
} from '../lib/outboxDb'
import { drainChatOutbox } from '../lib/processOutbox'
import { MAX_SEND_ATTEMPTS } from '../constants/outbox'

export type OutboxBannerState =
  | { kind: 'idle' }
  | { kind: 'pending'; count: number }
  | { kind: 'failed'; count: number }

export function useChatOutboxBanner(conversationId: string | null) {
  const [state, setState] = useState<OutboxBannerState>({ kind: 'idle' })

  const refresh = useCallback(async () => {
    if (!conversationId) {
      setState({ kind: 'idle' })
      return
    }
    try {
      const db = await openOutboxDb()
      const rows = await listPendingOutbox(db)
      const mine = rows.filter((r) => r.conversationId === conversationId)
      if (mine.length === 0) {
        setState({ kind: 'idle' })
        return
      }
      const failed = mine.filter((r) => r.attempts >= MAX_SEND_ATTEMPTS)
      if (failed.length > 0) {
        setState({ kind: 'failed', count: failed.length })
      } else {
        setState({ kind: 'pending', count: mine.length })
      }
    } catch {
      setState({ kind: 'idle' })
    }
  }, [conversationId])

  useEffect(() => {
    if (!conversationId) return
    void (async () => {
      const db = await openOutboxDb()
      await purgeFailedOutboxForConversation(db, conversationId, MAX_SEND_ATTEMPTS)
      await refresh()
    })()
  }, [conversationId, refresh])

  useEffect(() => {
    const id = window.setInterval(() => void refresh(), 5000)
    return () => window.clearInterval(id)
  }, [refresh])

  const retry = useCallback(async () => {
    await drainChatOutbox()
    await refresh()
  }, [refresh])

  const dismissFailed = useCallback(async () => {
    if (!conversationId) return
    const db = await openOutboxDb()
    await purgeFailedOutboxForConversation(db, conversationId, MAX_SEND_ATTEMPTS)
    await refresh()
  }, [conversationId, refresh])

  return { state, refresh, retry, dismissFailed }
}
