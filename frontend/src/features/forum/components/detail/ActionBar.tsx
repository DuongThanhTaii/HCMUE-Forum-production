import { Heart, Bookmark, Share2, Flag } from 'lucide-react'

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
    <div className="flex items-center gap-2">
      {/* Primary Actions */}
      <button
        type="button"
        onClick={onUpvotePost}
        disabled={isVoting}
        className={`inline-flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-[13px] font-medium transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed ${
          isUpvoted
            ? 'border-emerald-500 bg-emerald-50 text-emerald-600 hover:bg-emerald-100'
            : 'border-slate-200 bg-white text-slate-600 hover:bg-slate-50 hover:text-slate-900'
        }`}
      >
        <Heart className={`h-3.5 w-3.5 ${isUpvoted ? 'fill-current' : ''}`} />
        <span>Thích</span>
        {voteScore > 0 && <span className="ml-0.5 rounded px-1 py-0.5 text-[11px] font-bold bg-slate-100 text-slate-700">{voteScore}</span>}
      </button>

      <button
        type="button"
        onClick={onToggleBookmark}
        disabled={isBookmarking || isUnbookmarking}
        className={`inline-flex items-center gap-1.5 rounded-lg border px-3 py-1.5 text-[13px] font-medium transition-all duration-200 disabled:opacity-60 disabled:cursor-not-allowed ${
          isBookmarked
            ? 'border-amber-400 bg-amber-50 text-amber-700 hover:bg-amber-100'
            : 'border-slate-200 bg-white text-slate-600 hover:bg-slate-50 hover:text-slate-900'
        }`}
      >
        <Bookmark className={`h-3.5 w-3.5 ${isBookmarked ? 'fill-current' : ''}`} />
        <span>Đã lưu</span>
      </button>

      {/* Secondary Actions */}
      <button
        type="button"
        onClick={onSharePost}
        className="inline-flex items-center gap-1.5 rounded-lg border border-slate-200 bg-white px-3 py-1.5 text-[13px] font-medium text-slate-600 hover:bg-slate-50 hover:text-slate-900 transition-colors"
        title="Chia sẻ"
      >
        <Share2 className="h-3.5 w-3.5" />
        <span>Chia sẻ</span>
      </button>

      <button
        type="button"
        onClick={onOpenReportModal}
        disabled={isReporting}
        className="inline-flex items-center justify-center rounded-lg border border-transparent px-2 py-1.5 text-slate-400 hover:bg-rose-50 hover:text-rose-600 transition-colors disabled:opacity-60 disabled:cursor-not-allowed"
        title="Báo cáo"
      >
        <Flag className="h-4 w-4" />
      </button>
    </div>
  )
}
