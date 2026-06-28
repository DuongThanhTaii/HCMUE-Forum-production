export type ModerationReportStatusFilter = 'pending' | 'resolved_keep' | 'resolved_remove'

export type ModerationReportItem = {
  id: number
  reportedItemId: string
  reportedItemType: number
  reporterId: string
  reason: number
  description?: string
  status: number
  createdAt: string
  reviewedAt?: string
  reviewedBy?: string
  resolutionDecision?: 'keep' | 'remove'
  titlePreview?: string
  contentPreview?: string
  isTargetDeleted: boolean
}

export type ModerationReportListResult = {
  reports: ModerationReportItem[]
  totalCount: number
  pageNumber: number
  pageSize: number
  totalPages: number
  hasPreviousPage: boolean
  hasNextPage: boolean
}

export type PendingModerationPost = {
  id: string
  title: string
  authorName?: string
  categoryName?: string
  createdAt: string
  commentCount: number
}
