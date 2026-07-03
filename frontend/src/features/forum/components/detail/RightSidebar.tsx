import { Calendar, UserRound, Eye, MessageCircle, Heart, Bookmark, Clock3, GraduationCap, Briefcase, FileText } from 'lucide-react'
import { Link } from 'react-router-dom'
import type { ForumDetailItem } from '../../../api/forum.list.api'
import type { RelatedPostsResult } from '@features/assistant/api/assistant.api'

interface RightSidebarProps {
  post: Partial<ForumDetailItem>
  authorLine: string | null
  relatedPosts: RelatedPostsResult | null
  isLoadingRelated: boolean
  onLoadRelatedPosts: () => void
  tags: string[]
}

export function RightSidebar({ post, authorLine, relatedPosts, isLoadingRelated, onLoadRelatedPosts, tags }: RightSidebarProps) {
  const avatarLetter = (post.authorName || authorLine || 'U').charAt(0).toUpperCase()

  // Mock data for similar resources
  const mockedResources = [
    { title: 'Introduction to C#', type: 'Course', icon: GraduationCap },
    { title: '.NET Developer', type: 'Job', icon: Briefcase },
    { title: 'C# Coding Guidelines', type: 'Document', icon: FileText },
  ]

  return (
    <aside className="space-y-6 w-full lg:w-[340px] xl:w-[360px] shrink-0 sticky top-24 self-start hidden lg:block">
      
      {/* 1. Author Card */}
      <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
        <div className="flex items-center gap-4">
          <div className="flex-shrink-0">
            {post.authorAvatar ? (
              <img src={post.authorAvatar} alt={post.authorName} className="h-14 w-14 rounded-full object-cover" />
            ) : (
              <div className="flex h-14 w-14 select-none items-center justify-center rounded-full bg-slate-200 text-[20px] font-bold text-slate-700">
                {avatarLetter}
              </div>
            )}
          </div>
          <div>
            <h3 className="font-bold text-slate-900 text-[16px]">{post.authorName || authorLine || 'Unknown Author'}</h3>
            <p className="text-[13px] text-slate-500">Student • Faculty of IT</p>
          </div>
        </div>
        
        <div className="mt-4 grid grid-cols-2 gap-3 border-t border-slate-100 pt-4">
          <div className="text-center">
            <span className="block text-[16px] font-bold text-slate-900">42</span>
            <span className="block text-[11px] font-medium uppercase tracking-wider text-slate-500">Threads</span>
          </div>
          <div className="text-center border-l border-slate-100">
            <span className="block text-[16px] font-bold text-slate-900">128</span>
            <span className="block text-[11px] font-medium uppercase tracking-wider text-slate-500">Replies</span>
          </div>
        </div>

        <div className="mt-4 flex items-center justify-center gap-1.5 text-[12px] text-slate-500 bg-slate-50 rounded-lg py-2">
          <Calendar className="h-3.5 w-3.5" />
          <span>Joined Sep 2024</span>
        </div>

        <button className="mt-3 w-full rounded-lg bg-primary/10 py-2.5 text-[14px] font-semibold text-primary hover:bg-primary/20 transition-colors">
          Follow User
        </button>
      </div>

      {/* 2. Thread Statistics */}
      <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
        <h3 className="font-bold text-slate-900 text-[15px] mb-4">Thread Statistics</h3>
        <div className="space-y-3">
          <div className="flex items-center justify-between text-[13px]">
            <div className="flex items-center gap-2 text-slate-600"><Eye className="h-4 w-4" /> Views</div>
            <span className="font-semibold text-slate-900">{post.viewCount ?? 0}</span>
          </div>
          <div className="flex items-center justify-between text-[13px]">
            <div className="flex items-center gap-2 text-slate-600"><MessageCircle className="h-4 w-4" /> Replies</div>
            <span className="font-semibold text-slate-900">{post.replyCount ?? 0}</span>
          </div>
          <div className="flex items-center justify-between text-[13px]">
            <div className="flex items-center gap-2 text-slate-600"><Heart className="h-4 w-4" /> Likes</div>
            <span className="font-semibold text-slate-900">{post.likeCount ?? post.voteScore ?? 0}</span>
          </div>
          <div className="flex items-center justify-between text-[13px]">
            <div className="flex items-center gap-2 text-slate-600"><Bookmark className="h-4 w-4" /> Bookmarks</div>
            <span className="font-semibold text-slate-900">{post.bookmarkCount ?? 0}</span>
          </div>
          <div className="flex items-center justify-between text-[13px]">
            <div className="flex items-center gap-2 text-slate-600"><Clock3 className="h-4 w-4" /> Reading Time</div>
            <span className="font-semibold text-slate-900">5 min</span>
          </div>
        </div>
      </div>

      {/* 3. Related Discussions */}
      <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
        <div className="flex items-center justify-between mb-4">
          <h3 className="font-bold text-slate-900 text-[15px]">Related Discussions</h3>
          {!relatedPosts && !isLoadingRelated && (
            <button onClick={onLoadRelatedPosts} className="text-[12px] font-medium text-primary hover:underline">
              Load
            </button>
          )}
        </div>
        
        {isLoadingRelated ? (
          <div className="space-y-4">
            {[1, 2, 3].map(i => (
              <div key={i} className="animate-pulse">
                <div className="h-4 w-full bg-slate-200 rounded"></div>
                <div className="h-3 w-1/2 bg-slate-100 rounded mt-2"></div>
              </div>
            ))}
          </div>
        ) : relatedPosts?.items?.length ? (
          <div className="space-y-4">
            {relatedPosts.items.slice(0, 5).map((item) => (
              <Link key={item.id} to={`/threads/${item.id}`} className="block group">
                <h4 className="text-[13px] font-medium text-slate-800 leading-snug group-hover:text-primary transition-colors line-clamp-2">
                  {item.title}
                </h4>
                <div className="flex items-center gap-2 mt-1.5 text-[11px] text-slate-500">
                  <span className="bg-slate-100 px-1.5 py-0.5 rounded text-slate-600">Discussion</span>
                  <span>•</span>
                  <span>14 replies</span>
                </div>
              </Link>
            ))}
          </div>
        ) : (
          <p className="text-[13px] text-slate-500 text-center py-2">No related discussions found.</p>
        )}
      </div>

      {/* 4. Popular Tags */}
      {tags && tags.length > 0 && (
        <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
          <h3 className="font-bold text-slate-900 text-[15px] mb-4">Thread Tags</h3>
          <div className="flex flex-wrap gap-2">
            {tags.map(tag => (
              <Link key={tag} to={`/explore?tag=${tag}`} className="inline-flex px-2.5 py-1 bg-slate-50 border border-slate-200 rounded-md text-[12px] font-medium text-slate-700 hover:bg-slate-100 transition-colors">
                {tag}
              </Link>
            ))}
          </div>
        </div>
      )}

      {/* 5. Similar Resources */}
      <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
        <h3 className="font-bold text-slate-900 text-[15px] mb-4">Similar Resources</h3>
        <div className="space-y-3">
          {mockedResources.map((resource, idx) => {
            const Icon = resource.icon
            return (
              <div key={idx} className="flex items-center gap-3 p-2 -mx-2 rounded-lg hover:bg-slate-50 transition-colors cursor-pointer">
                <div className="flex h-8 w-8 shrink-0 items-center justify-center rounded-md bg-slate-100 text-slate-600">
                  <Icon className="h-4 w-4" />
                </div>
                <div>
                  <h4 className="text-[13px] font-semibold text-slate-800 leading-tight">{resource.title}</h4>
                  <p className="text-[11px] text-slate-500 mt-0.5">{resource.type}</p>
                </div>
              </div>
            )
          })}
        </div>
      </div>

    </aside>
  )
}
