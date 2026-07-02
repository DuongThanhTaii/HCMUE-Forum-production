import { startTransition, useCallback, useEffect, useMemo, useRef, useState } from 'react'
import type { FormEvent } from 'react'
import { useTranslation } from 'react-i18next'
import { useParams } from 'react-router-dom'
import { useAuth } from '@features/auth/context/useAuth'
import { selectUserRole, selectUserId } from '@features/auth/model/auth.slice'
import { useAppSelector } from '@shared/hooks/useAppSelector'
import {
  useModerationHintMutation,
  useRelatedPostsMutation,
  useSummarizePostMutation,
  type ModerationHintResult,
  type RelatedPostsResult,
  type SummarizePostResult,
} from '@features/assistant/api/assistant.api'
import type { ForumCommentItem } from '../api/forum.list.api'
import {
  useAddCommentMutation,
  useAcceptAnswerMutation,
  useBookmarkPostMutation,
  useGetForumPostByIdQuery,
  useGetPostCommentsQuery,
  useReportPostMutation,
  usePinCommentMutation,
  useUnbookmarkPostMutation,
  useUploadForumAttachmentsMutation,
  useVoteCommentMutation,
  useVotePostMutation,
  useDeleteCommentMutation,
} from '../api/forum.list.api'

export type CommentThreadNode = ForumCommentItem & { children: CommentThreadNode[] }
export type CommentSortMode = 'top' | 'new'

function buildCommentThreads(flat: ForumCommentItem[]): CommentThreadNode[] {
  // Build map with children sorted by time (natural reply order)
  const byTime = [...flat].sort(
    (a, b) => new Date(a.createdAt).getTime() - new Date(b.createdAt).getTime(),
  )
  const map = new Map<string, CommentThreadNode>()
  for (const c of byTime) {
    map.set(c.id, { ...c, children: [] })
  }
  const roots: CommentThreadNode[] = []
  for (const c of byTime) {
    const node = map.get(c.id)!
    const pid = c.parentCommentId
    if (pid && map.has(pid)) {
      map.get(pid)!.children.push(node)
    } else {
      roots.push(node)
    }
  }
  return roots
}

function sortCommentThreads(nodes: CommentThreadNode[], mode: CommentSortMode): CommentThreadNode[] {
  const next = nodes.map((node) => ({
    ...node,
    children: sortCommentThreads(node.children, mode),
  }))

  next.sort((a, b) => {
    const pinDiff = Number(Boolean(b.isPinned)) - Number(Boolean(a.isPinned))
    if (pinDiff !== 0) return pinDiff

    if (mode === 'new') {
      return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
    }

    const voteDiff = (b.voteScore ?? 0) - (a.voteScore ?? 0)
    if (voteDiff !== 0) return voteDiff
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
  })

  return next
}

function formatDateTime(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }

  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
  })}`
}

export function useForumDetailPage() {
  const { t } = useTranslation()
  const { id: paramId, threadId } = useParams<{ id?: string; threadId?: string }>()
  const id = paramId || threadId || ''
  const { requireAuth } = useAuth()
  const roles = useAppSelector(selectUserRole)
  const userId = useAppSelector(selectUserId)
  const hasModeratorRole = useMemo(
    () => (roles ?? []).some((role) => role.trim().toLowerCase() === 'moderator'),
    [roles],
  )
  const [commentDraft, setCommentDraft] = useState('')
  const [hasTriedCommentSubmit, setHasTriedCommentSubmit] = useState(false)
  const [isBookmarked, setIsBookmarked] = useState(false)
  const [isUpvoted, setIsUpvoted] = useState(false)
  const [interactionErrorKey, setInteractionErrorKey] = useState<string | null>(null)
  const [interactionSuccessKey, setInteractionSuccessKey] = useState<string | null>(null)
  const [replyingToCommentId, setReplyingToCommentId] = useState<string | null>(null)
  const [replyDraft, setReplyDraft] = useState('')
  const [commentSortMode, setCommentSortMode] = useState<CommentSortMode>('top')
  const [hasManualSortSelection, setHasManualSortSelection] = useState(false)
  const [hasTriedReplySubmit, setHasTriedReplySubmit] = useState(false)
  const [commentAttachments, setCommentAttachments] = useState<File[]>([])
  const [replyAttachments, setReplyAttachments] = useState<File[]>([])
  const [hoveredCommentId, setHoveredCommentId] = useState<string | null>(null)

  const isLineHovered = useCallback((node: CommentThreadNode): boolean => {
    if (node.id === hoveredCommentId) return true
    if (node.children.some(c => c.id === hoveredCommentId)) return true
    return false
  }, [hoveredCommentId])
  const [isCommentCooldownActive, setIsCommentCooldownActive] = useState(false)
  const commentCooldownTimerRef = useRef<number | null>(null)
  const { data: post, isLoading, isError } = useGetForumPostByIdQuery(id, {
    skip: !id,
  })
  useEffect(() => {
    startTransition(() => {
      setIsBookmarked(post?.isBookmarked === true)
      setIsUpvoted(post?.isUpvoted === true)
    })
  }, [post?.isBookmarked, post?.isUpvoted])
  const { data: commentData = [], isLoading: isCommentsLoading } = useGetPostCommentsQuery(
    { postId: id, pageNumber: 1, pageSize: 30 },
    { skip: !id },
  )
  const [addComment, { isLoading: isSubmittingComment }] = useAddCommentMutation()
  const [votePost, { isLoading: isVoting }] = useVotePostMutation()
  const [voteComment, { isLoading: isVotingComment }] = useVoteCommentMutation()
  const [acceptAnswer, { isLoading: isAcceptingAnswer }] = useAcceptAnswerMutation()
  const [pinComment, { isLoading: isPinningComment }] = usePinCommentMutation()
  const [bookmarkPost, { isLoading: isBookmarking }] = useBookmarkPostMutation()
  const [unbookmarkPost, { isLoading: isUnbookmarking }] = useUnbookmarkPostMutation()
  const [reportPost, { isLoading: isReporting }] = useReportPostMutation()
  const [uploadAttachments, { isLoading: isUploadingAttachments }] = useUploadForumAttachmentsMutation()
  const [deleteComment, { isLoading: isDeletingComment }] = useDeleteCommentMutation()

  const fallbackTitle = t('forum.detail.fallbackTitle')
  const title = post?.title || `${fallbackTitle} #${id || t('common.noData')}`
  const category = post?.category || t('forum.categories')
  const authorLine = post?.authorName?.trim() || null
  const activityText = post?.activityAt ? formatDateTime(post.activityAt) : t('common.noData')
  const realPostContent =
    (post as { content?: string } | undefined)?.content?.trim() ||
    (post as { body?: string } | undefined)?.body?.trim() ||
    ''
  const postContent = realPostContent || t('forum.detail.fallbackContent')
  const voteScore = typeof post?.voteScore === 'number' ? post.voteScore : 0
  const queryView =
    typeof window !== 'undefined'
      ? new URLSearchParams(window.location.search).get('view')
      : null
  const hasThreadTag =
    Array.isArray(post?.tags) &&
    post.tags.some((tag) => {
      const normalized = String(tag ?? '').trim().toLowerCase()
      return normalized === 'thread' || normalized.startsWith('thread:')
    })
  const hasThreadChannel = Boolean((post as { threadChannelId?: string } | undefined)?.threadChannelId)
  const isThreadTopic = queryView === 'thread' || hasThreadTag || hasThreadChannel
  const effectiveSortMode: CommentSortMode =
    isThreadTopic && !hasManualSortSelection ? 'new' : commentSortMode

  const commentThreads = useMemo(
    () => sortCommentThreads(buildCommentThreads(commentData), effectiveSortMode),
    [commentData, effectiveSortMode],
  )

  const [reportOpen, setReportOpen] = useState(false)
  const [reportReason, setReportReason] = useState<number>(1)
  const [reportDescription, setReportDescription] = useState('')
  const [copilotError, setCopilotError] = useState<string | null>(null)
  const [copilotSummary, setCopilotSummary] = useState<SummarizePostResult | null>(null)
  const [copilotRelated, setCopilotRelated] = useState<RelatedPostsResult | null>(null)
  const [copilotModeration, setCopilotModeration] = useState<ModerationHintResult | null>(null)
  const [summarizePost, { isLoading: isSummarizing }] = useSummarizePostMutation()
  const [relatedPosts, { isLoading: isLoadingRelated }] = useRelatedPostsMutation()
  const [moderationHint, { isLoading: isLoadingModerationHint }] = useModerationHintMutation()

  function setFeedback(successKey: string | null, errorKey: string | null) {
    setInteractionSuccessKey(successKey)
    setInteractionErrorKey(errorKey)
  }

  function getMutationErrorKey(error: unknown, fallbackKey: string) {
    const status = (error as { status?: number } | undefined)?.status
    if (status === 401 || status === 403) {
      return 'forum.feedback.loginRequired'
    }
    return fallbackKey
  }

  function ensureAuthenticated() {
    return requireAuth(() => {
      setFeedback(null, 'forum.feedback.loginRequired')
    })
  }

  function getCopilotError(error: unknown, fallback = 'Unable to process AI request right now.') {
    const raw = error as { data?: { error?: string }; error?: string; status?: number } | undefined
    return raw?.data?.error || raw?.error || (raw?.status === 401 ? t('forum.feedback.loginRequired') : fallback)
  }

  const canSubmitComment = commentDraft.trim().length > 0 && !isSubmittingComment && Boolean(id)

  useEffect(() => {
    return () => {
      if (commentCooldownTimerRef.current) {
        window.clearTimeout(commentCooldownTimerRef.current)
      }
    }
  }, [])

  function startCommentCooldown(ms = 8000) {
    setIsCommentCooldownActive(true)
    if (commentCooldownTimerRef.current) {
      window.clearTimeout(commentCooldownTimerRef.current)
    }
    commentCooldownTimerRef.current = window.setTimeout(() => {
      setIsCommentCooldownActive(false)
      commentCooldownTimerRef.current = null
    }, ms)
  }

  function onCommentDraftChange(value: string) {
    setCommentDraft(value)
    if (interactionErrorKey || interactionSuccessKey) {
      setFeedback(null, null)
    }
  }

  async function onSubmitComment(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setFeedback(null, null)
    setHasTriedCommentSubmit(true)
    if (!canSubmitComment || isCommentCooldownActive) {
      if (commentDraft.trim().length === 0) {
        setFeedback(null, 'forum.feedback.commentRequired')
      }
      if (isCommentCooldownActive) {
        setFeedback(null, 'forum.feedback.commentCooldown')
      }
      return
    }
    if (!ensureAuthenticated()) {
      return
    }

    try {
      let finalContent = commentDraft.trim()
      if (commentAttachments.length > 0) {
        const urls = await uploadAttachments(commentAttachments).unwrap()
        if (urls.length > 0) {
          finalContent = `${finalContent}\n\nAttachments:\n${urls.map((u) => `- ${u}`).join('\n')}`
        }
      }
      await addComment({ postId: id, content: finalContent }).unwrap()
      setCommentDraft('')
      setCommentAttachments([])
      setHasTriedCommentSubmit(false)
      startCommentCooldown()
      setFeedback('forum.feedback.commentSuccess', null)
    } catch (error) {
      setFeedback(null, getMutationErrorKey(error, 'forum.feedback.commentFailed'))
    }
  }

  async function onUpvotePost() {
    setFeedback(null, null)
    if (!id || isVoting) {
      return
    }
    if (!ensureAuthenticated()) {
      return
    }
    try {
      const nextUpvoted = !isUpvoted
      setIsUpvoted(nextUpvoted)
      await votePost({ postId: id, voteType: 1 }).unwrap()
      setFeedback(nextUpvoted ? 'forum.feedback.voteSuccess' : 'forum.feedback.unvoteSuccess', null)
    } catch (error) {
      setIsUpvoted((prev) => !prev)
      setFeedback(null, getMutationErrorKey(error, 'forum.feedback.voteFailed'))
    }
  }

  async function onVoteComment(commentId: string, voteType: 1 | 2) {
    if (!id || !commentId || isVotingComment) return
    if (!ensureAuthenticated()) return
    try {
      await voteComment({ commentId, postId: id, voteType }).unwrap()
    } catch {
      /* optimistic update already rolled back by RTK Query */
    }
  }

  function onStartReply(commentId: string, authorName?: string, sourceContent?: string) {
    if (!ensureAuthenticated()) return
    const safeAuthor = (authorName || t('forum.commentSection.author')).trim()
    const content = (sourceContent || '').trim()
    const quoteBody = content
      .slice(0, 240)
      .split('\n')
      .map((line) => `> ${line}`)
      .join('\n')
    const quotePrefix = t('forum.commentSection.quotePrefix', { author: safeAuthor })
    setReplyingToCommentId(commentId)
    setReplyDraft(content ? `${quotePrefix}\n${quoteBody}\n\n` : '')
    setReplyAttachments([])
    setHasTriedReplySubmit(false)
  }

  function onCancelReply() {
    setReplyingToCommentId(null)
    setReplyDraft('')
    setReplyAttachments([])
    setHasTriedReplySubmit(false)
  }

  async function onSubmitReply(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    setHasTriedReplySubmit(true)
    if (isCommentCooldownActive) {
      setFeedback(null, 'forum.feedback.commentCooldown')
      return
    }
    if (!replyDraft.trim() || !replyingToCommentId || !id) return
    try {
      let finalContent = replyDraft.trim()
      if (replyAttachments.length > 0) {
        const urls = await uploadAttachments(replyAttachments).unwrap()
        if (urls.length > 0) {
          finalContent = `${finalContent}\n\nAttachments:\n${urls.map((u) => `- ${u}`).join('\n')}`
        }
      }
      await addComment({
        postId: id,
        content: finalContent,
        parentCommentId: replyingToCommentId,
      }).unwrap()
      setReplyingToCommentId(null)
      setReplyDraft('')
      setReplyAttachments([])
      setHasTriedReplySubmit(false)
      startCommentCooldown()
    } catch (error) {
      setFeedback(null, getMutationErrorKey(error, 'forum.feedback.commentFailed'))
    }
  }

  async function onAcceptAnswer(commentId: string) {
    if (!id || !commentId || !ensureAuthenticated()) return
    setFeedback(null, null)
    try {
      await acceptAnswer({ commentId, postId: id }).unwrap()
      setFeedback('forum.feedback.acceptAnswerSuccess', null)
    } catch (error) {
      setFeedback(null, getMutationErrorKey(error, 'forum.feedback.acceptAnswerFailed'))
    }
  }

  async function onPinComment(commentId: string) {
    if (!id || !commentId || !ensureAuthenticated()) return
    setFeedback(null, null)
    try {
      await pinComment({ commentId, postId: id }).unwrap()
      setFeedback('forum.feedback.pinCommentSuccess', null)
    } catch (error) {
      setFeedback(null, getMutationErrorKey(error, 'forum.feedback.pinCommentFailed'))
    }
  }

  function onChangeCommentSortMode(mode: CommentSortMode) {
    setHasManualSortSelection(true)
    setCommentSortMode(mode)
  }

  async function onToggleBookmark() {
    setFeedback(null, null)
    if (!id || isBookmarking || isUnbookmarking) {
      return
    }
    if (!ensureAuthenticated()) {
      return
    }

    try {
      if (isBookmarked) {
        setIsBookmarked(false)
        await unbookmarkPost({ postId: id }).unwrap()
        setFeedback('forum.feedback.unbookmarkSuccess', null)
        return
      }

      setIsBookmarked(true)
      await bookmarkPost({ postId: id }).unwrap()
      setFeedback('forum.feedback.bookmarkSuccess', null)
    } catch (error) {
      setIsBookmarked((prev) => !prev)
      setFeedback(
        null,
        getMutationErrorKey(
          error,
          isBookmarked ? 'forum.feedback.unbookmarkFailed' : 'forum.feedback.bookmarkFailed',
        ),
      )
    }
  }

  function onOpenReportModal() {
    setFeedback(null, null)
    if (!id) {
      return
    }
    if (!ensureAuthenticated()) {
      return
    }
    setReportReason(1)
    setReportDescription('')
    setReportOpen(true)
  }

  function onCloseReportModal() {
    setReportOpen(false)
  }

  async function onSubmitReportModal() {
    setFeedback(null, null)
    if (!id || isReporting) {
      return
    }
    try {
      await reportPost({
        postId: id,
        reason: reportReason,
        description: reportDescription.trim() || undefined,
      }).unwrap()
      setReportOpen(false)
      setReportDescription('')
      setFeedback('forum.feedback.reportSuccess', null)
    } catch (error) {
      setFeedback(null, getMutationErrorKey(error, 'forum.feedback.reportFailed'))
    }
  }

  async function onSharePost() {
    setFeedback(null, null)
    if (!id) {
      return
    }

    const shareUrl = window.location.href
    try {
      if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(shareUrl)
        setFeedback('forum.feedback.shareSuccess', null)
        return
      }

      setFeedback(null, 'forum.feedback.shareUnavailable')
    } catch {
      setFeedback(null, 'forum.feedback.shareUnavailable')
    }
  }

  async function onShareFacebook() {
    setFeedback(null, null)
    if (!id) return
    const directUrl = `${window.location.origin}/forum/${id}`
    const excerpt = postContent.slice(0, 280)
    const quote = `${title}\n\n${excerpt}\n\n${directUrl}`
    try {
      if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(quote)
        setFeedback('forum.feedback.facebookShareReady', null)
      }
    } catch {
      // continue opening share dialog even if clipboard access is unavailable
    }
    const fbShareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(directUrl)}&quote=${encodeURIComponent(quote)}`
    window.open(fbShareUrl, '_blank', 'noopener,noreferrer,width=700,height=560')
  }

  async function onSummarizePost() {
    if (!id || !ensureAuthenticated()) return
    setCopilotError(null)
    try {
      const data = await summarizePost({ postId: id, length: 'medium' }).unwrap()
      setCopilotSummary(data)
    } catch (error) {
      setCopilotError(getCopilotError(error, 'Failed to summarize this post.'))
    }
  }

  async function onLoadRelatedPosts() {
    if (!id || !ensureAuthenticated()) return
    setCopilotError(null)
    try {
      const data = await relatedPosts({ postId: id, limit: 5 }).unwrap()
      setCopilotRelated(data)
    } catch (error) {
      setCopilotError(getCopilotError(error, 'Failed to load related posts.'))
    }
  }

  async function onGenerateModerationHint() {
    if (!id || !hasModeratorRole || !ensureAuthenticated()) return
    setCopilotError(null)
    try {
      const data = await moderationHint({ postId: id }).unwrap()
      setCopilotModeration(data)
    } catch (error) {
      setCopilotError(getCopilotError(error, 'Failed to generate moderation hint.'))
    }
  }

  return {
    t,
    post,
    title,
    category,
    authorLine,
    activityText,
    postContent,
    voteScore,
    commentThreads,
    commentDraft,
    setCommentDraft,
    commentAttachments,
    setCommentAttachments,
    onCommentDraftChange,
    hasTriedCommentSubmit,
    canSubmitComment,
    onSubmitComment,
    onUpvotePost,
    onVoteComment,
    onAcceptAnswer,
    onPinComment,
    commentSortMode: effectiveSortMode,
    setCommentSortMode: onChangeCommentSortMode,
    replyingToCommentId,
    replyDraft,
    setReplyDraft,
    replyAttachments,
    setReplyAttachments,
    hasTriedReplySubmit,
    onStartReply,
    onCancelReply,
    onSubmitReply,
    onToggleBookmark,
    onOpenReportModal,
    onCloseReportModal,
    onSubmitReportModal,
    reportOpen,
    reportReason,
    setReportReason,
    reportDescription,
    setReportDescription,
    userId,
    onSharePost,
    onShareFacebook,
    onSummarizePost,
    onLoadRelatedPosts,
    onGenerateModerationHint,
    interactionErrorKey,
    interactionSuccessKey,
    copilotError,
    copilotSummary,
    copilotRelated,
    copilotModeration,
    hasModeratorRole,
    isThreadTopic,
    isQuestionPost: post?.type === 2,
    canAcceptAnswer: Boolean(post?.authorId && userId && post.authorId === userId),
    canPinComment: hasModeratorRole || Boolean(post?.authorId && userId && post.authorId === userId),
    isBookmarked,
    isUpvoted,
    isCommentsLoading,
    isSubmittingComment,
    isUploadingAttachments,
    isVotingComment,
    isAcceptingAnswer,
    isPinningComment,
    isVoting,
    onDeleteComment: async (commentId: string) => {
      if (!window.confirm(t('forum.commentSection.confirmDelete', 'Bạn có chắc chắn muốn xóa bình luận này?'))) return
      try {
        await deleteComment({ commentId, postId: id }).unwrap()
      } catch (error) {
        console.error('Failed to delete comment:', error)
      }
    },
    isBookmarking,
    isUnbookmarking,
    isReporting,
    isDeletingComment,
    isSummarizing,
    isLoadingRelated,
    isLoadingModerationHint,
    isLoading,
    isError,
    hoveredCommentId,
    setHoveredCommentId,
    isLineHovered,
  }
}
