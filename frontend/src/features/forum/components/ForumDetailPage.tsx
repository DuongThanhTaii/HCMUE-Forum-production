import { ArrowBigDown, ArrowBigUp, CornerDownRight } from 'lucide-react'
import type { ReactNode } from 'react'
import { useState } from 'react'
import type { FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { featureFlags } from '@shared/config/featureFlags'
import type { CommentSortMode, CommentThreadNode } from '../hooks/useForumDetailPage'
import { useForumDetailPage } from '../hooks/useForumDetailPage'
import { parseForumRichContent } from '../lib/parseForumRichContent'

function formatCommentTime(value: string) {
  const date = new Date(value)
  if (Number.isNaN(date.getTime())) {
    return ''
  }
  return `${date.toLocaleDateString()} ${date.toLocaleTimeString([], {
    hour: '2-digit',
    minute: '2-digit',
  })}`
}

type CommentActions = {
  replyingToId: string | null
  replyDraft: string
  hasTriedReplySubmit: boolean
  collapsedIds: Set<string>
  onReplyDraftChange: (v: string) => void
  onStartReply: (id: string, authorName?: string, sourceContent?: string) => void
  onCancelReply: () => void
  onToggleCollapse: (id: string) => void
  onSubmitReply: (e: FormEvent<HTMLFormElement>) => Promise<void>
  replyAttachments: File[]
  onReplyAttachmentsChange: (files: File[]) => void
  onVoteComment: (commentId: string, voteType: 1 | 2) => Promise<void>
  onAcceptAnswer: (commentId: string) => Promise<void>
  onPinComment: (commentId: string) => Promise<void>
  isVotingComment: boolean
  canAcceptAnswer: boolean
  isQuestionPost: boolean
  isAcceptingAnswer: boolean
  canPinComment: boolean
  isPinningComment: boolean
  t: (key: string) => string
}

function renderWithMentions(content: string): ReactNode {
  const parts = content.split(/(@[a-zA-Z0-9._-]+)/g)
  return parts.map((part, index) => {
    if (/^@[a-zA-Z0-9._-]+$/.test(part)) {
      return (
        <span key={`${part}-${index}`} className="font-medium text-primary">
          {part}
        </span>
      )
    }
    return <span key={`${index}-${part.slice(0, 10)}`}>{part}</span>
  })
}

function CommentBranch({
  node,
  depth,
  actions,
}: {
  node: CommentThreadNode
  depth: number
  actions: CommentActions
}) {
  const time = formatCommentTime(node.createdAt)
  const isReplying = actions.replyingToId === node.id
  const isUpvoted = node.currentUserVote === 1
  const isDownvoted = node.currentUserVote === 2
  const isCollapsed = actions.collapsedIds.has(node.id)
  const parsed = parseForumRichContent(node.content)
  return (
    <div className={depth > 0 ? 'mt-3 border-l-2 border-slate-200 pl-4' : ''}>
      <div className="flex flex-wrap items-center justify-between gap-2 text-[13px]">
        <div className="flex items-center gap-2">
          {node.children.length > 0 ? (
            <button
              type="button"
              onClick={() => actions.onToggleCollapse(node.id)}
              className="rounded border border-slate-200 px-1.5 py-0.5 text-[11px] text-slate-600 hover:bg-slate-100"
            >
              {isCollapsed ? actions.t('forum.commentSection.expandThread') : actions.t('forum.commentSection.collapseThread')}
            </button>
          ) : null}
          <span className="font-medium text-slate-700">{node.authorName}</span>
        </div>
        <span className="text-slate-400 tabular-nums">{time}</span>
      </div>
      {parsed.body ? (
        <p className="mt-1.5 whitespace-pre-line text-[14px] leading-6 text-slate-700">
          {renderWithMentions(parsed.body)}
        </p>
      ) : null}
      {parsed.imageUrls.length > 0 ? (
        <div className="mt-2 grid grid-cols-1 gap-2 sm:grid-cols-2">
          {parsed.imageUrls.map((url) => (
            <a key={url} href={url} target="_blank" rel="noopener noreferrer" className="block">
              <img
                src={url}
                alt={actions.t('forum.detail.attachmentImageAlt')}
                loading="lazy"
                className="max-h-56 w-full rounded-md border border-slate-200 object-contain"
              />
            </a>
          ))}
        </div>
      ) : null}
      {parsed.fileUrls.length > 0 ? (
        <div className="mt-2 space-y-1">
          {parsed.fileUrls.map((url) => (
            <a
              key={url}
              href={url}
              target="_blank"
              rel="noopener noreferrer"
              className="block break-all text-[13px] text-primary hover:underline"
            >
              {url}
            </a>
          ))}
        </div>
      ) : null}

      <div className="mt-1.5 flex items-center gap-1">
        <button
          type="button"
          onClick={() => void actions.onVoteComment(node.id, 1)}
          disabled={actions.isVotingComment}
          aria-pressed={isUpvoted}
          className={`inline-flex items-center gap-0.5 rounded px-1.5 py-0.5 text-[12px] font-medium disabled:opacity-50 ${
            isUpvoted
              ? 'bg-emerald-50 text-emerald-700'
              : 'text-slate-500 hover:bg-slate-100 hover:text-emerald-600'
          }`}
          title="Upvote"
        >
          <ArrowBigUp className="h-3.5 w-3.5" strokeWidth={2} aria-hidden />
          <span className="tabular-nums">{node.voteScore}</span>
        </button>
        <button
          type="button"
          onClick={() => void actions.onVoteComment(node.id, 2)}
          disabled={actions.isVotingComment}
          aria-pressed={isDownvoted}
          className={`inline-flex items-center rounded px-1.5 py-0.5 text-[12px] font-medium disabled:opacity-50 ${
            isDownvoted ? 'bg-rose-50 text-rose-700' : 'text-slate-500 hover:bg-slate-100 hover:text-rose-500'
          }`}
          title="Downvote"
        >
          <ArrowBigDown className="h-3.5 w-3.5" strokeWidth={2} aria-hidden />
        </button>
        <button
          type="button"
          onClick={() =>
            isReplying
              ? actions.onCancelReply()
              : actions.onStartReply(node.id, node.authorName, parsed.body || node.content)
          }
          className="inline-flex items-center gap-1 rounded px-1.5 py-0.5 text-[12px] font-medium text-slate-500 hover:bg-slate-100 hover:text-primary"
        >
          <CornerDownRight className="h-3 w-3" strokeWidth={2} aria-hidden />
          {isReplying ? actions.t('forum.commentSection.cancelReply') : actions.t('forum.commentSection.reply')}
        </button>
        {actions.isQuestionPost && actions.canAcceptAnswer && !node.isAcceptedAnswer ? (
          <button
            type="button"
            onClick={() => void actions.onAcceptAnswer(node.id)}
            disabled={actions.isAcceptingAnswer}
            className="inline-flex items-center rounded px-1.5 py-0.5 text-[12px] font-medium text-emerald-700 hover:bg-emerald-50 disabled:opacity-60"
          >
            {actions.t('forum.commentSection.acceptAnswer')}
          </button>
        ) : null}
        {actions.canPinComment ? (
          <button
            type="button"
            onClick={() => void actions.onPinComment(node.id)}
            disabled={actions.isPinningComment}
            className={`inline-flex items-center rounded px-1.5 py-0.5 text-[12px] font-medium disabled:opacity-60 ${
              node.isPinned ? 'text-amber-800 hover:bg-amber-50' : 'text-slate-600 hover:bg-slate-100'
            }`}
          >
            {node.isPinned ? actions.t('forum.commentSection.unpinComment') : actions.t('forum.commentSection.pinComment')}
          </button>
        ) : null}
        {node.isAcceptedAnswer ? (
          <span className="inline-flex items-center rounded bg-emerald-100 px-1.5 py-0.5 text-[11px] font-semibold text-emerald-800">
            {actions.t('forum.commentSection.acceptedAnswer')}
          </span>
        ) : null}
        {node.isPinned ? (
          <span className="inline-flex items-center rounded bg-amber-100 px-1.5 py-0.5 text-[11px] font-semibold text-amber-800">
            {actions.t('forum.commentSection.pinnedComment')}
          </span>
        ) : null}
      </div>

      {isReplying ? (
        <form
          onSubmit={(e) => void actions.onSubmitReply(e)}
          className="mt-2 rounded-md border border-slate-200 bg-slate-50 p-2"
        >
          <textarea
            value={actions.replyDraft}
            onChange={(e) => actions.onReplyDraftChange(e.target.value)}
            rows={2}
            autoFocus
            className="w-full rounded border border-slate-300 px-2 py-1.5 text-[13px] focus:border-primary focus:outline-none"
            placeholder={actions.t('forum.detail.commentPlaceholder')}
          />
          <input
            type="file"
            multiple
            onChange={(e) => actions.onReplyAttachmentsChange(Array.from(e.target.files ?? []))}
            className="mt-2 w-full rounded border border-slate-300 px-2 py-1.5 text-[13px] file:mr-2 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs"
          />
          {actions.replyAttachments.length > 0 ? (
            <p className="mt-1 text-[11px] text-slate-500">{actions.replyAttachments.length} file(s) selected</p>
          ) : null}
          {actions.hasTriedReplySubmit && !actions.replyDraft.trim() ? (
            <p className="mt-0.5 text-[11px] text-rose-600">{actions.t('forum.feedback.commentRequired')}</p>
          ) : null}
          <div className="mt-1.5 flex justify-end gap-2">
            <button
              type="button"
              onClick={actions.onCancelReply}
              className="rounded px-2.5 py-1 text-[12px] font-medium text-slate-600 hover:bg-slate-200"
            >
              {actions.t('forum.commentSection.cancelReply')}
            </button>
            <button
              type="submit"
              disabled={!actions.replyDraft.trim()}
              className="rounded bg-primary px-2.5 py-1 text-[12px] font-medium text-white hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
            >
              {actions.t('forum.commentSection.reply')}
            </button>
          </div>
        </form>
      ) : null}

      {node.children.length > 0 && !isCollapsed ? (
        <div className="mt-2 space-y-1">
          {node.children.map((ch) => (
            <CommentBranch key={ch.id} node={ch} depth={depth + 1} actions={actions} />
          ))}
        </div>
      ) : null}
    </div>
  )
}

export function ForumDetailPage() {
  const [collapsedCommentIds, setCollapsedCommentIds] = useState<Set<string>>(new Set())
  const {
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
    commentSortMode,
    setCommentSortMode,
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
    isQuestionPost,
    canAcceptAnswer,
    canPinComment,
    isBookmarked,
    isCommentsLoading,
    isSubmittingComment,
    isUploadingAttachments,
    isVotingComment,
    isAcceptingAnswer,
    isPinningComment,
    isVoting,
    isBookmarking,
    isUnbookmarking,
    isReporting,
    isSummarizing,
    isLoadingRelated,
    isLoadingModerationHint,
    isLoading,
    isError,
  } = useForumDetailPage()
  const parsedPost = parseForumRichContent(postContent)

  function onToggleCollapse(commentId: string) {
    setCollapsedCommentIds((prev) => {
      const next = new Set(prev)
      if (next.has(commentId)) next.delete(commentId)
      else next.add(commentId)
      return next
    })
  }

  function onSortModeChange(mode: CommentSortMode) {
    setCommentSortMode(mode)
    setCollapsedCommentIds(new Set())
  }

  const commentActions: CommentActions = {
    replyingToId: replyingToCommentId,
    replyDraft,
    hasTriedReplySubmit,
    collapsedIds: collapsedCommentIds,
    onReplyDraftChange: setReplyDraft,
    onStartReply: (id, authorName, sourceContent) => onStartReply(id, authorName, sourceContent),
    onCancelReply,
    onToggleCollapse,
    onSubmitReply,
    replyAttachments,
    onReplyAttachmentsChange: setReplyAttachments,
    onVoteComment,
    onAcceptAnswer,
    onPinComment,
    isVotingComment,
    canAcceptAnswer,
    isQuestionPost,
    isAcceptingAnswer,
    canPinComment,
    isPinningComment,
    t,
  }

  if (isLoading) {
    return (
      <div className="forum-compact-card px-4 py-3 text-[14px] text-slate-600">
        {t('common.loading')}
      </div>
    )
  }

  if (isError) {
    return (
      <div className="forum-compact-card border-rose-200 bg-rose-50 px-4 py-3 text-[14px] text-jasper">
        {t('forum.error.loadFailed')}
      </div>
    )
  }

  return (
    <div className="space-y-2.5">
      <nav className="forum-compact-card px-4 py-2 text-[13px] text-slate-600">
        <Link to="/forum" className="forum-topic-link hover:text-primary">
          {t('forum.title')}
        </Link>
        <span className="mx-1.5 text-slate-400">/</span>
        <span className="font-medium text-slate-700">{title}</span>
      </nav>

      <section className="forum-compact-card px-4 py-3">
        <h1 className="text-[18px] font-semibold text-slate-900">{title}</h1>
        {isThreadTopic ? (
          <div className="mt-2 rounded-md border border-blue-200 bg-blue-50 px-3 py-2 text-[13px] text-blue-800">
            <span className="font-semibold">{t('forum.threadMode.badge')}:</span>{' '}
            {t('forum.threadMode.description')}
            <ul className="mt-2 list-disc space-y-0.5 pl-5 text-[12px] text-blue-900">
              <li>{t('forum.threadMode.point1')}</li>
              <li>{t('forum.threadMode.point2')}</li>
              <li>{t('forum.threadMode.point3')}</li>
            </ul>
          </div>
        ) : null}
        <div className="mt-1.5 flex flex-wrap items-center gap-x-3 gap-y-1 text-[13px] text-slate-500">
          {authorLine ? (
            <>
              <span className="font-medium text-slate-700">{authorLine}</span>
              <span className="text-slate-300">|</span>
            </>
          ) : null}
          <span className="font-medium text-slate-600">{category}</span>
          <span className="text-slate-300">|</span>
          <span>
            {t('forum.replies')}: {post?.replyCount ?? 0}
          </span>
          <span className="text-slate-300">|</span>
          <span>{t('forum.meta.lastUpdated')}: {activityText}</span>
        </div>
        {post?.tags?.length ? (
          <div className="mt-2 flex flex-wrap gap-1.5">
            {post.tags.map((tag) => (
              <span key={tag} className="forum-tag-chip px-1.5 py-0.5 text-[12px] font-medium">
                {tag}
              </span>
            ))}
          </div>
        ) : null}
        <div className="mt-3 border-t border-slate-200 pt-3">
          {parsedPost.body ? (
            <p className="whitespace-pre-line text-[14px] leading-6 text-slate-700">{parsedPost.body}</p>
          ) : null}
          {parsedPost.imageUrls.length > 0 ? (
            <div className="mt-3">
              <p className="mb-2 text-[12px] font-medium uppercase tracking-wide text-slate-500">
                {t('forum.detail.attachmentsLabel')}
              </p>
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                {parsedPost.imageUrls.map((url) => (
                  <a key={url} href={url} target="_blank" rel="noopener noreferrer" className="block">
                    <img
                      src={url}
                      alt={t('forum.detail.attachmentImageAlt')}
                      loading="lazy"
                      className="max-h-80 w-full rounded-md border border-slate-200 object-contain"
                    />
                  </a>
                ))}
              </div>
            </div>
          ) : null}
          {parsedPost.fileUrls.length > 0 ? (
            <div className="mt-3">
              <p className="mb-2 text-[12px] font-medium uppercase tracking-wide text-slate-500">
                {t('forum.detail.attachmentsLabel')}
              </p>
              <div className="space-y-1.5">
                {parsedPost.fileUrls.map((url) => (
                  <a
                    key={url}
                    href={url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className="block break-all text-[13px] text-primary hover:underline"
                  >
                    {url}
                  </a>
                ))}
              </div>
            </div>
          ) : null}
        </div>
      </section>

      <section className="forum-compact-card border-t border-slate-200 px-4 py-2.5">
        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => void onUpvotePost()}
            disabled={isVoting}
            className="inline-flex items-center gap-2 rounded-md border border-primary px-3 py-1.5 text-[13px] font-medium text-primary hover:bg-slate-50 disabled:cursor-not-allowed disabled:opacity-60"
          >
            <ArrowBigUp className="h-4 w-4 shrink-0" strokeWidth={2} aria-hidden />
            <span className="tabular-nums font-semibold">{voteScore}</span>
            <span className="text-slate-600">{t('forum.actions.upvote')}</span>
          </button>
          <button
            type="button"
            onClick={onOpenReportModal}
            disabled={isReporting}
            className="rounded-md border border-slate-200 px-3 py-1.5 text-[13px] font-medium text-slate-600 hover:border-primary hover:text-primary disabled:cursor-not-allowed disabled:opacity-60"
          >
            {t('forum.actions.report')}
          </button>
          <button
            type="button"
            onClick={() => void onToggleBookmark()}
            disabled={isBookmarking || isUnbookmarking}
            className="rounded-md border border-slate-200 px-3 py-1.5 text-[13px] font-medium text-slate-600 hover:border-primary hover:text-primary disabled:cursor-not-allowed disabled:opacity-60"
          >
            {isBookmarked ? t('forum.actions.unbookmark') : t('forum.actions.bookmark')}
          </button>
          <button
            type="button"
            onClick={() => void onSharePost()}
            className="rounded-md border border-slate-200 px-3 py-1.5 text-[13px] font-medium text-slate-600 hover:border-primary hover:text-primary"
          >
            {t('forum.actions.share')}
          </button>
          <button
            type="button"
            onClick={onShareFacebook}
            className="rounded-md border border-sky-200 px-3 py-1.5 text-[13px] font-medium text-sky-700 hover:border-sky-400 hover:text-sky-800"
          >
            Facebook
          </button>
          {featureFlags.copilotActionsEnabled ? (
            <>
              <button
                type="button"
                onClick={() => void onSummarizePost()}
                disabled={isSummarizing}
                className="cursor-pointer rounded-md border border-violet-200 px-3 py-1.5 text-[13px] font-medium text-violet-700 hover:border-violet-400 hover:text-violet-800 disabled:opacity-60"
              >
                {isSummarizing ? 'Summarizing...' : 'AI Summary'}
              </button>
              <button
                type="button"
                onClick={() => void onLoadRelatedPosts()}
                disabled={isLoadingRelated}
                className="cursor-pointer rounded-md border border-indigo-200 px-3 py-1.5 text-[13px] font-medium text-indigo-700 hover:border-indigo-400 hover:text-indigo-800 disabled:opacity-60"
              >
                {isLoadingRelated ? 'Loading...' : 'Related Posts'}
              </button>
              {hasModeratorRole ? (
                <button
                  type="button"
                  onClick={() => void onGenerateModerationHint()}
                  disabled={isLoadingModerationHint}
                  className="cursor-pointer rounded-md border border-amber-200 px-3 py-1.5 text-[13px] font-medium text-amber-700 hover:border-amber-400 hover:text-amber-800 disabled:opacity-60"
                >
                  {isLoadingModerationHint ? 'Analyzing...' : 'Moderation Hint'}
                </button>
              ) : null}
            </>
          ) : null}
        </div>
        {interactionSuccessKey ? (
          <div className="mt-2 rounded-md border border-emerald-200 bg-emerald-50 px-3 py-2 text-[13px] text-emerald-700">
            {t(interactionSuccessKey)}
          </div>
        ) : null}
        {interactionErrorKey ? (
          <div className="mt-2 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-[13px] text-rose-700">
            {t(interactionErrorKey)}
          </div>
        ) : null}
        {featureFlags.copilotActionsEnabled && copilotError ? (
          <div className="mt-2 rounded-md border border-rose-200 bg-rose-50 px-3 py-2 text-[13px] text-rose-700">
            {copilotError}
          </div>
        ) : null}
        {featureFlags.copilotActionsEnabled && copilotSummary ? (
          <div className="mt-2 rounded-md border border-violet-200 bg-violet-50 px-3 py-2">
            <p className="text-[12px] font-semibold uppercase tracking-wide text-violet-700">AI Summary</p>
            <p className="mt-1 whitespace-pre-line text-[13px] text-slate-700">{copilotSummary.summary}</p>
            {copilotSummary.keyPoints.length > 0 ? (
              <ul className="mt-2 list-disc space-y-1 pl-5 text-[12px] text-slate-700">
                {copilotSummary.keyPoints.map((point) => (
                  <li key={point}>{point}</li>
                ))}
              </ul>
            ) : null}
          </div>
        ) : null}
        {featureFlags.copilotActionsEnabled && copilotRelated?.items?.length ? (
          <div className="mt-2 rounded-md border border-indigo-200 bg-indigo-50 px-3 py-2">
            <p className="text-[12px] font-semibold uppercase tracking-wide text-indigo-700">Related Posts</p>
            <div className="mt-1.5 space-y-1.5">
              {copilotRelated.items.map((item) => (
                <Link
                  key={item.id}
                  to={`/forum/${item.id}`}
                  className="block rounded border border-indigo-100 bg-white px-2 py-1.5 text-[12px] text-slate-700 hover:border-indigo-300 hover:text-indigo-700"
                >
                  <p className="font-medium">{item.title}</p>
                  <p className="mt-0.5 text-[11px] text-slate-500">{item.reason}</p>
                  <p className="mt-0.5 text-[11px] text-indigo-700">
                    Citation:
                    {' '}
                    <span className="font-medium">{item.citationUrl}</span>
                    {' '}
                    | Rank:
                    {' '}
                    {item.searchRank.toFixed(3)}
                  </p>
                </Link>
              ))}
            </div>
          </div>
        ) : null}
        {featureFlags.copilotActionsEnabled && hasModeratorRole && copilotModeration ? (
          <div className="mt-2 rounded-md border border-amber-200 bg-amber-50 px-3 py-2">
            <p className="text-[12px] font-semibold uppercase tracking-wide text-amber-700">Moderation Hint</p>
            <p className="mt-1 text-[13px] text-slate-700">
              Recommendation: <span className="font-semibold">{copilotModeration.recommendation}</span>
            </p>
            <p className="text-[12px] text-slate-600">{copilotModeration.reason}</p>
          </div>
        ) : null}
      </section>

      {reportOpen ? (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/40 p-4"
          role="presentation"
          tabIndex={-1}
          onClick={onCloseReportModal}
          onKeyDown={(e) => {
            if (e.key === 'Escape') {
              onCloseReportModal()
            }
          }}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="report-dialog-title"
            className="w-full max-w-md rounded-lg border border-slate-200 bg-white p-4 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="report-dialog-title" className="text-base font-semibold text-slate-900">
              {t('forum.reportModal.title')}
            </h2>
            <p className="mt-1 text-[13px] text-slate-600">{t('forum.reportModal.subtitle')}</p>
            <label htmlFor="report-reason" className="mt-4 block text-[12px] font-medium text-slate-700">
              {t('forum.reportModal.reasonLabel')}
            </label>
            <select
              id="report-reason"
              value={reportReason}
              onChange={(e) => setReportReason(Number(e.target.value))}
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-[14px] text-slate-900 focus:border-primary focus:outline-none"
            >
              <option value={1}>{t('forum.reportModal.reasons.spam')}</option>
              <option value={2}>{t('forum.reportModal.reasons.harassment')}</option>
              <option value={3}>{t('forum.reportModal.reasons.inappropriate')}</option>
              <option value={4}>{t('forum.reportModal.reasons.misinformation')}</option>
              <option value={5}>{t('forum.reportModal.reasons.offTopic')}</option>
              <option value={6}>{t('forum.reportModal.reasons.copyright')}</option>
              <option value={99}>{t('forum.reportModal.reasons.other')}</option>
            </select>
            <label htmlFor="report-details" className="mt-3 block text-[12px] font-medium text-slate-700">
              {t('forum.reportModal.detailsLabel')}
            </label>
            <textarea
              id="report-details"
              value={reportDescription}
              onChange={(e) => setReportDescription(e.target.value)}
              rows={4}
              className="mt-1 w-full rounded-md border border-slate-300 px-3 py-2 text-[14px] focus:border-primary focus:outline-none"
              placeholder={t('forum.reportModal.detailsPlaceholder')}
            />
            <div className="mt-4 flex justify-end gap-2">
              <button
                type="button"
                onClick={onCloseReportModal}
                className="rounded-md border border-slate-200 px-3 py-1.5 text-[13px] font-medium text-slate-700 hover:bg-slate-50"
              >
                {t('forum.reportModal.cancel')}
              </button>
              <button
                type="button"
                onClick={() => void onSubmitReportModal()}
                disabled={isReporting}
                className="rounded-md bg-primary px-3 py-1.5 text-[13px] font-medium text-white hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isReporting ? t('common.loading') : t('forum.reportModal.submit')}
              </button>
            </div>
          </div>
        </div>
      ) : null}

      <section className="forum-compact-card overflow-hidden">
        <header className="border-b border-slate-200 bg-slate-50 px-4 py-2 text-[13px] font-semibold text-slate-600">
          <div className="flex flex-wrap items-center justify-between gap-2">
            <span>{isThreadTopic ? t('forum.threadMode.replyTitle') : t('forum.replies')}</span>
            <div className="inline-flex items-center gap-1 rounded-md border border-slate-200 bg-white p-0.5">
              <span className="px-1 text-[11px] font-medium text-slate-500">{t('forum.commentSection.sortLabel')}</span>
              <button
                type="button"
                onClick={() => onSortModeChange('top')}
                className={`rounded px-2 py-1 text-[11px] ${
                  commentSortMode === 'top' ? 'bg-primary/10 text-primary' : 'text-slate-600 hover:bg-slate-100'
                }`}
              >
                {t('forum.commentSection.sortTop')}
              </button>
              <button
                type="button"
                onClick={() => onSortModeChange('new')}
                className={`rounded px-2 py-1 text-[11px] ${
                  commentSortMode === 'new' ? 'bg-primary/10 text-primary' : 'text-slate-600 hover:bg-slate-100'
                }`}
              >
                {t('forum.commentSection.sortNew')}
              </button>
            </div>
          </div>
        </header>
        <form onSubmit={(event) => void onSubmitComment(event)} className="border-b border-slate-100 px-4 py-3">
          <label htmlFor="comment-content" className="mb-1 block text-[12px] font-medium text-slate-600">
            {isThreadTopic ? t('forum.threadMode.replyLabel') : t('forum.actions.reply')}
          </label>
          <textarea
            id="comment-content"
            value={commentDraft}
            onChange={(event) => onCommentDraftChange(event.target.value)}
            rows={3}
            className="w-full rounded-md border border-slate-300 px-3 py-2 text-[14px] focus:border-primary focus:outline-none"
            placeholder={
              isThreadTopic
                ? t('forum.threadMode.replyPlaceholder')
                : t('forum.detail.commentPlaceholder')
            }
          />
          <input
            type="file"
            multiple
            onChange={(e) => setCommentAttachments(Array.from(e.target.files ?? []))}
            className="mt-2 w-full rounded-md border border-slate-300 px-2 py-1.5 text-[13px] file:mr-2 file:rounded file:border-0 file:bg-slate-100 file:px-2 file:py-1 file:text-xs"
          />
          {commentAttachments.length > 0 ? (
            <p className="mt-1 text-[11px] text-slate-500">{commentAttachments.length} file(s) selected</p>
          ) : null}
          {hasTriedCommentSubmit && commentDraft.trim().length === 0 ? (
            <p className="mt-1 text-[12px] text-rose-600">{t('forum.feedback.commentRequired')}</p>
          ) : null}
          <div className="mt-2 flex justify-end">
            <button
              type="submit"
              disabled={!canSubmitComment}
              className="rounded-md bg-primary px-3 py-1.5 text-[13px] font-medium text-white hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60"
            >
              {isSubmittingComment || isUploadingAttachments
                ? t('common.loading')
                : isThreadTopic
                  ? t('forum.threadMode.replyAction')
                  : t('forum.actions.reply')}
            </button>
          </div>
        </form>
        <div className="divide-y divide-slate-100">
          {isCommentsLoading ? (
            <div className="px-4 py-3 text-[14px] text-slate-500">{t('common.loading')}</div>
          ) : null}
          {!isCommentsLoading
            ? commentThreads.map((node) => (
                <article key={node.id} className="px-4 py-3">
                  <CommentBranch node={node} depth={0} actions={commentActions} />
                </article>
              ))
            : null}
          {!isCommentsLoading && commentThreads.length === 0 ? (
            <div className="px-4 py-3 text-[14px] text-slate-500">{t('forum.commentSection.noComments')}</div>
          ) : null}
        </div>
      </section>
    </div>
  )
}
