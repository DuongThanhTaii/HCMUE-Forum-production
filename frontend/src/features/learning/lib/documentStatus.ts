import type { TFunction } from 'i18next'

export type DocumentStatusTone = 'success' | 'danger' | 'warning' | 'muted'

const STATUS_LABEL_KEYS: Record<string, string> = {
  Draft: 'learning.documentStatus.draft',
  PendingApproval: 'learning.documentStatus.pendingApproval',
  Approved: 'learning.documentStatus.approved',
  Rejected: 'learning.documentStatus.rejected',
  Deleted: 'learning.documentStatus.deleted',
}

export function getDocumentStatusLabel(status: string, t: TFunction): string {
  const key = STATUS_LABEL_KEYS[status]
  return key ? t(key) : status
}

export function getDocumentStatusTone(status: string): DocumentStatusTone {
  if (status === 'Approved') return 'success'
  if (status === 'Rejected') return 'danger'
  if (status === 'Deleted') return 'muted'
  return 'warning'
}

export function documentStatusBadgeClass(tone: DocumentStatusTone): string {
  switch (tone) {
    case 'success':
      return 'text-emerald-700'
    case 'danger':
      return 'text-rose-700'
    case 'muted':
      return 'text-slate-500'
    default:
      return 'text-amber-700'
  }
}
