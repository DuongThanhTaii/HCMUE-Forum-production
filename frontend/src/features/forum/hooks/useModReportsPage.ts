import { useMemo, useState } from 'react'
import { useTranslation } from 'react-i18next'
import {
  useGetModerationReportsQuery,
  useResolveModerationReportMutation,
} from '../api/forum.moderation.api'
import type { ModerationReportItem, ModerationReportStatusFilter } from '../types/forum-moderation'

const PAGE_SIZE = 20

/** Map numeric reason code → i18n key inside `forum.mod.reason` */
function reasonKey(reason: number): string {
  const known = [1, 2, 3, 4, 5, 6]
  return known.includes(reason) ? String(reason) : 'unknown'
}

/** Build a navigation link for the reported item. */
function buildItemLink(report: ModerationReportItem): string | undefined {
  if (report.reportedItemType === 1) {
    // Post
    return `/forum/${report.reportedItemId}`
  }
  if (report.reportedItemType === 2) {
    // Comment — link to post anchor (best effort without postId in response)
    return `/forum?commentId=${report.reportedItemId}`
  }
  return undefined
}

/** Returns true when a RTK Query rejection payload looks like HTTP 403. */
function isForbidden(error: unknown): boolean {
  if (!error || typeof error !== 'object') return false
  const e = error as { status?: number | string }
  return e.status === 403 || e.status === '403'
}

export function useModReportsPage() {
  const { t } = useTranslation()
  const [status, setStatus] = useState<ModerationReportStatusFilter>('pending')
  const [page, setPage] = useState(1)

  const { data, isLoading, isError, isFetching } = useGetModerationReportsQuery({
    status,
    pageNumber: page,
    pageSize: PAGE_SIZE,
  })

  const [resolveReport, { isLoading: isResolving }] = useResolveModerationReportMutation()
  const [feedback, setFeedback] = useState<{ type: 'success' | 'error' | 'forbidden'; message: string } | null>(null)

  const reports = data?.reports ?? []
  const totalCount = data?.totalCount ?? 0
  const totalPages = data?.totalPages ?? 1
  const hasPreviousPage = data?.hasPreviousPage ?? false
  const hasNextPage = data?.hasNextPage ?? false

  const tabs = useMemo(
    () => [
      { id: 'pending' as const, label: t('forum.mod.status.pending') },
      { id: 'resolved_keep' as const, label: t('forum.mod.status.resolvedKeep') },
      { id: 'resolved_remove' as const, label: t('forum.mod.status.resolvedRemove') },
    ],
    [t],
  )

  function getReasonLabel(reason: number): string {
    const key = reasonKey(reason)
    if (key === 'unknown') {
      return t('forum.mod.reason.unknown', { value: reason })
    }
    return t(`forum.mod.reason.${key}`)
  }

  function getItemLink(report: ModerationReportItem): string | undefined {
    return buildItemLink(report)
  }

  function onPageChange(newPage: number) {
    setPage(newPage)
    setFeedback(null)
  }

  function onStatusChange(newStatus: ModerationReportStatusFilter) {
    setStatus(newStatus)
    setPage(1)
    setFeedback(null)
  }

  async function onResolve(reportId: number, action: 'keep' | 'remove') {
    setFeedback(null)
    try {
      await resolveReport({ reportId, action }).unwrap()
      setFeedback({ type: 'success', message: t('forum.mod.feedback.resolveSuccess') })
    } catch (err) {
      if (isForbidden(err)) {
        setFeedback({ type: 'forbidden', message: t('forum.mod.feedback.resolveForbidden') })
      } else {
        setFeedback({ type: 'error', message: t('forum.mod.feedback.resolveFailed') })
      }
    }
  }

  return {
    t,
    status,
    setStatus: onStatusChange,
    tabs,
    reports,
    totalCount,
    totalPages,
    page,
    hasPreviousPage,
    hasNextPage,
    isLoading,
    isError,
    isFetching,
    isResolving,
    feedback,
    onResolve,
    onPageChange,
    getReasonLabel,
    getItemLink,
  }
}
