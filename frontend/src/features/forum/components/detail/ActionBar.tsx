import { Heart, Bookmark, UserPlus, Share2, Flag } from 'lucide-react'

interface ActionBarProps {
  isUpvoted: boolean
  isVoting: boolean
  onUpvotePost: () => void
  voteScore: number
  isBookmarked: boolean
  isBookmarking: boolean
  isUnbookmarking: boolean
  onToggleBookmark: () => void
  onSharePost: () => void
  onOpenReportModal: () => void
  isReporting: boolean
}

export function ActionBar({
  isUpvoted,
  isVoting,
  onUpvotePost,
  voteScore,
  isBookmarked,
  isBookmarking,
  isUnbookmarking,
  onToggleBookmark,
  onSharePost,
  onOpenReportModal,
  isReporting
}: ActionBarProps) {
  return (
    <div className="mt-8 flex flex-wrap items-center gap-3 border-y border-slate-200 py-4">
      {/* Primary Actions */}
      <button
        type="button"
        onClick={onUpvotePost}
        disabled={isVoting}
        className={`inline-flex items-center gap-2 rounded-lg px-4 py-2 text-[14px] font-semibold transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed ${
          isUpvoted
            ? 'bg-rose-50 text-rose-600 hover:bg-rose-100'
            : 'bg-slate-50 text-slate-600 hover:bg-slate-100 hover:text-slate-900'
        }`}
      >
        <Heart className={`h-4 w-4 ${isUpvoted ? 'fill-current' : ''}`} />
        <span>{isUpvoted ? 'Liked' : 'Like'}</span>
        {voteScore > 0 && <span className="ml-1 rounded-full bg-white/50 px-1.5 py-0.5 text-[11px] font-bold">{voteScore}</span>}
      </button>

      <button
        type="button"
        onClick={onToggleBookmark}
        disabled={isBookmarking || isUnbookmarking}
        className={`inline-flex items-center gap-2 rounded-lg px-4 py-2 text-[14px] font-semibold transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed ${
          isBookmarked
            ? 'bg-amber-50 text-amber-700 hover:bg-amber-100'
            : 'bg-slate-50 text-slate-600 hover:bg-slate-100 hover:text-slate-900'
        }`}
      >
        <Bookmark className={`h-4 w-4 ${isBookmarked ? 'fill-current' : ''}`} />
        <span>{isBookmarked ? 'Saved' : 'Save'}</span>
      </button>

      <button
        type="button"
        className="inline-flex items-center gap-2 rounded-lg bg-slate-50 px-4 py-2 text-[14px] font-semibold text-slate-600 transition-all duration-200 hover:bg-slate-100 hover:text-slate-900"
      >
        <UserPlus className="h-4 w-4" />
        <span>Follow</span>
      </button>

      <div className="flex-1" />

      {/* Secondary Actions */}
      <div className="flex items-center gap-1">
        <button
          type="button"
          onClick={onSharePost}
          className="inline-flex items-center justify-center h-10 w-10 rounded-lg text-slate-500 hover:bg-slate-100 hover:text-slate-900 transition-colors"
          title="Share"
        >
          <Share2 className="h-4 w-4" />
        </button>
        <button
          type="button"
          onClick={onOpenReportModal}
          disabled={isReporting}
          className="inline-flex items-center justify-center h-10 w-10 rounded-lg text-slate-500 hover:bg-rose-50 hover:text-rose-600 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
          title="Report"
        >
          <Flag className="h-4 w-4" />
        </button>
      </div>
    </div>
  )
}
