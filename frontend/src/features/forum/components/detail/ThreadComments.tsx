import type { FormEvent } from 'react'
import type { User } from '@shared/types/auth'
import ReactMarkdown from 'react-markdown'
import remarkGfm from 'remark-gfm'
import { ThumbsUp, ThumbsDown, ChevronDown, ChevronUp, Bold, Italic, Link as LinkIcon, Code, Quote, Image as ImageIcon, Paperclip } from 'lucide-react'
import type { CommentThreadNode } from '../../hooks/useForumDetailPage'
import { parseForumRichContent } from '../../lib/parseForumRichContent'

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

function getRoleBadgeClasses(role: string): string | null {
  const normalized = role.trim().toLowerCase()
  switch (normalized) {
    case 'admin':
      return 'bg-rose-100 text-rose-800'
    case 'moderator':
      return 'bg-blue-100 text-blue-800'
    case 'teacher':
    case 'giảng viên':
      return 'bg-emerald-100 text-emerald-800'
    case 'student':
    case 'sinh viên':
    case 'user':
    case 'người dùng':
      return null
    case 'staff':
    case 'nhân viên':
      return 'bg-amber-100 text-amber-800'
    default:
      return 'bg-slate-200 text-slate-800'
  }
}

function getInitial(name?: string) {
  if (!name) return 'U'
  const parts = name.trim().split(' ')
  return parts[parts.length - 1].charAt(0).toUpperCase()
}

export interface CommentActions {
  replyingToId: string | null
  replyDraft: string
  hasTriedReplySubmit: boolean
  expandedIds: Set<string>
  onReplyDraftChange: (v: string) => void
  onStartReply: (id: string) => void
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
  userId: string | null
  hasModeratorRole: boolean
  onDeleteComment: (commentId: string) => void
  isDeletingComment: boolean
  hoveredCommentId: string | null
  setHoveredCommentId: (id: string | null) => void
  isLineHovered: (node: CommentThreadNode) => boolean
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
  const isExpanded = actions.expandedIds.has(node.id)
  const parsed = parseForumRichContent(node.content)
  const hasChildren = node.children.length > 0

  const avatarLetter = getInitial(node.authorName)

  return (
    <div 
      className={`group/thread relative ${depth > 0 ? 'mt-4' : 'mb-6'}`}
      onMouseEnter={(e) => {
        e.stopPropagation()
        actions.setHoveredCommentId(node.id)
      }}
      onMouseLeave={(e) => {
        e.stopPropagation()
        actions.setHoveredCommentId(null)
      }}
    >
      {/* Branch curve connecting parent's vertical line to this child's avatar */}
      {depth > 0 && (
        <div className={`absolute top-0 -left-[33px] w-[33px] h-[16px] rounded-bl-xl border-b-2 border-l-2 transition-colors z-0 pointer-events-none ${actions.isLineHovered(node) ? 'border-slate-300' : 'border-slate-100'}`} />
      )}
      
      {/* Main vertical line for this comment's children */}
      {hasChildren && isExpanded && (
        <div 
          onClick={() => actions.onToggleCollapse(node.id)}
          className={`absolute top-[32px] bottom-[24px] left-[15px] w-4 cursor-pointer border-l-2 transition-colors z-20 hover:border-slate-300 ${actions.isLineHovered(node) ? 'border-slate-300' : 'border-slate-100'}`} 
        />
      )}
      <div className="flex gap-4 relative z-10">
        <div className="flex-shrink-0">
          {node.authorAvatar ? (
            <img src={node.authorAvatar} alt={node.authorName} className="h-8 w-8 rounded-full object-cover shadow-sm" />
          ) : (
            <div className="flex h-8 w-8 select-none items-center justify-center rounded-full bg-slate-200 text-[13px] font-bold text-slate-700 shadow-sm">
              {avatarLetter}
            </div>
          )}
        </div>
        <div className="min-w-0 flex-1">
          <div className="py-1">
            <div className="flex flex-wrap items-center gap-2 text-[13px] mb-2">
              <span className="font-semibold text-slate-900">{node.authorName}</span>
              {node.authorRoles && node.authorRoles.length > 0 && (
                <div className="flex items-center gap-1">
                  {node.authorRoles.map(role => {
                    const badgeClass = getRoleBadgeClasses(role)
                    if (!badgeClass) return null
                    return (
                      <span key={role} className={`rounded-full px-2 py-0.5 text-[10px] font-bold uppercase tracking-wide ${badgeClass}`}>
                        {role}
                      </span>
                    )
                  })}
                </div>
              )}
              <span className="text-[12px] text-slate-400">•</span>
              <span className="text-[12px] tabular-nums text-slate-500">{time}</span>
            </div>
            {parsed.body ? (
              <div className="prose prose-slate max-w-none prose-p:text-[14px] prose-p:leading-[1.6]">
                <div className="break-words text-slate-800 m-0 prose prose-sm max-w-none prose-p:my-1">
                  <ReactMarkdown remarkPlugins={[remarkGfm]}>
                    {parsed.body}
                  </ReactMarkdown>
                </div>
              </div>
            ) : null}
            {parsed.imageUrls.length > 0 ? (
              <div className="mt-3 grid grid-cols-1 gap-2 sm:grid-cols-2">
                {parsed.imageUrls.map((url) => (
                  <a key={url} href={url} target="_blank" rel="noopener noreferrer" className="block">
                    <img
                      src={url}
                      alt="Attachment"
                      loading="lazy"
                      className="max-h-56 w-full rounded-lg border border-slate-200 object-contain"
                    />
                  </a>
                ))}
              </div>
            ) : null}

            <div className="mt-1 flex flex-wrap items-center gap-x-4 gap-y-2 text-[13px] font-medium text-slate-500">
              <button
                type="button"
                onClick={() => void actions.onVoteComment(node.id, 1)}
                disabled={actions.isVotingComment}
                className={`flex items-center gap-1.5 transition-colors disabled:opacity-50 ${isUpvoted ? 'text-emerald-600' : 'hover:text-emerald-600'}`}
              >
                <ThumbsUp className={`h-4 w-4 ${isUpvoted ? 'fill-current' : ''}`} />
                {node.voteScore > 0 && <span>{node.voteScore}</span>}
              </button>
              
              <button
                type="button"
                onClick={() => void actions.onVoteComment(node.id, 2)}
                disabled={actions.isVotingComment}
                className={`flex items-center gap-1.5 transition-colors disabled:opacity-50 ${isDownvoted ? 'text-rose-600' : 'hover:text-rose-600'}`}
              >
                <ThumbsDown className={`h-4 w-4 ${isDownvoted ? 'fill-current' : ''}`} />
              </button>

              <button
                type="button"
                onClick={() =>
                  isReplying
                    ? actions.onCancelReply()
                    : actions.onStartReply(node.id)
                }
                className="hover:text-slate-900 transition-colors"
              >
                {isReplying ? actions.t('forum.commentSection.cancelReply') : 'Trả lời'}
              </button>

              {(node.authorId === actions.userId || actions.hasModeratorRole) && (
                <>
                  <button type="button" onClick={() => void actions.onPinComment(node.id)} className="hover:text-slate-900 transition-colors">
                    {node.isPinned ? 'Bỏ ghim' : 'Ghim'}
                  </button>
                  <button type="button" onClick={() => alert('Tính năng sửa đang được phát triển')} className="hover:text-slate-900 transition-colors">
                    Chỉnh sửa
                  </button>
                  <button
                    type="button"
                    onClick={() => void actions.onDeleteComment(node.id)}
                    disabled={actions.isDeletingComment}
                    className="hover:text-rose-600 disabled:opacity-60 transition-colors"
                  >
                    Xóa
                  </button>
                </>
              )}
            </div>

            {isReplying ? (
              <form
                onSubmit={(e) => void actions.onSubmitReply(e)}
                className="mt-4 rounded-xl border border-slate-200 bg-slate-50 p-3 shadow-inner"
              >
                <textarea
                  value={actions.replyDraft}
                  onChange={(e) => actions.onReplyDraftChange(e.target.value)}
                  rows={3}
                  autoFocus
                  className="w-full rounded-lg border border-slate-300 px-3 py-2 text-[14px] focus:border-primary focus:ring-2 focus:ring-primary/20 focus:outline-none bg-white resize-y"
                  placeholder="Write a reply..."
                />
                <div className="mt-3 flex items-center justify-between">
                  <div className="flex gap-1">
                    {/* Mock Toolbar icons for visual consistency */}
                    <button type="button" onClick={() => actions.onReplyDraftChange(actions.replyDraft + ' **chữ đậm** ')} className="p-1.5 text-slate-400 hover:text-slate-700 rounded hover:bg-slate-200"><Bold className="h-4 w-4" /></button>
                    <button type="button" onClick={() => actions.onReplyDraftChange(actions.replyDraft + ' *chữ nghiêng* ')} className="p-1.5 text-slate-400 hover:text-slate-700 rounded hover:bg-slate-200"><Italic className="h-4 w-4" /></button>
                    <div className="w-[1px] h-4 bg-slate-300 mx-1 self-center" />
                    <button type="button" onClick={() => actions.onReplyDraftChange(actions.replyDraft + ' [liên kết](https://) ')} className="p-1.5 text-slate-400 hover:text-slate-700 rounded hover:bg-slate-200"><LinkIcon className="h-4 w-4" /></button>
                    <button type="button" onClick={() => actions.onReplyDraftChange(actions.replyDraft + ' `mã code` ')} className="p-1.5 text-slate-400 hover:text-slate-700 rounded hover:bg-slate-200"><Code className="h-4 w-4" /></button>
                    <button type="button" onClick={() => actions.onReplyDraftChange(actions.replyDraft + ' > trích dẫn \n')} className="p-1.5 text-slate-400 hover:text-slate-700 rounded hover:bg-slate-200"><Quote className="h-4 w-4" /></button>
                  </div>
                  <div className="flex items-center gap-2">
                    <button
                      type="button"
                      onClick={actions.onCancelReply}
                      className="rounded-lg px-4 py-1.5 text-[13px] font-semibold text-slate-600 hover:bg-slate-200 transition-colors"
                    >
                      Hủy
                    </button>
                    <button
                      type="submit"
                      disabled={!actions.replyDraft.trim()}
                      className="rounded-lg bg-primary px-5 py-1.5 text-[13px] font-semibold text-white shadow-sm hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60 transition-colors"
                    >
                      Trả lời
                    </button>
                  </div>
                </div>
              </form>
            ) : null}
          </div>
        </div>
      </div>

      {hasChildren && (
        <div className="mt-2 relative z-10">
          {!isExpanded ? (
            <div className="pl-[48px]">
              <button
                onClick={() => actions.onToggleCollapse(node.id)}
                className="flex items-center gap-2 rounded-full px-4 py-1.5 text-[13px] font-semibold text-primary bg-primary/5 hover:bg-primary/10 transition-colors"
              >
                <ChevronDown className="h-4 w-4" />
                {node.children.length} {actions.t('forum.replies') || 'replies'}
              </button>
            </div>
          ) : (
            <div className="relative pt-2">
              <div className="relative z-10 pl-[48px]">
                {node.children.map((ch) => (
                  <CommentBranch key={ch.id} node={ch} depth={depth + 1} actions={actions} />
                ))}
              </div>
              <div className="relative z-10 mt-2 pl-[48px]">
                <button
                  onClick={() => actions.onToggleCollapse(node.id)}
                  className="flex items-center gap-2 rounded-full px-4 py-1.5 text-[13px] font-semibold text-slate-600 hover:bg-slate-100 hover:text-slate-900 transition-colors"
                >
                  <ChevronUp className="h-4 w-4" />
                  Ẩn bình luận
                </button>
              </div>
            </div>
          )}
        </div>
      )}
    </div>
  )
}

interface ThreadCommentsProps {
  commentCount: number
  isCommentsLoading: boolean
  commentThreads: CommentThreadNode[]
  commentActions: CommentActions
  commentDraft: string
  onCommentDraftChange: (v: string) => void
  hasTriedCommentSubmit: boolean
  canSubmitComment: boolean
  isSubmittingComment: boolean
  isUploadingAttachments: boolean
  onSubmitComment: (e: FormEvent<HTMLFormElement>) => Promise<void>
  currentUser?: User | null
  commentSortMode: 'top' | 'new'
  setCommentSortMode: (mode: 'top' | 'new') => void
}

export function ThreadComments({
  commentCount,
  isCommentsLoading,
  commentThreads,
  commentActions,
  commentDraft,
  onCommentDraftChange,
  hasTriedCommentSubmit,
  canSubmitComment,
  isSubmittingComment,
  isUploadingAttachments,
  onSubmitComment,
  currentUser,
  commentSortMode,
  setCommentSortMode,
}: ThreadCommentsProps) {
  const userInitials = getInitial(currentUser?.fullName)

  const insertFormat = (format: string) => {
    onCommentDraftChange(commentDraft + format)
  }
  return (
    <div className="mt-4 rounded-xl border border-slate-200 bg-white">
      <div className="flex flex-wrap items-center justify-between border-b border-slate-100 p-4 sm:p-6 pb-4">
        <h3 className="text-[16px] font-bold text-slate-900">
          Trả lời ({commentCount})
        </h3>
        <div className="flex items-center gap-2 text-[13px]">
          <span className="text-slate-500 hidden sm:inline">Sắp xếp:</span>
          <div className="flex bg-slate-100/80 rounded-lg p-1">
            <button 
              onClick={() => setCommentSortMode('top')}
              className={`px-4 py-1.5 rounded-md shadow-sm font-semibold transition-colors ${commentSortMode === 'top' ? 'bg-white text-sky-700' : 'text-slate-600 hover:text-slate-900 shadow-none bg-transparent'}`}
            >
              Top
            </button>
            <button 
              onClick={() => setCommentSortMode('new')}
              className={`px-4 py-1.5 rounded-md shadow-sm font-semibold transition-colors ${commentSortMode === 'new' ? 'bg-white text-sky-700' : 'text-slate-600 hover:text-slate-900 shadow-none bg-transparent'}`}
            >
              Mới nhất
            </button>
          </div>
        </div>
      </div>

      <div className="p-4 sm:p-6 pt-6">
        <div className="mb-8">
          <form onSubmit={(e) => void onSubmitComment(e)} className="rounded-xl border border-slate-200 bg-white focus-within:border-primary focus-within:ring-1 focus-within:ring-primary transition-all overflow-hidden shadow-sm">
            <div className="p-3 pb-0 flex gap-3">
              <div className="flex-shrink-0 mt-1">
                {currentUser?.avatar ? (
                  <img src={currentUser.avatar} alt="User Avatar" className="h-9 w-9 rounded-full object-cover shadow-sm" />
                ) : (
                  <div className="flex h-9 w-9 select-none items-center justify-center rounded-full bg-indigo-600 text-[14px] font-bold text-white shadow-sm">
                    {userInitials}
                  </div>
                )}
              </div>
              <textarea
                value={commentDraft}
                onChange={(e) => onCommentDraftChange(e.target.value)}
                rows={2}
                className="w-full text-[14px] bg-transparent outline-none resize-none placeholder-slate-400 min-h-[50px] mt-1.5"
                placeholder="Viết bình luận của bạn..."
              />
            </div>
            
            <div className="px-3 py-3 mt-2 flex flex-wrap items-center justify-between border-t border-slate-100 bg-slate-50">
              <div className="flex gap-1 text-slate-500">
                <button type="button" onClick={() => insertFormat(' **chữ đậm** ')} className="p-1.5 hover:text-slate-900 rounded hover:bg-slate-200/60 transition-all"><Bold className="h-4 w-4" /></button>
                <button type="button" onClick={() => insertFormat(' *chữ nghiêng* ')} className="p-1.5 hover:text-slate-900 rounded hover:bg-slate-200/60 transition-all"><Italic className="h-4 w-4" /></button>
                <button type="button" onClick={() => insertFormat(' `mã code` ')} className="p-1.5 hover:text-slate-900 rounded hover:bg-slate-200/60 transition-all"><Code className="h-4 w-4" /></button>
                <button type="button" onClick={() => insertFormat(' [liên kết](https://) ')} className="p-1.5 hover:text-slate-900 rounded hover:bg-slate-200/60 transition-all"><LinkIcon className="h-4 w-4" /></button>
                <button type="button" onClick={() => insertFormat(' ![hình ảnh](https://) ')} className="p-1.5 hover:text-slate-900 rounded hover:bg-slate-200/60 transition-all"><ImageIcon className="h-4 w-4" /></button>
                <button type="button" className="p-1.5 hover:text-slate-900 rounded hover:bg-slate-200/60 transition-all"><Paperclip className="h-4 w-4" /></button>
              </div>
              <div className="flex items-center gap-3 mt-2 sm:mt-0">
                {hasTriedCommentSubmit && !commentDraft.trim() && (
                  <span className="text-[12px] font-medium text-rose-600">Bình luận trống</span>
                )}
                <button type="button" className="text-[13px] font-semibold text-slate-600 border border-slate-300 bg-white px-3 py-1.5 rounded-lg flex items-center gap-2 hover:bg-slate-50 transition-colors">
                  <Paperclip className="h-4 w-4" /> Chọn tệp
                </button>
                <button
                  type="submit"
                  disabled={!canSubmitComment}
                  className="rounded-lg bg-primary px-5 py-1.5 text-[14px] font-bold text-white shadow-sm hover:bg-primary-hover disabled:cursor-not-allowed disabled:opacity-60 transition-colors"
                >
                  {isSubmittingComment || isUploadingAttachments ? 'Đang gửi...' : 'Trả lời'}
                </button>
              </div>
            </div>
          </form>
        </div>

        <div className="space-y-4">
          {isCommentsLoading ? (
            <div className="py-8 text-center text-[14px] font-medium text-slate-500">Đang tải bình luận...</div>
          ) : commentThreads.length > 0 ? (
            commentThreads.map((node) => (
              <CommentBranch key={node.id} node={node} depth={0} actions={commentActions} />
            ))
          ) : (
            <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 py-12 text-center">
              <h4 className="text-[15px] font-bold text-slate-900 mb-1">Chưa có bình luận</h4>
              <p className="text-[14px] text-slate-500 mb-4">Hãy là người đầu tiên trả lời và bắt đầu cuộc thảo luận.</p>
            </div>
          )}
        </div>
      </div>
    </div>
  )
}
