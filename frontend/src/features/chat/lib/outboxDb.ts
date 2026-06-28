import { MAX_OUTBOX_ITEMS } from '../constants/outbox'
import { shouldBlockEnqueue } from './outboxPolicy'

const DB_NAME = 'unihub-chat-outbox'
const DB_VERSION = 1
const STORE = 'messages'

export type OutboxTextBody = {
  type: 'text'
  content: string
  replyToMessageId?: string
}

export type OutboxRecord = {
  id: string
  conversationId: string
  body: OutboxTextBody
  attempts: number
  createdAt: number
  lastError?: string
}

function openRequest(): IDBOpenDBRequest {
  return indexedDB.open(DB_NAME, DB_VERSION)
}

export function openOutboxDb(): Promise<IDBDatabase> {
  return new Promise((resolve, reject) => {
    const req = openRequest()
    req.onerror = () => reject(req.error)
    req.onsuccess = () => {
      const db = req.result
      resolve(db)
    }
    req.onupgradeneeded = () => {
      const db = req.result
      if (!db.objectStoreNames.contains(STORE)) {
        db.createObjectStore(STORE, { keyPath: 'id' })
      }
    }
  })
}

export async function outboxCount(db: IDBDatabase): Promise<number> {
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE, 'readonly')
    const store = tx.objectStore(STORE)
    const req = store.count()
    req.onerror = () => reject(req.error)
    req.onsuccess = () => resolve(req.result)
  })
}

export async function listPendingOutbox(db: IDBDatabase): Promise<OutboxRecord[]> {
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE, 'readonly')
    const store = tx.objectStore(STORE)
    const req = store.getAll()
    req.onerror = () => reject(req.error)
    req.onsuccess = () => {
      const rows = (req.result as OutboxRecord[]) ?? []
      resolve(rows.sort((a, b) => a.createdAt - b.createdAt))
    }
  })
}

export async function enqueueOutbox(
  db: IDBDatabase,
  record: OutboxRecord
): Promise<'ok' | 'full'> {
  const n = await outboxCount(db)
  if (shouldBlockEnqueue(n, MAX_OUTBOX_ITEMS)) {
    return 'full'
  }
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE, 'readwrite')
    const store = tx.objectStore(STORE)
    const req = store.put(record)
    req.onerror = () => reject(req.error)
    tx.oncomplete = () => resolve('ok')
  })
}

export async function removeOutbox(db: IDBDatabase, id: string): Promise<void> {
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE, 'readwrite')
    const store = tx.objectStore(STORE)
    const req = store.delete(id)
    req.onerror = () => reject(req.error)
    tx.oncomplete = () => resolve()
  })
}

export async function clearOutboxForConversation(
  db: IDBDatabase,
  conversationId: string,
): Promise<void> {
  const rows = await listPendingOutbox(db)
  const mine = rows.filter((r) => r.conversationId === conversationId)
  await Promise.all(mine.map((r) => removeOutbox(db, r.id)))
}

export async function purgeFailedOutboxForConversation(
  db: IDBDatabase,
  conversationId: string,
  maxAttempts: number,
): Promise<void> {
  const rows = await listPendingOutbox(db)
  const dead = rows.filter(
    (r) => r.conversationId === conversationId && r.attempts >= maxAttempts,
  )
  await Promise.all(dead.map((r) => removeOutbox(db, r.id)))
}

export async function updateOutboxAttempts(
  db: IDBDatabase,
  id: string,
  attempts: number,
  lastError?: string
): Promise<void> {
  return new Promise((resolve, reject) => {
    const tx = db.transaction(STORE, 'readwrite')
    const store = tx.objectStore(STORE)
    const getReq = store.get(id)
    getReq.onerror = () => reject(getReq.error)
    getReq.onsuccess = () => {
      const row = getReq.result as OutboxRecord | undefined
      if (!row) {
        resolve()
        return
      }
      const next: OutboxRecord = { ...row, attempts, lastError }
      const putReq = store.put(next)
      putReq.onerror = () => reject(putReq.error)
      tx.oncomplete = () => resolve()
    }
  })
}
