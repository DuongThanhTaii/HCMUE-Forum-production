import { useCallback, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { usePublishForumPostMutation } from '../api/forum.list.api'
import { useGetModerationPendingPostsQuery } from '../api/forum.moderation.api'

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
  const [feedback, setFeedback] = useState<string | null>(null)

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

  return {
    t,
    posts: data,
    isLoading,
    isError,
    isEmpty: !isLoading && !isError && data.length === 0,
    onApprove,
    isPublishing,
    feedback,
  }
}
