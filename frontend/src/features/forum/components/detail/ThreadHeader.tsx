import { Eye, MessageCircle, Heart, Bookmark, Clock3, Pin, Lock, CheckCircle2 } from 'lucide-react'
import type { ForumDetailItem } from '../../api/forum.list.api'

interface ThreadHeaderProps {
  post: Partial<ForumDetailItem>
  title: string
  category: string
  authorLine: string | null
  activityText: string
  t: (key: string) => string
  children?: React.ReactNode
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

export function ThreadHeader({ post, title, category, authorLine, activityText, t, children }: ThreadHeaderProps) {
  const avatarLetter = (post.authorName || 'U').charAt(0).toUpperCase()
  
  // Try to parse author roles if provided
  const authorRoles: string[] = [] // Post API doesn't return author roles directly right now, leaving empty for mock

  return (
    <div className="transition-all duration-200">
      <h1 className="text-[28px] sm:text-[32px] font-bold text-slate-900 leading-tight">
        {title}
      </h1>

      {/* Status Badges */}
      {(post.isPinned || post.isLocked || post.isSolved) && (
        <div className="mt-3 flex flex-wrap gap-2">
          {post.isPinned && (
            <span className="inline-flex items-center gap-1 rounded bg-amber-50 px-2 py-1 text-[12px] font-medium text-amber-700">
              <Pin className="h-3.5 w-3.5" /> Pinned
            </span>
          )}
          {post.isSolved && (
            <span className="inline-flex items-center gap-1 rounded bg-emerald-50 px-2 py-1 text-[12px] font-medium text-emerald-700">
              <CheckCircle2 className="h-3.5 w-3.5" /> Solved
            </span>
          )}
          {post.isLocked && (
            <span className="inline-flex items-center gap-1 rounded bg-slate-100 px-2 py-1 text-[12px] font-medium text-slate-700">
              <Lock className="h-3.5 w-3.5" /> Locked
            </span>
          )}
        </div>
      )}

      {/* Author Row */}
      <div className="mt-4 flex items-center gap-3">
        <div className="flex-shrink-0">
          {post.authorAvatar ? (
            <img src={post.authorAvatar} alt={post.authorName} className="h-10 w-10 rounded-full object-cover" />
          ) : (
            <div className="flex h-10 w-10 select-none items-center justify-center rounded-full bg-slate-200 text-[15px] font-bold text-slate-700">
              {avatarLetter}
            </div>
          )}
        </div>
        <div className="flex-1 min-w-0">
          <div className="flex flex-wrap items-center gap-x-2 gap-y-1">
            <span className="font-semibold text-slate-900 text-[15px]">{post.authorName || authorLine}</span>
            {authorRoles.map(role => {
              const badgeClass = getRoleBadgeClasses(role)
              if (!badgeClass) return null
              return (
                <span key={role} className={`rounded-full px-2 py-0.5 text-[11px] font-bold uppercase tracking-wide ${badgeClass}`}>
                  {role}
                </span>
              )
            })}
          </div>
          <div className="flex flex-wrap items-center gap-2 mt-0.5 text-[13px] text-slate-500">
            <span className="font-medium text-slate-600">{category}</span>
            <span className="text-slate-300">•</span>
            <span>{t('forum.meta.lastUpdated') || 'Updated'} {activityText}</span>
          </div>
        </div>
      </div>

      {/* Statistics & Action Row */}
      <div className="mt-6 flex flex-wrap items-center justify-between gap-4 border-b border-slate-100 pb-4">
        <div className="flex flex-wrap items-center gap-4 text-[13px] font-medium text-slate-500">
          <div className="flex items-center gap-1.5" title="Views">
            <Eye className="h-4 w-4" />
            <span>{post.viewCount ?? 0}</span>
          </div>
          <div className="flex items-center gap-1.5" title="Replies">
            <MessageCircle className="h-4 w-4" />
            <span>{post.replyCount ?? 0}</span>
          </div>
          <div className="flex items-center gap-1.5" title="Likes">
            <Heart className="h-4 w-4" />
            <span>{post.likeCount ?? post.voteScore ?? 0}</span>
          </div>
          <div className="flex items-center gap-1.5" title="Bookmarks">
            <Bookmark className="h-4 w-4" />
            <span>{post.bookmarkCount ?? 0}</span>
          </div>
        </div>
        <div className="flex flex-wrap items-center gap-3">
          {children}
        </div>
      </div>

      {/* Tags */}
      {post.tags && post.tags.length > 0 && (
        <div className="mt-4 flex flex-wrap gap-2">
          {post.tags.map((tag: string) => (
            <span key={tag} className="inline-flex items-center px-2.5 py-1 rounded-md border border-slate-200 bg-slate-50 text-[12px] font-medium text-slate-600 hover:bg-slate-100 cursor-pointer transition-colors">
              {tag}
            </span>
          ))}
        </div>
      )}
    </div>
  )
}
