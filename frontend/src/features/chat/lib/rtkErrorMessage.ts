import type { FetchBaseQueryError } from '@reduxjs/toolkit/query'

/** Pull user-visible message from RTK Query / ApiResponse failure payloads. */
export function getRtkQueryErrorMessage(err: unknown): string {
  if (!err || typeof err !== 'object') return ''
  const e = err as FetchBaseQueryError
  const data = e.data as Record<string, unknown> | string | undefined
  if (typeof data === 'string' && data.trim()) return data.trim()
  if (data && typeof data === 'object') {
    const msg = data.message ?? data.error
    if (typeof msg === 'string' && msg.trim()) return msg.trim()
    const inner = data.data
    if (inner && typeof inner === 'object') {
      const m = (inner as Record<string, unknown>).message
      if (typeof m === 'string' && m.trim()) return m.trim()
    }
  }
  if ('status' in e && e.status === 'FETCH_ERROR' && typeof e.error === 'string') {
    return e.error
  }
  return ''
}
