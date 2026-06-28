import { store } from '../../../app/store'
import { chatApi } from '../api/chat.api'
import { MAX_SEND_ATTEMPTS, outboxRetryDelayMs } from '../constants/outbox'
import {
  listPendingOutbox,
  openOutboxDb,
  removeOutbox,
  updateOutboxAttempts,
} from './outboxDb'

export async function drainChatOutbox(): Promise<void> {
  const db = await openOutboxDb()
  const pending = await listPendingOutbox(db)
  for (const row of pending) {
    if (row.attempts >= MAX_SEND_ATTEMPTS) {
      continue
    }
    try {
      await store
        .dispatch(
          chatApi.endpoints.sendMessage.initiate({
            conversationId: row.conversationId,
            content: row.body.content,
            replyToMessageId: row.body.replyToMessageId,
          })
        )
        .unwrap()
      await removeOutbox(db, row.id)
    } catch (err) {
      const nextAttempts = row.attempts + 1
      const msg = err instanceof Error ? err.message : String(err)
      await updateOutboxAttempts(db, row.id, nextAttempts, msg)
      await new Promise((r) => setTimeout(r, outboxRetryDelayMs(nextAttempts)))
    }
  }
}
