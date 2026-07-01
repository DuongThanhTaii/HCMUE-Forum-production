import { Link } from 'react-router-dom';
import { MessageCircle, TrendingUp, Hash, BookOpen, Search } from 'lucide-react';
import { useGetForumCategoriesQuery, useGetPopularForumTagsQuery, useGetForumListQuery } from '../api/forum.list.api';
import { useTranslation } from 'react-i18next';

export function ExplorePage() {
  const { t } = useTranslation('forum');
  const { data: categories = [], isLoading: isLoadingCategories } = useGetForumCategoriesQuery();
  const { data: popularTags = [], isLoading: isLoadingTags } = useGetPopularForumTagsQuery();
  const { data: trendingPosts = [], isLoading: isLoadingTrending } = useGetForumListQuery({ pageNumber: 1, pageSize: 5 });

  return (
    <div className="max-w-6xl mx-auto px-4 py-8 space-y-10">
      {/* Header Area */}
      <div className="text-center space-y-4 max-w-2xl mx-auto">
        <h1 className="text-3xl md:text-4xl font-extrabold text-slate-900 tracking-tight">
          {t('explore.title')}
        </h1>
        <p className="text-lg text-slate-600">
          {t('explore.subtitle')}
        </p>
        <div className="relative max-w-xl mx-auto mt-6 shadow-sm group">
          <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none text-slate-400 group-focus-within:text-primary transition-colors">
            <Search className="h-5 w-5" />
          </div>
          <input
            type="text"
            className="block w-full pl-11 pr-4 py-3 border-transparent bg-white rounded-xl text-slate-900 placeholder-slate-400 focus:border-primary focus:ring-2 focus:ring-primary/20 sm:text-base outline-none transition-all shadow-sm border border-slate-200 hover:border-slate-300"
            placeholder={t('explore.searchPlaceholder')}
          />
        </div>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-3 gap-8">
        
        {/* Main Content Area */}
        <div className="lg:col-span-2 space-y-8">
          
          {/* Categories Grid */}
          <section>
            <div className="flex items-center gap-2 mb-6">
              <BookOpen className="h-5 w-5 text-primary" />
              <h2 className="text-xl font-bold text-slate-900">{t('explore.browseAreas')}</h2>
            </div>
            
            {isLoadingCategories ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {[1, 2, 3, 4].map(i => (
                  <div key={i} className="animate-pulse bg-white p-5 rounded-xl border border-slate-100 h-28"></div>
                ))}
              </div>
            ) : (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {categories.map((category) => (
                  <Link 
                    key={category.id}
                    to={`/discussions/${category.slug || category.id}`}
                    className="group flex flex-col justify-between bg-white p-5 rounded-xl border border-slate-200 shadow-sm hover:shadow-md hover:border-primary/50 transition-all duration-200 ease-out hover:-translate-y-1"
                  >
                    <div>
                      <h3 className="font-semibold text-lg text-slate-900 group-hover:text-primary mb-1">
                        {category.name}
                      </h3>
                      <p className="text-sm text-slate-500 line-clamp-2">
                        {category.description || 'Join the conversation'}
                      </p>
                    </div>
                    <div className="mt-4 flex items-center gap-4 text-xs font-medium text-slate-500">
                      <div className="flex items-center gap-1.5 bg-slate-50 px-2 py-1 rounded-md">
                        <MessageCircle className="h-3.5 w-3.5" />
                        <span>{t('explore.topicsCount', { count: category.postCount })}</span>
                      </div>
                    </div>
                  </Link>
                ))}
              </div>
            )}
          </section>
        </div>

        {/* Sidebar */}
        <div className="space-y-8">
          {/* Trending Now */}
          <section className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
            <div className="flex items-center gap-2 mb-5">
              <TrendingUp className="h-5 w-5 text-orange-500" />
              <h2 className="font-bold text-slate-900">{t('explore.trending')}</h2>
            </div>
            <div className="space-y-4">
              {isLoadingTrending ? (
                <div className="space-y-4">
                  {[1, 2, 3].map((i) => (
                    <div key={i} className="animate-pulse flex items-start gap-3">
                      <div className="h-6 w-6 bg-slate-100 rounded-md"></div>
                      <div className="flex-1 space-y-2">
                        <div className="h-4 bg-slate-100 rounded w-full"></div>
                        <div className="h-3 bg-slate-100 rounded w-1/2"></div>
                      </div>
                    </div>
                  ))}
                </div>
              ) : trendingPosts.length === 0 ? (
                <div className="text-sm text-slate-500">No trending discussions found.</div>
              ) : (
                trendingPosts.slice(0, 5).map((post, index) => (
                  <Link to={`/threads/${post.id}`} key={post.id} className="flex items-start gap-3 group cursor-pointer no-underline">
                    <span className="text-slate-300 font-bold text-lg">{(index + 1).toString().padStart(2, '0')}</span>
                    <div>
                      <h4 className="font-medium text-slate-800 text-sm group-hover:text-primary line-clamp-2">{post.title}</h4>
                      <span className="text-xs text-slate-500">{post.replyCount} replies • {post.viewCount} views</span>
                    </div>
                  </Link>
                ))
              )}
            </div>
          </section>

          {/* Popular Tags */}
          <section className="bg-white p-6 rounded-xl border border-slate-200 shadow-sm">
            <div className="flex items-center gap-2 mb-5">
              <Hash className="h-5 w-5 text-indigo-500" />
              <h2 className="font-bold text-slate-900">{t('explore.popularTags')}</h2>
            </div>
            {isLoadingTags ? (
              <div className="flex flex-wrap gap-2">
                {[1, 2, 3, 4, 5].map(i => (
                  <div key={i} className="animate-pulse bg-slate-100 rounded-full h-8 w-16"></div>
                ))}
              </div>
            ) : (
              <div className="flex flex-wrap gap-2">
                {popularTags.slice(0, 15).map(tag => (
                  <span 
                    key={tag.name} 
                    className="inline-flex items-center px-3 py-1.5 rounded-full text-xs font-medium bg-indigo-50 text-indigo-700 hover:bg-indigo-100 cursor-pointer transition-colors border border-indigo-100"
                  >
                    #{tag.name}
                    <span className="ml-1.5 text-indigo-400 font-normal">{tag.postCount}</span>
                  </span>
                ))}
              </div>
            )}
          </section>

        </div>
      </div>
    </div>
  );
}
