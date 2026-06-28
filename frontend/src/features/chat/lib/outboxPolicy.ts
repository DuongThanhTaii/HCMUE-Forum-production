import { MAX_OUTBOX_ITEMS } from '../constants/outbox'

export function shouldBlockEnqueue(count: number, max = MAX_OUTBOX_ITEMS): boolean {
  return count >= max
}
