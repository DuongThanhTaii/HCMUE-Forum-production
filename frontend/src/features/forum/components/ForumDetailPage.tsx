import { useState } from 'react'
import { Link } from 'react-router-dom'
import { ChevronRight } from 'lucide-react'

import { useForumDetailPage } from '../hooks/useForumDetailPage'
import { ConfirmDeleteCommentModal } from './ConfirmDeleteCommentModal'

// Detail Components
import { ThreadHeader } from './detail/ThreadHeader'
import { AISummaryCard } from './detail/AISummaryCard'
import { ThreadContent } from './detail/ThreadContent'
import { ActionBar } from './detail/ActionBar'
import { ThreadComments, type CommentActions } from './detail/ThreadComments'
import { RightSidebar } from './detail/RightSidebar'

export function ForumDetailPage() {
  const [expandedCommentIds, setExpandedCommentIds] = useState<Set<string>>(new Set())
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
    onCommentDraftChange,
    hasTriedCommentSubmit,
    canSubmitComment,
    onSubmitComment,
    onUpvotePost,
    onVoteComment,
    onAcceptAnswer,
    onPinComment,
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
    onSummarizePost,
    onLoadRelatedPosts,
    interactionErrorKey,
    interactionSuccessKey,
    copilotError,
    copilotSummary,
    copilotRelated,
    hasModeratorRole,
    isQuestionPost,
    canAcceptAnswer,
    canPinComment,
    isBookmarked,
    isUpvoted,
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
    isLoading,
    isError,
    userId,
    onDeleteComment,
    isDeletingComment,
    hoveredCommentId,
    setHoveredCommentId,
    isLineHovered,
    commentToDelete,
    setCommentToDelete,
    onConfirmDeleteComment,
  } = useForumDetailPage()

  function onToggleCollapse(commentId: string) {
    setExpandedCommentIds((prev) => {
      const next = new Set(prev)
      if (next.has(commentId)) next.delete(commentId)
      else next.add(commentId)
      return next
    })
  }

  const commentActions: CommentActions = {
    replyingToId: replyingToCommentId,
    replyDraft,
    hasTriedReplySubmit,
    expandedIds: expandedCommentIds,
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
    userId,
    hasModeratorRole,
    onDeleteComment,
    isDeletingComment,
    hoveredCommentId,
    setHoveredCommentId,
    isLineHovered,
  }

  if (isLoading) {
    return (
      <div className="flex justify-center py-20">
        <div className="space-y-6 w-full max-w-3xl animate-pulse">
          <div className="h-4 w-32 bg-slate-200 rounded"></div>
          <div className="h-32 bg-slate-200 rounded-xl"></div>
          <div className="space-y-3">
            <div className="h-4 w-full bg-slate-200 rounded"></div>
            <div className="h-4 w-full bg-slate-200 rounded"></div>
            <div className="h-4 w-3/4 bg-slate-200 rounded"></div>
          </div>
        </div>
      </div>
    )
  }

  if (isError) {
    return (
      <div className="flex justify-center py-20">
        <div className="rounded-xl border border-red-200 bg-red-50 p-8 text-center max-w-md">
          <h2 className="text-lg font-bold text-red-800 mb-2">Failed to load thread</h2>
          <p className="text-sm text-red-600 mb-4">{t('forum.error.loadFailed') || 'There was an error loading this discussion.'}</p>
          <button onClick={() => window.location.reload()} className="px-4 py-2 bg-red-100 text-red-700 font-semibold rounded-lg hover:bg-red-200 transition-colors">
            Try again
          </button>
        </div>
      </div>
    )
  }

  return (
    <div className="relative">
      <div className="grid grid-cols-1 lg:grid-cols-[minmax(0,1fr)_340px] xl:grid-cols-[minmax(0,1fr)_360px] gap-8">
        
        {/* Main Content Column */}
        <main className="min-w-0">
          
          {/* Breadcrumb */}
          <nav className="flex items-center text-[13px] font-medium text-slate-500 mb-6" aria-label="Breadcrumb">
            <Link to="/explore" className="hover:text-primary transition-colors">
              Explore
            </Link>
            <ChevronRight className="h-4 w-4 mx-1.5 text-slate-300 flex-shrink-0" />
            <Link to={`/discussions/${post?.categorySlug || 'general'}`} className="hover:text-primary transition-colors">
              {category}
            </Link>
            <ChevronRight className="h-4 w-4 mx-1.5 text-slate-300 flex-shrink-0" />
            <span className="text-slate-900 truncate" aria-current="page">
              {title}
            </span>
          </nav>

          {/* Single Content Block */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm mb-6">
            <div className="p-6">
              {/* Thread Header Card */}
              <ThreadHeader 
                post={post || {}} 
                title={title} 
                category={category} 
                authorLine={authorLine} 
                activityText={activityText} 
                t={t} 
              >
                {/* Action Bar inside Header */}
                <ActionBar
                  isUpvoted={isUpvoted}
                  isVoting={isVoting}
                  onUpvotePost={onUpvotePost}
                  voteScore={voteScore}
                  isBookmarked={isBookmarked}
                  isBookmarking={isBookmarking}
                  isUnbookmarking={isUnbookmarking}
                  onToggleBookmark={onToggleBookmark}
                  onSharePost={onSharePost}
                  onOpenReportModal={onOpenReportModal}
                  isReporting={isReporting}
                />
              </ThreadHeader>

              {/* AI Summary Card */}
              <AISummaryCard 
                summary={copilotSummary} 
                isLoading={isSummarizing} 
                error={copilotError} 
                onRetry={onSummarizePost} 
              />

              {/* Provide a way to manually trigger AI summary if it hasn't been triggered */}
              {!copilotSummary && !isSummarizing && !copilotError && (
                 <div className="mt-6">
                    <button
                      onClick={onSummarizePost}
                      className="flex items-center gap-2 text-[13px] font-semibold text-sky-600 hover:text-sky-700 hover:underline"
                    >
                      ✨ Generate AI Summary
                    </button>
                 </div>
              )}

              {/* Thread Content */}
              <div className="mt-8">
                <h2 className="text-[18px] font-bold text-slate-900 mb-4">Nội dung</h2>
                <ThreadContent content={postContent} t={t} />
              </div>
              
              <div className="mt-8 text-[13px] text-slate-500 border-t border-slate-100 pt-4">
                Cập nhật lần cuối: {activityText}
              </div>
            </div>
          </div>

          {/* Interaction Status Messages */}
          {(interactionSuccessKey || interactionErrorKey) && (
            <div className="mt-4 space-y-2">
              {interactionSuccessKey && (
                <div className="rounded-lg border border-emerald-200 bg-emerald-50 px-4 py-3 text-[13px] font-medium text-emerald-700 shadow-sm">
                  {t(interactionSuccessKey)}
                </div>
              )}
              {interactionErrorKey && (
                <div className="rounded-lg border border-rose-200 bg-rose-50 px-4 py-3 text-[13px] font-medium text-rose-700 shadow-sm">
                  {t(interactionErrorKey)}
                </div>
              )}
            </div>
          )}

          {/* Comments Section */}
          <ThreadComments
            commentCount={post?.replyCount ?? 0}
            isCommentsLoading={isCommentsLoading}
            commentThreads={commentThreads}
            commentActions={commentActions}
            commentDraft={commentDraft}
            onCommentDraftChange={onCommentDraftChange}
            hasTriedCommentSubmit={hasTriedCommentSubmit}
            canSubmitComment={canSubmitComment}
            isSubmittingComment={isSubmittingComment}
            isUploadingAttachments={isUploadingAttachments}
            onSubmitComment={onSubmitComment}
          />
        </main>

        {/* Right Sidebar Column */}
        <RightSidebar
          post={post || {}}
          authorLine={authorLine}
          relatedPosts={copilotRelated}
          isLoadingRelated={isLoadingRelated}
          onLoadRelatedPosts={onLoadRelatedPosts}
          tags={post?.tags || []}
        />
      </div>

      {/* Report Modal */}
      {reportOpen ? (
        <div
          className="fixed inset-0 z-50 flex items-center justify-center bg-slate-900/50 backdrop-blur-sm p-4 transition-opacity"
          role="presentation"
          tabIndex={-1}
          onClick={onCloseReportModal}
          onKeyDown={(e) => {
            if (e.key === 'Escape') onCloseReportModal()
          }}
        >
          <div
            role="dialog"
            aria-modal="true"
            aria-labelledby="report-dialog-title"
            className="w-full max-w-md rounded-xl border border-slate-200 bg-white p-6 shadow-xl"
            onClick={(e) => e.stopPropagation()}
          >
            <h2 id="report-dialog-title" className="text-[18px] font-bold text-slate-900">
              {t('forum.reportModal.title') || 'Report Post'}
            </h2>
            <p className="mt-2 text-[14px] text-slate-500">{t('forum.reportModal.subtitle') || 'Please select a reason for reporting this post.'}</p>
            
            <label htmlFor="report-reason" className="mt-6 block text-[13px] font-semibold text-slate-700">
              {t('forum.reportModal.reasonLabel') || 'Reason'}
            </label>
            <select
              id="report-reason"
              value={reportReason}
              onChange={(e) => setReportReason(Number(e.target.value))}
              className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2.5 text-[14px] text-slate-900 focus:border-primary focus:ring-2 focus:ring-primary/20 focus:outline-none"
            >
              <option value={1}>{t('forum.reportModal.reasons.spam') || 'Spam'}</option>
              <option value={2}>{t('forum.reportModal.reasons.harassment') || 'Harassment'}</option>
              <option value={3}>{t('forum.reportModal.reasons.inappropriate') || 'Inappropriate content'}</option>
              <option value={4}>{t('forum.reportModal.reasons.misinformation') || 'Misinformation'}</option>
              <option value={5}>{t('forum.reportModal.reasons.offTopic') || 'Off-topic'}</option>
              <option value={6}>{t('forum.reportModal.reasons.copyright') || 'Copyright violation'}</option>
              <option value={99}>{t('forum.reportModal.reasons.other') || 'Other'}</option>
            </select>
            
            <label htmlFor="report-details" className="mt-4 block text-[13px] font-semibold text-slate-700">
              {t('forum.reportModal.detailsLabel') || 'Additional Details'}
            </label>
            <textarea
              id="report-details"
              value={reportDescription}
              onChange={(e) => setReportDescription(e.target.value)}
              rows={4}
              className="mt-1.5 w-full rounded-lg border border-slate-300 px-3 py-2 text-[14px] focus:border-primary focus:ring-2 focus:ring-primary/20 focus:outline-none resize-y"
              placeholder={t('forum.reportModal.detailsPlaceholder') || 'Please provide any additional context...'}
            />
            
            <div className="mt-6 flex justify-end gap-3">
              <button
                type="button"
                onClick={onCloseReportModal}
                className="rounded-lg px-4 py-2 text-[14px] font-semibold text-slate-600 hover:bg-slate-100 transition-colors"
              >
                {t('forum.reportModal.cancel') || 'Cancel'}
              </button>
              <button
                type="button"
                onClick={() => void onSubmitReportModal()}
                disabled={isReporting}
                className="rounded-lg bg-rose-600 px-4 py-2 text-[14px] font-semibold text-white shadow-sm hover:bg-rose-700 disabled:cursor-not-allowed disabled:opacity-60 transition-colors"
              >
                {isReporting ? t('common.loading') || 'Submitting...' : t('forum.reportModal.submit') || 'Submit Report'}
              </button>
            </div>
          </div>
        </div>
      ) : null}

      <ConfirmDeleteCommentModal
        isOpen={commentToDelete !== null}
        isDeleting={isDeletingComment}
        onClose={() => setCommentToDelete(null)}
        onConfirm={onConfirmDeleteComment}
      />
    </div>
  )
}
