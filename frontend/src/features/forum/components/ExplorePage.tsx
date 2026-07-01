import { Link } from 'react-router-dom';
import { MessageCircle, TrendingUp, Hash, BookOpen, ArrowRight, Clock, ChevronRight, MessageSquarePlus } from 'lucide-react';
import { useGetForumCategoriesQuery, useGetPopularForumTagsQuery, useGetForumListQuery } from '../api/forum.list.api';
import { useTranslation } from 'react-i18next';

export function ExplorePage() {
  const { t } = useTranslation();
  const { data: categories = [], isLoading: isLoadingCategories } = useGetForumCategoriesQuery();
  const { data: popularTags = [], isLoading: isLoadingTags } = useGetPopularForumTagsQuery();
  const { data: recentPosts = [], isLoading: isLoadingRecent } = useGetForumListQuery({ pageNumber: 1, pageSize: 5 });
  const { data: trendingPosts = [], isLoading: isLoadingTrending } = useGetForumListQuery({ pageNumber: 1, pageSize: 5 });

  const featuredCategories = categories.slice(0, 4);
  const popularCategories = categories.slice(4);

  return (
    <div className="max-w-7xl mx-auto px-4 py-8 space-y-10">
      
      <div className="flex items-center justify-between">
        <h1 className="text-3xl font-extrabold text-slate-900">{t('forum.explore.title')}</h1>
        <Link 
          to="/forum/new" 
          className="inline-flex items-center gap-2 bg-primary text-white px-4 py-2.5 rounded-xl font-medium shadow-sm hover:bg-primary/90 transition-colors"
        >
          <MessageSquarePlus className="h-5 w-5" />
          {t('forum.createPost.title', 'Đăng bài mới')}
        </Link>
      </div>

      {/* MAIN CONTENT GRID */}
      <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
        
        {/* LEFT COLUMN (70%) */}
        <div className="lg:col-span-8 space-y-12">
          
          {/* FEATURED CATEGORIES */}
          <section>
            <div className="flex items-center justify-between mb-6">
              <h2 className="text-2xl font-bold text-slate-900">{t('forum.explore.featuredCategories')}</h2>
            </div>
            {isLoadingCategories ? (
               <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  {[1, 2, 3, 4].map(i => (
                    <div key={i} className="animate-pulse bg-white p-6 rounded-2xl border border-slate-100 h-48"></div>
                  ))}
               </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                {featuredCategories.map(cat => (
                  <Link to={`/discussions/${cat.slug}`} key={cat.id} className="group block bg-white p-6 rounded-2xl border border-slate-200 shadow-sm hover:shadow-md hover:border-primary/40 transition-all duration-300 ease-out hover:-translate-y-1 flex flex-col h-full">
                    <div className="h-12 w-12 bg-primary/10 text-primary rounded-xl flex items-center justify-center mb-5 group-hover:scale-110 transition-transform">
                      <BookOpen className="h-6 w-6" />
                    </div>
                    <h3 className="text-xl font-bold text-slate-900 mb-2 group-hover:text-primary transition-colors">{cat.name}</h3>
                    <p className="text-slate-500 mb-6 line-clamp-2 text-sm flex-1">{cat.description || 'Discuss and share with the community'}</p>
                    <div className="flex items-center justify-between mt-auto pt-4 border-t border-slate-100">
                      <span className="text-sm font-medium text-slate-600 flex items-center gap-1.5">
                        <MessageCircle className="h-4 w-4" />
                        {t('forum.explore.discussionsCount', { count: cat.postCount })}
                      </span>
                      <span className="text-primary font-semibold flex items-center gap-1 group-hover:gap-2 transition-all text-sm">
                        Explore <ArrowRight className="h-4 w-4" />
                      </span>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </section>

          {/* RECENTLY ACTIVE */}
          <section>
            <h2 className="text-2xl font-bold text-slate-900 mb-6">{t('forum.explore.recentlyActive')}</h2>
            <div className="bg-white rounded-2xl border border-slate-200 shadow-sm divide-y divide-slate-100 overflow-hidden">
               {isLoadingRecent ? (
                 <div className="p-5 animate-pulse bg-slate-50 h-24"></div>
               ) : recentPosts.length === 0 ? (
                 <div className="p-8 text-center text-slate-500">No recent activity found.</div>
               ) : (
                 recentPosts.map(post => (
                   <Link to={`/threads/${post.id}`} key={post.id} className="block p-5 hover:bg-slate-50 transition-colors group">
                     <h4 className="font-semibold text-slate-900 group-hover:text-primary mb-2 text-lg">{post.title}</h4>
                     <div className="flex items-center gap-4 text-xs font-medium text-slate-500">
                       <span className="flex items-center gap-1.5"><Clock className="h-4 w-4"/> {new Date(post.activityAt || '').toLocaleDateString()}</span>
                       <span className="flex items-center gap-1.5"><div className="w-4 h-4 rounded-full bg-slate-200 overflow-hidden">{post.authorAvatar && <img src={post.authorAvatar} alt="" className="w-full h-full object-cover" />}</div> {post.authorName || 'User'}</span>
                       <span className="bg-slate-100 px-2 py-1 rounded-md text-slate-600">{post.category}</span>
                     </div>
                   </Link>
                 ))
               )}
            </div>
          </section>

          {/* POPULAR CATEGORIES */}
          {popularCategories.length > 0 && (
            <section>
              <h2 className="text-2xl font-bold text-slate-900 mb-6">{t('forum.explore.browseAreas')}</h2>
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                 {popularCategories.map(cat => (
                   <Link to={`/discussions/${cat.slug}`} key={cat.id} className="flex items-center gap-4 p-4 bg-white rounded-xl border border-slate-200 hover:border-primary/40 hover:shadow-md transition-all group">
                     <div className="flex-1 min-w-0">
                       <h4 className="font-semibold text-slate-900 group-hover:text-primary truncate mb-0.5">{cat.name}</h4>
                       <p className="text-xs text-slate-500">{t('forum.explore.topicsCount', {count: cat.postCount})}</p>
                     </div>
                     <ChevronRight className="h-5 w-5 text-slate-300 group-hover:text-primary transition-colors" />
                   </Link>
                 ))}
              </div>
            </section>
          )}

        </div>

        {/* RIGHT COLUMN (30%) */}
        <div className="lg:col-span-4 space-y-8">
          
          {/* TRENDING WIDGET */}
          <section className="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
            <div className="flex items-center justify-between mb-6">
              <div className="flex items-center gap-2">
                <TrendingUp className="h-5 w-5 text-orange-500" />
                <h2 className="font-bold text-slate-900 text-lg">{t('forum.explore.trending')}</h2>
              </div>
            </div>
            <div className="space-y-5">
               {isLoadingTrending ? (
                 <div className="animate-pulse bg-slate-100 h-16 rounded-lg w-full"></div>
               ) : trendingPosts.length === 0 ? (
                 <div className="text-sm text-slate-500">No trending discussions found.</div>
               ) : (
                 trendingPosts.slice(0, 5).map((post, index) => (
                   <Link to={`/threads/${post.id}`} key={post.id} className="flex items-start gap-4 group">
                     <span className="text-slate-300 font-bold text-xl mt-0.5 w-6 text-center">{(index + 1).toString()}</span>
                     <div className="flex-1 min-w-0">
                       <h4 className="font-medium text-slate-800 text-sm group-hover:text-primary line-clamp-2 leading-snug mb-1.5">{post.title}</h4>
                       <span className="text-xs font-medium text-slate-500">{post.replyCount} replies • {post.viewCount} views</span>
                     </div>
                   </Link>
                 ))
               )}
            </div>
            <button className="w-full mt-6 py-2.5 bg-slate-50 text-slate-700 font-medium rounded-xl text-sm hover:bg-slate-100 transition-colors border border-slate-200">
              {t('forum.explore.seeAll')}
            </button>
          </section>

          {/* POPULAR TAGS WIDGET */}
          <section className="bg-white p-6 rounded-2xl border border-slate-200 shadow-sm">
            <div className="flex items-center gap-2 mb-6">
              <Hash className="h-5 w-5 text-indigo-500" />
              <h2 className="font-bold text-slate-900 text-lg">{t('forum.explore.popularTags')}</h2>
            </div>
            <div className="flex flex-col gap-2">
              {isLoadingTags ? (
                <div className="animate-pulse bg-slate-100 h-10 rounded-lg w-full"></div>
              ) : (
                popularTags.slice(0, 10).map(tag => (
                  <Link to={`/discussions/all?search=%23${tag.name}`} key={tag.name} className="flex items-center justify-between p-3 rounded-xl hover:bg-indigo-50 group transition-all">
                    <span className="font-semibold text-slate-700 group-hover:text-indigo-700 transition-colors">#{tag.name}</span>
                    <span className="text-[11px] text-slate-500 font-medium bg-slate-100 group-hover:bg-indigo-100/80 px-2.5 py-1 rounded-md transition-colors">
                      {t('forum.explore.discussionsCount', { count: tag.postCount })}
                    </span>
                  </Link>
                ))
              )}
            </div>
          </section>

        </div>
      </div>
    </div>
  );
}
