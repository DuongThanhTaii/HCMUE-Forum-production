import { useCallback, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { usePublishForumPostMutation } from '../api/forum.list.api'
import {
  useGetModerationPendingPostsQuery,
  useRejectModerationPostMutation,
  useApproveBulkModerationPostsMutation,
} from '../api/forum.moderation.api'

function readApiErrorMessage(err: unknown): string | null {
  if (!err || typeof err !== 'object') return null
  const e = err as { data?: unknown; error?: unknown }
  if (typeof e.error === 'string' && e.error.trim().length > 0) return e.error
  if (!e.data || typeof e.data !== 'object') return null
  const data = e.data as { error?: unknown; message?: unknown }
  if (typeof data.error === 'string' && data.error.trim().length > 0) return data.error
  if (typeof data.message === 'string' && data.message.trim().length > 0) return data.message
  return null
}

export function useModPostsPage() {
  const { t } = useTranslation()
  const { data = [], isLoading, isError } = useGetModerationPendingPostsQuery({ pageNumber: 1, pageSize: 30 })
  const [publishPost, { isLoading: isPublishing }] = usePublishForumPostMutation()
  const [rejectPost, { isLoading: isRejecting }] = useRejectModerationPostMutation()
  const [approveBulk, { isLoading: isApprovingBulk }] = useApproveBulkModerationPostsMutation()
  const [feedback, setFeedback] = useState<string | null>(null)
  const [selectedIds, setSelectedIds] = useState<string[]>([])

  const onApprove = useCallback(
    async (postId: string) => {
      setFeedback(null)
      try {
        await publishPost({ postId }).unwrap()
        setFeedback(t('forum.mod.feedback.approveSuccess'))
      } catch (err) {
        const apiMessage = readApiErrorMessage(err)
        setFeedback(apiMessage ?? t('forum.mod.feedback.approveFailed'))
      }
    },
    [publishPost, t],
  )

  const onReject = useCallback(
    async (postId: string, reason: string) => {
      setFeedback(null)
      try {
        await rejectPost({ postId, reason }).unwrap()
        setFeedback(t('forum.mod.feedback.rejectSuccess', 'Đã từ chối bài viết'))
        setSelectedIds((prev) => prev.filter((id) => id !== postId))
      } catch (err) {
        const apiMessage = readApiErrorMessage(err)
        setFeedback(apiMessage ?? t('forum.mod.feedback.rejectFailed', 'Không thể từ chối bài viết'))
      }
    },
    [rejectPost, t],
  )

  const onApproveBulk = useCallback(
    async () => {
      if (selectedIds.length === 0) return
      setFeedback(null)
      try {
        await approveBulk({ postIds: selectedIds }).unwrap()
        setFeedback(t('forum.mod.feedback.approveBulkSuccess', `Đã duyệt ${selectedIds.length} bài viết`))
        setSelectedIds([])
      } catch (err) {
        const apiMessage = readApiErrorMessage(err)
        setFeedback(apiMessage ?? t('forum.mod.feedback.approveBulkFailed', 'Không thể duyệt hàng loạt'))
      }
    },
    [approveBulk, selectedIds, t],
  )

  const toggleSelection = (id: string) => {
    setSelectedIds((prev) => (prev.includes(id) ? prev.filter((i) => i !== id) : [...prev, id]))
  }

  const toggleAll = () => {
    if (selectedIds.length === data.length) {
      setSelectedIds([])
    } else {
      setSelectedIds(data.map((p) => p.id))
    }
  }

  return {
    t,
    posts: data,
    isLoading,
    isError,
    isEmpty: !isLoading && !isError && data.length === 0,
    onApprove,
    onReject,
    onApproveBulk,
    isPublishing: isPublishing || isRejecting || isApprovingBulk,
    feedback,
    selectedIds,
    toggleSelection,
    toggleAll,
  }
}
