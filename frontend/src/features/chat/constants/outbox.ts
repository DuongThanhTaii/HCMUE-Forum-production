/** Max pending outbox items (spec §4.8). */
export const MAX_OUTBOX_ITEMS = 50
/** Retries before giving up on a single item. */
export const MAX_SEND_ATTEMPTS = 5

/** Backoff base for retries (ms): min(30000, 1000 * 2^attempts). */
export function outboxRetryDelayMs(attempts: number): number {
  return Math.min(30_000, 1000 * Math.pow(2, attempts))
}
