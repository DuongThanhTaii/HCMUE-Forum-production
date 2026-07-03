import { Link } from 'react-router-dom'
import { useTranslation } from 'react-i18next'
import type { ForumDetailItem } from '../../api/forum.list.api'
import { useGetPopularForumTagsQuery, useGetForumListQuery } from '../../api/forum.list.api'

interface RightSidebarProps {
  post: Partial<ForumDetailItem>
  authorLine: string | null
}

export function RightSidebar({ post, authorLine }: RightSidebarProps) {
  const { t } = useTranslation()
  const avatarLetter = (post.authorName || authorLine || 'U').charAt(0).toUpperCase()
  
  // Fetch popular tags
  const { data: popularTags = [] } = useGetPopularForumTagsQuery({ count: 8 })

  // Fetch related posts by category
  const { data: relatedPostsList, isLoading: isLoadingRelatedList } = useGetForumListQuery(
    { categoryId: post.categoryId, pageSize: 6 },
    { skip: !post.categoryId }
  )

  const filteredRelated = relatedPostsList?.filter(p => p.id !== post.id).slice(0, 5) || []

  return (
    <aside className="space-y-6 w-full lg:w-[340px] xl:w-[360px] shrink-0 sticky top-24 self-start hidden lg:block">
      
      {/* 1. Author Card */}
      <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
        <h3 className="font-bold text-slate-900 text-[16px] mb-4">Thông tin tác giả</h3>
        
        <div className="flex items-center gap-3 mb-4">
          <div className="flex-shrink-0">
            {post.authorAvatar ? (
              <img src={post.authorAvatar} alt={post.authorName} className="h-12 w-12 rounded-full object-cover shadow-sm" />
            ) : (
              <div className="flex h-12 w-12 select-none items-center justify-center rounded-full bg-indigo-600 text-white text-[20px] font-bold shadow-sm">
                {avatarLetter}
              </div>
            )}
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h4 className="font-bold text-slate-900 text-[15px]">{post.authorName || authorLine || 'Unknown Author'}</h4>
              <span className="bg-indigo-100 text-indigo-700 text-[10px] font-bold px-1.5 py-0.5 rounded">ADMIN</span>
            </div>
            <p className="text-[12px] text-slate-500 mt-0.5">Thành viên từ 2023</p>
          </div>
        </div>
        
        <div className="space-y-3 pt-4 border-t border-slate-100">
          <div className="flex items-center justify-between text-[13px]">
            <span className="text-slate-600 font-medium">Bài viết</span>
            <span className="font-bold text-slate-900">1</span>
          </div>
          <div className="flex items-center justify-between text-[13px]">
            <span className="text-slate-600 font-medium">Trả lời</span>
            <span className="font-bold text-slate-900">0</span>
          </div>
          <div className="flex items-center justify-between text-[13px]">
            <span className="text-slate-600 font-medium">Điểm uy tín</span>
            <span className="font-bold text-slate-900">{post.likeCount ?? post.voteScore ?? 0}</span>
          </div>
        </div>
      </div>

      {/* 2. Related Discussions */}
      <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
        <h3 className="font-bold text-slate-900 text-[16px] mb-4">{t('forum.rightSidebar.relatedDiscussions', 'Chủ đề liên quan')}</h3>
        
        {isLoadingRelatedList ? (
          <div className="space-y-4">
            {[1, 2, 3].map(i => (
              <div key={i} className="animate-pulse">
                <div className="h-4 w-full bg-slate-200 rounded"></div>
                <div className="h-3 w-1/2 bg-slate-100 rounded mt-2"></div>
              </div>
            ))}
          </div>
        ) : filteredRelated.length > 0 ? (
          <div className="space-y-4">
            {filteredRelated.map((item) => (
              <Link key={item.id} to={`/threads/${item.id}`} className="block group pb-1 last:pb-0">
                <h4 className="text-[14px] font-bold text-slate-700 leading-snug group-hover:text-primary transition-colors line-clamp-2">
                  {item.title}
                </h4>
                <div className="flex items-center gap-2 mt-1.5 text-[12px] text-slate-500">
                  <span>{item.replyCount ?? 0} {t('forum.rightSidebar.replies', 'trả lời').toLowerCase()}</span>
                </div>
              </Link>
            ))}
            <Link to={`/explore?category=${post.categoryId}`} className="w-full mt-2 py-2 bg-slate-50 text-slate-700 font-semibold text-[13px] rounded-lg hover:bg-slate-100 transition-colors text-center block">
              {t('forum.rightSidebar.seeAll', 'Xem tất cả')}
            </Link>
          </div>
        ) : (
          <p className="text-[13px] text-slate-500 text-center py-2">{t('forum.rightSidebar.noRelatedFound', 'Không có chủ đề liên quan')}</p>
        )}
      </div>

      {/* 3. Popular Tags */}
      <div className="bg-white rounded-xl p-5 border border-slate-200 shadow-sm">
        <h3 className="font-bold text-slate-900 text-[16px] mb-4">{t('forum.rightSidebar.popularTags', 'Thẻ phổ biến')}</h3>
        <div className="flex flex-wrap gap-2">
          {popularTags.map(tag => (
            <Link key={tag.name} to={`/explore?tag=${tag.name}`} className="inline-flex items-center gap-1.5 px-3 py-1.5 bg-indigo-50/60 border border-indigo-50/50 rounded-lg text-[13px] font-semibold text-indigo-700 hover:bg-indigo-100/50 transition-colors">
              #{tag.name} <span className="text-indigo-400 text-[12px] font-medium">{tag.postCount}</span>
            </Link>
          ))}
          {(!popularTags || popularTags.length === 0) && (
            <p className="text-[13px] text-slate-500">Đang cập nhật...</p>
          )}
        </div>
        <Link to="/explore" className="w-full mt-4 py-2 bg-transparent text-primary font-semibold text-[13px] hover:underline transition-colors text-center block">
          Xem tất cả thẻ
        </Link>
      </div>
    </aside>
  )
}
