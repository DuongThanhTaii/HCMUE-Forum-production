import type { ReadReceiptDto } from '../types/chat.types'

export function formatReadReceiptLabel(
  receipts: ReadReceiptDto[],
  currentUserId: string | null,
  peerUserId: string | null,
): string | null {
  const others = receipts.filter((r) => r.userId && r.userId !== currentUserId)
  if (others.length === 0) return null
  if (peerUserId && others.some((r) => r.userId === peerUserId)) {
    return 'seen'
  }
  if (others.length === 1) return 'seen'
  return 'seenMany'
}

export function readReceiptCountExcludingSelf(
  receipts: ReadReceiptDto[],
  currentUserId: string | null,
): number {
  return receipts.filter((r) => r.userId && r.userId !== currentUserId).length
}
